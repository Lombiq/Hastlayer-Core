using Hast.Common.Services;
using Hast.Layer;
using Hast.Remote.Bridge.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HardwareGenerationConfiguration = Hast.Layer.HardwareGenerationConfiguration;
using Timer = System.Timers.Timer;

namespace Hast.Remote.Worker.Services
{
    public sealed class TransformationWorker : ITransformationWorker, IDisposable
    {
        private readonly IJsonConverter _jsonConverter;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;
        private readonly IApplicationInsightsTelemetryManager _applicationInsightsTelemetryManager;
        private readonly ILogger _logger;
        private readonly IHastlayer _hastlayer;
        private readonly CloudBlobContainer _container;
        private readonly TelemetryClient _telemetryClient;
        private readonly ConcurrentDictionary<string, Task> _transformationTasks = new();

        private int _restartCount;
        private Timer _oldResultBlobsCleanerTimer;


        public TransformationWorker(
            IJsonConverter jsonConverter,
            IAppDataFolder appDataFolder,
            IClock clock,
            IApplicationInsightsTelemetryManager applicationInsightsTelemetryManager,
            ILogger<TransformationWorker> logger,
            IHastlayer hastlayer,
            CloudBlobContainer container,
            TelemetryClient telemetryClient)
        {
            _jsonConverter = jsonConverter;
            _appDataFolder = appDataFolder;
            _clock = clock;
            _applicationInsightsTelemetryManager = applicationInsightsTelemetryManager;
            _logger = logger;
            _hastlayer = hastlayer;
            _container = container;
            _telemetryClient = telemetryClient;

            _oldResultBlobsCleanerTimer = new Timer(TimeSpan.FromHours(3).TotalMilliseconds);
            _oldResultBlobsCleanerTimer.Elapsed += async (_, _) =>
            {
                try
                {
                    // Removing those result blobs that weren't deleted somehow (like the client exited while waiting
                    // for the result to appear, thus never requesting it hence it never getting deleted).
                    var oldResultBlobs = (await GetBlobs(container, "results/"))
                        .Cast<CloudBlockBlob>()
                        .Where(blob => blob.Properties.LastModified < _clock.UtcNow.AddHours(-1));

                    foreach (var blob in oldResultBlobs)
                    {
                        await blob.DeleteAsync();
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    // If an exception escapes here it'll take down the whole process, so need to handle them.
                    _logger.LogError(ex, "Error during cleaning up old result blobs.");
                }
            };
            _oldResultBlobsCleanerTimer.Enabled = true;
        }


        public async Task WorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    _telemetryClient.Flush();
                    var jobBlobs = (await GetBlobs(_container, "jobs/"))
                        .Where(blob => !blob.StorageUri.PrimaryUri.ToString().Contains("$$$ORCHARD$$$.$$$"))
                        .Cast<CloudBlockBlob>()
                        .Where(blob => blob.Properties.LeaseStatus == LeaseStatus.Unlocked && blob.Properties.Length != 0);

                    foreach (var jobBlob in jobBlobs)
                    {

                        cancellationToken.ThrowIfCancellationRequested();

                        _transformationTasks[jobBlob.Name] = Task.Factory.StartNew(
                            blobObject => WorkInnerAsync(blobObject, cancellationToken),
                            jobBlob,
                            cancellationToken)
                            .ContinueWith(
                                (task, blobNameObject) => _transformationTasks.TryRemove((string)blobNameObject, out _),
                                jobBlob.Name,
                                cancellationToken);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // Waiting a bit between cycles not to have excessive Blob Storage usage due to polling (otherwise
                    // it's not an issue, this loop barely uses any CPU).
                    await Task.Delay(1000, cancellationToken);
                    _restartCount = 0;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Cancelling {_transformationTasks.Count} tasks.");
                await Task.WhenAll(_transformationTasks.Values);
                Dispose();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                if (_restartCount < 100)
                {
                    _logger.LogError(ex, "Transformation Worker crashed with an unhandled exception. Restarting...");

                    Dispose();
                    _restartCount++;

                    // Waiting a bit for transient errors to go away.
                    await Task.Delay(10000, cancellationToken);

                    await WorkAsync(cancellationToken);
                }
                else
                {
                    _logger.LogCritical(
                        ex,
                        "Transformation Worker crashed with an unhandled exception and was restarted " +
                        _restartCount + " times. It won't be restarted again.");
                    _telemetryClient.Flush();
                }
            }
        }

        private async Task WorkInnerAsync(object blobObject, CancellationToken cancellationToken)
        {
            var blob = (CloudBlockBlob)blobObject;
            var telemetry = new TransformationTelemetry
            {
                JobName = blob.Name,
                StartTimeUtc = _clock.UtcNow,
            };

            try
            {
                var leaseTimeSpan = TimeSpan.FromSeconds(15);
                AccessCondition accessCondition;
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

                var processingTask = Task.Run(
                    () => ProcessTaskAsync(blob, telemetry, accessCondition, cancellationToken),
                    cancellationToken);

                // Repeatedly renewing the lease until the processing completes.
                while (!processingTask.IsCompleted)
                {
                    try
                    {
                        await blob.RenewLeaseAsync(accessCondition, cancellationToken);
                    }
                    catch (StorageException ex)
                    {
                        // If in the meantime the Task finished and removed the blob then nothing to do.
                        if (HasHttpStatus(ex, HttpStatusCode.NotFound)) return;

                        throw;
                    }

                    // Task.Delay() waits for 1 second less so the lease is renewed for sure before it
                    // expires.
                    await Task.WhenAny(
                        processingTask,
                        Task.Delay(leaseTimeSpan - TimeSpan.FromSeconds(1), cancellationToken));
                }

                // This is so if there was an exception in the Task that will be thrown with its original
                // stack trace. Otherwise Task.WhenAny() isn't throwing exceptions from its arguments.
                await processingTask;
                telemetry.IsSuccess = true;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Processing the job blob {0} failed.", blob.Name);
            }

            telemetry.FinishTimeUtc = _clock.UtcNow;
            _applicationInsightsTelemetryManager.TrackTransformation(telemetry);
        }

