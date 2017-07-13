﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Remote.Bridge.Models;
using Hast.Remote.Worker.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Orchard;
using Orchard.Exceptions;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.Services;

namespace Hast.Remote.Worker
{
    public class TransformationWorker : ITransformationWorker, IDisposable
    {
        private readonly IJsonConverter _jsonConverter;
        private readonly IAppDataFolder _appDataFolder;

        private IHastlayer _hastlayer;
        private CloudBlobContainer _container;
        private ConcurrentDictionary<string, Task> _transformationTasks = new ConcurrentDictionary<string, Task>();
        private int _restartCount = 0;

        public ILogger Logger { get; set; }


        public TransformationWorker(IJsonConverter jsonConverter, IAppDataFolder appDataFolder)
        {
            _jsonConverter = jsonConverter;
            _appDataFolder = appDataFolder;

            Logger = NullLogger.Instance;
        }


        public async Task Work(ITransformationWorkerConfiguration configuration, CancellationToken cancellationToken)
        {
            try
            {
                if (_hastlayer == null)
                {
                    _hastlayer = await Hastlayer.Create(new HastlayerConfiguration { Flavor = HastlayerFlavor.Developer });

                    cancellationToken.ThrowIfCancellationRequested();

                    _container = CloudStorageAccount
                        .Parse(configuration.StorageConnectionString)
                        .CreateCloudBlobClient()
                        .GetContainerReference("transformation");

                    await _container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null);
                }

                cancellationToken.ThrowIfCancellationRequested();

                while (true)
                {
                    var jobBlobs = _container
                        .ListBlobs("jobs/")
                        .Where(blob => !blob.StorageUri.PrimaryUri.ToString().Contains("$$$ORCHARD$$$.$$$"))
                        .Cast<CloudBlockBlob>()
                        .Where(blob => blob.Properties.LeaseStatus == LeaseStatus.Unlocked && blob.Properties.Length != 0);

                    foreach (var jobBlob in jobBlobs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        _transformationTasks[jobBlob.Name] = Task.Factory.StartNew(async blobObject =>
                        {
                            var blob = (CloudBlockBlob)blobObject;

                            try
                            {
                                var leaseTimeSpan = TimeSpan.FromSeconds(15);
                                AccessCondition accessCondition = null;
                                try
                                {
                                    // If in the short time between the blob listing and this line some other Task
                                    // started to work then nothing to do.
                                    if (blob.Properties.LeaseStatus != LeaseStatus.Unlocked) return;

                                    var leaseId = await blob.AcquireLeaseAsync(leaseTimeSpan);
                                    accessCondition = new AccessCondition { LeaseId = leaseId };
                                }
                                catch (StorageException ex)
                                {
                                    // If the lease was already acquired or some other Task deleted even finished it
                                    // then we get these exceptions, nothing to do.
                                    if (HasHttpStatus(ex, HttpStatusCode.Conflict) ||
                                        HasHttpStatus(ex, HttpStatusCode.NotFound))
                                    {
                                        return;
                                    }

                                    throw;
                                }

                                var processingTask = Task.Run(async () =>
                                {
                                    try
                                    {
                                        TransformationJob job;

                                        using (var stream = await blob.OpenReadAsync(cancellationToken))
                                        using (var streamReader = new StreamReader(stream))
                                        {
                                            job = _jsonConverter
                                                .Deserialize<TransformationJob>(await streamReader.ReadToEndAsync());
                                        }

                                        var jobFolder = _appDataFolder.Combine("Hastlayer", "RemoteWorker", job.Token);

                                        var assemblyPaths = new List<string>();
                                        foreach (var assembly in job.Assemblies)
                                        {
                                            cancellationToken.ThrowIfCancellationRequested();

                                            var path = _appDataFolder.Combine(jobFolder, assembly.Id);

                                            assemblyPaths.Add(_appDataFolder.MapPath(path));

                                            using (var memoryStream = new MemoryStream(assembly.FileContent))
                                            using (var fileStream = _appDataFolder.CreateFile(path))
                                            {
                                                await memoryStream.CopyToAsync(fileStream);
                                            }
                                        }

                                        cancellationToken.ThrowIfCancellationRequested();

                                        var result = new TransformationJobResult
                                        {
                                            Token = job.Token,
                                            UserId = job.UserId
                                        };

                                        IHardwareRepresentation hardwareRepresentation;
                                        try
                                        {
                                            hardwareRepresentation = await _hastlayer.GenerateHardware(
                                                assemblyPaths,
                                                new Layer.HardwareGenerationConfiguration(job.Configuration.DeviceName)
                                                {
                                                    CustomConfiguration = job.Configuration.CustomConfiguration,
                                                    HardwareEntryPointMemberFullNames = job.Configuration.HardwareEntryPointMemberFullNames,
                                                    HardwareEntryPointMemberNamePrefixes = job.Configuration.HardwareEntryPointMemberNamePrefixes
                                                });

                                            cancellationToken.ThrowIfCancellationRequested();

                                            var hardwareDescription = hardwareRepresentation.HardwareDescription;

                                            result.HardwareDescription = new HardwareDescription
                                            {
                                                HardwareEntryPointNamesToMemberIdMappings = hardwareDescription
                                                    .HardwareEntryPointNamesToMemberIdMappings
                                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                                Language = hardwareDescription.Language,
                                            };

                                            using (var memoryStream = new MemoryStream())
                                            {
                                                await hardwareDescription.WriteSource(memoryStream);
                                                result.HardwareDescription.Source = Encoding.UTF8.GetString(memoryStream.ToArray());
                                            }
                                        }
                                        catch (Exception ex) when (!ex.IsFatal() && !(ex is OperationCanceledException))
                                        {
                                            result.Errors = new[] { ex.ToString() };
                                        }

                                        cancellationToken.ThrowIfCancellationRequested();

                                        var resultBlob = _container.GetBlockBlobReference("results/" + job.Token);

                                        using (var blobStream = await resultBlob.OpenWriteAsync(cancellationToken))
                                        using (var streamWriter = new StreamWriter(blobStream))
                                        {
                                            await streamWriter.WriteAsync(_jsonConverter.Serialize(result));
                                        }

                                        await blob.DeleteAsync(
                                            DeleteSnapshotsOption.None,
                                            accessCondition,
                                            new BlobRequestOptions(),
                                            new OperationContext());
                                    }
                                    catch
                                    {
                                        await blob.ReleaseLeaseAsync(accessCondition);
                                        throw;
                                    }
                                });

                                // Repeatedly renewing the lease until the processing completes.
                                while (!processingTask.IsCompleted)
                                {
                                    try
                                    {
                                        await blob.RenewLeaseAsync(accessCondition);
                                    }
                                    catch (StorageException ex)
                                    {
                                        // If in the meantime the Task finished and removed the blob then nothing to do.
                                        if (HasHttpStatus(ex, HttpStatusCode.NotFound)) return;

                                        throw;
                                    }

                                    // Task.Delay() waits for 1 second less so the lease is renewed for sure before it
                                    // expires.
                                    await Task.WhenAny(processingTask, Task.Delay(leaseTimeSpan - TimeSpan.FromSeconds(1)));
                                }

                                // This is so if there was an exception in the Task that will be thrown with its original
                                // stack trace. Otherwise Task.WhenAny() isn't throwing exceptions from its arguments.
                                await processingTask;
                            }
                            catch (Exception ex) when (!ex.IsFatal())
                            {
                                Logger.Error(ex, "Processing the job blob {0} failed.", blob.Name);
                            }
                        }, jobBlob, cancellationToken)
                        .ContinueWith((task, blobNameObject) =>
                        {
                            Task dummy;
                            _transformationTasks.TryRemove((string)blobNameObject, out dummy);
                        }, jobBlob.Name);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // Waiting a bit between cycles not to have access Blob Storage usage due to polling (otherwise it's
                    // not an issue, this loop barely uses any CPU).
                    await Task.Delay(500);
                    _restartCount = 0;
                }
            }
            catch (OperationCanceledException)
            {
                await Task.WhenAll(_transformationTasks.Values);
                Dispose();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                if (_restartCount >= 10)
                {
                    Logger.Error(ex, "Transformation Worker crashed with an unhandled exception. Restarting...");
                    Dispose();
                    _restartCount++;
                    await Work(configuration, cancellationToken);
                }
                else
                {
                    Logger.Fatal(
                        ex,
                        "Transformation Worker crashed with an unhandled exception and was restarted " +
                        _restartCount + " times. It won't be restarted again.");
                }
            }
        }

        public void Dispose()
        {
            if (_hastlayer == null) return;

            _hastlayer.Dispose();
            _hastlayer = null;
            _transformationTasks.Clear();
        }


        private static bool HasHttpStatus(StorageException exception, HttpStatusCode statusCode) =>
            ((exception.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode == statusCode;
    }
}
