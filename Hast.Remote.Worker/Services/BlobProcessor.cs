using Hast.Common.Services;
using Hast.Layer;
using Hast.Remote.Bridge.Models;
using Hast.Remote.Worker.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Services
{
    public class BlobProcessor : IBlobProcessor
    {
        private readonly CloudBlobContainer _container;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IApplicationInsightsTelemetryManager _applicationInsightsTelemetryManager;
        private readonly IClock _clock;
        private readonly IHastlayer _hastlayer;
        private readonly IJsonConverter _jsonConverter;
        private readonly ILogger<BlobProcessor> _logger;

        public BlobProcessor(
            CloudBlobContainer container,
            IAppDataFolder appDataFolder,
            IApplicationInsightsTelemetryManager applicationInsightsTelemetryManager,
            IClock clock,
            IHastlayer hastlayer,
            IJsonConverter jsonConverter,
            ILogger<BlobProcessor> logger)
        {
            _container = container;
            _appDataFolder = appDataFolder;
            _applicationInsightsTelemetryManager = applicationInsightsTelemetryManager;
            _clock = clock;
            _hastlayer = hastlayer;
            _jsonConverter = jsonConverter;
            _logger = logger;
        }

        public async Task StartJobAsync(CloudBlockBlob blob, CancellationToken cancellationToken = default)
        {
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

                    var leaseId = await blob.AcquireLeaseAsync(leaseTimeSpan, proposedLeaseId: null, cancellationToken);
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

                telemetry.IsSuccess = await TryProcessAsync(
                    leaseTimeSpan,
                    accessCondition,
                    telemetry,
                    blob,
                    cancellationToken);
                if (!telemetry.IsSuccess) return;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Processing the job blob {0} failed.", blob.Name);
            }

            telemetry.FinishTimeUtc = _clock.UtcNow;
            _applicationInsightsTelemetryManager.TrackTransformation(telemetry);
        }

        private async Task<bool> TryProcessAsync(
            TimeSpan leaseTimeSpan,
            AccessCondition accessCondition,
            TransformationTelemetry telemetry,
            CloudBlockBlob blob,
            CancellationToken cancellationToken)
        {
            var processingTask = ProcessAsync(accessCondition, telemetry, blob, cancellationToken);

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
                    if (HasHttpStatus(ex, HttpStatusCode.NotFound)) return false;

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
            return true;
        }

        private async Task ProcessAsync(
            AccessCondition accessCondition,
            TransformationTelemetry telemetry,
            CloudBlob blob,
            CancellationToken cancellationToken)
        {
            try
            {
                TransformationJob job;

                // The StreamReader disposes the stream too.
                using (var streamReader = new StreamReader(await blob.OpenReadAsync(null, null, null, cancellationToken)))
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

                    var result = new TransformationJobResult
                    {
                        RemoteHastlayerVersion = GetType().Assembly.GetName().Version?.ToString() ?? "0.0",
                        Token = job.Token,
                        AppId = job.AppId,
                    };

                    try
                    {
                        var hardwareGenerationConfiguration =
                            new Layer.HardwareGenerationConfiguration(
                                job.Configuration.DeviceName,
                                null,
                                job.Configuration.CustomConfiguration,
                                job.Configuration.HardwareEntryPointMemberFullNames,
                                job.Configuration.HardwareEntryPointMemberNamePrefixes)
                            {
                                EnableCaching = true,
                                EnableHardwareImplementationComposition = false,
                            };
                        var hardwareRepresentation = await _hastlayer
                            .GenerateHardwareAsync(assemblyPaths, hardwareGenerationConfiguration);

                        cancellationToken.ThrowIfCancellationRequested();

                        await using var memoryStream = new MemoryStream();
                        await hardwareRepresentation.HardwareDescription.SerializeAsync(memoryStream);
                        result.HardwareDescription = new HardwareDescription
                        {
                            Language = hardwareRepresentation.HardwareDescription.Language,
                            SerializedHardwareDescription = Encoding.UTF8.GetString(memoryStream.ToArray()),
                        };
                    }
                    catch (Exception ex) when (!ex.IsFatal() && !(ex is OperationCanceledException))
                    {
                        result.Errors = new[] { ex.WithoutStackTrace() };
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var resultBlob = _container.GetBlockBlobReference("results/" + job.Token);

                    await using (var streamWriter = new StreamWriter(
                        await resultBlob.OpenWriteAsync(null, null, null, cancellationToken)))
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

        private static bool HasHttpStatus(StorageException exception, HttpStatusCode statusCode) =>
            ((exception.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode == statusCode;
    }
}