        private async Task ProcessTaskAsync(
            CloudBlob blob,
            TransformationTelemetry telemetry,
            AccessCondition accessCondition,
            CancellationToken cancellationToken)
        {
            try
            {
                TransformationJob job;

                await using (var stream = await blob.OpenReadAsync(null, null, null, cancellationToken))
                using (var streamReader = new StreamReader(stream))
                {
                    job = _jsonConverter.Deserialize<TransformationJob>(await streamReader.ReadToEndAsync());
                }

                telemetry.AppId = job.AppId;
                var jobFolder = _appDataFolder.Combine("Hastlayer", "RemoteWorker", job.Token);

                var assemblyPaths = new List<string>();

                try
                {
                    foreach (var assembly in job.Assemblies)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var path = _appDataFolder.Combine(jobFolder, assembly.Name + ".dll");

                        assemblyPaths.Add(_appDataFolder.MapPath(path));

                        await using var memoryStream = new MemoryStream(assembly.FileContent);
                        await using var fileStream = _appDataFolder.CreateFile(path);
                        await memoryStream.CopyToAsync(fileStream, cancellationToken);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var result = new TransformationJobResult { RemoteHastlayerVersion = GetType().Assembly.GetName().Version?.ToString(), Token = job.Token, AppId = job.AppId, };

                    IHardwareRepresentation hardwareRepresentation;
                    try
                    {
                        hardwareRepresentation = await _hastlayer.GenerateHardware(assemblyPaths, new HardwareGenerationConfiguration(job.Configuration.DeviceName, null)
                        {
                            CustomConfiguration = job.Configuration.CustomConfiguration,
                            EnableCaching = true,
                            HardwareEntryPointMemberFullNames = job.Configuration.HardwareEntryPointMemberFullNames,
                            HardwareEntryPointMemberNamePrefixes = job.Configuration.HardwareEntryPointMemberNamePrefixes,
                            EnableHardwareImplementationComposition = false,
                        });

                        cancellationToken.ThrowIfCancellationRequested();

                        await using var memoryStream = new MemoryStream();
                        await hardwareRepresentation.HardwareDescription.Serialize(memoryStream);
                        result.HardwareDescription = new HardwareDescription { Language = hardwareRepresentation.HardwareDescription.Language, SerializedHardwareDescription = Encoding.UTF8.GetString(memoryStream.ToArray()), };
                    }
                    catch (Exception ex) when (!ex.IsFatal() && !(ex is OperationCanceledException))
                    {
                        // We don't want to show the stack trace to the user, just exception
                        // message, so building one by iterating all the nested exceptions.

                        var currentException = ex;
                        var message = string.Empty;

                        while (currentException != null)
                        {
                            message += currentException.Message + Environment.NewLine;
                            currentException = currentException.InnerException;
                        }

                        result.Errors = new[]
                        {
                            message,
                        };
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var resultBlob = _container.GetBlockBlobReference("results/" + job.Token);

                    await using (var blobStream = await resultBlob.OpenWriteAsync(null, null, null, cancellationToken))
                    await using (var streamWriter = new StreamWriter(blobStream))
                    {
                        await streamWriter.WriteAsync(_jsonConverter.Serialize(result));
                    }

                    await blob.DeleteAsync(
                        DeleteSnapshotsOption.None,
                        accessCondition,
                        new BlobRequestOptions(),
                        new OperationContext(),
                        cancellationToken);
                }
                finally
                {
                    foreach (var assemblyPath in assemblyPaths)
                    {
                        _appDataFolder.DeleteFile(assemblyPath);
                    }

                    _appDataFolder.DeleteFile(jobFolder);
                }
            }
            catch
            {
                await blob.ReleaseLeaseAsync(accessCondition, cancellationToken);
                throw;
            }
        }

        public void Dispose()
        {
            if (_oldResultBlobsCleanerTimer != null)
            {
                _oldResultBlobsCleanerTimer?.Stop();
                _oldResultBlobsCleanerTimer?.Dispose();
                _oldResultBlobsCleanerTimer = null;
                _transformationTasks.Clear();
            }

            _telemetryClient.Flush();
            Task.Delay(10000).Wait();
        }

        private static async Task<List<IListBlobItem>> GetBlobs(CloudBlobContainer container, string prefix)
        {
            var segment = await container.ListBlobsSegmentedAsync(prefix, null);
            var list = new List<IListBlobItem>();
            list.AddRange(segment.Results);
            while (segment.ContinuationToken != null)
            {
                segment = await container.ListBlobsSegmentedAsync(prefix, segment.ContinuationToken);
                list.AddRange(segment.Results);
            }
            return list;
        }

        private static bool HasHttpStatus(StorageException exception, HttpStatusCode statusCode) =>
            ((exception.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode == statusCode;

        private class TransformationTelemetry : ITransformationTelemetry
        {
            public string JobName { get; set; }
            public int AppId { get; set; }
            public DateTime StartTimeUtc { get; set; }
            public DateTime FinishTimeUtc { get; set; }
            public bool IsSuccess { get; set; }
        }
    }
}