using Hast.Catapult;
using Hast.Common.Services;
using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Remote.Worker.Services;
using Hast.Synthesis.Services;
using Hast.Transformer;
using Hast.Transformer.Vhdl.Services;
using Hast.Xilinx;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Timer = System.Timers.Timer;

namespace Hast.Remote.Worker
{
    public sealed class TransformationWorker : ITransformationWorker, IDisposable
    {
        private readonly IBlobProcessor _blobProcessor;
        private readonly ILogger _logger;
        private readonly CloudBlobContainer _container;
        private readonly TelemetryClient _telemetryClient;
        private readonly ConcurrentDictionary<string, Task> _transformationTasks = new();

        private int _restartCount;
        private Timer _oldResultBlobsCleanerTimer;

        public TransformationWorker(
            IBlobProcessor blobProcessor,
            IClock clock,
            ILogger<TransformationWorker> logger,
            CloudBlobContainer container,
            TelemetryClient telemetryClient)
        {
            _blobProcessor = blobProcessor;
            _logger = logger;
            _container = container;
            _telemetryClient = telemetryClient;

            _oldResultBlobsCleanerTimer = new Timer(TimeSpan.FromHours(3).TotalMilliseconds);

            // The exceptions are caught inside the async function so returning void is not a danger.
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
#pragma warning disable AsyncFixer03 // Avoid unsupported fire-and-forget async-void methods or delegates. Unhandled exceptions will crash the process.

            _oldResultBlobsCleanerTimer.Elapsed += async (_, _) =>
            {
                try
                {
                    // Removing those result blobs that weren't deleted somehow (like the client exited while waiting
                    // for the result to appear, thus never requesting it hence it never getting deleted).
                    var oldResultBlobs = (await GetBlobsAsync(container, "results/"))
                        .Cast<CloudBlockBlob>()
                        .Where(blob => blob.Properties.LastModified < clock.UtcNow.AddHours(-1));

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
#pragma warning restore AsyncFixer03 // Avoid unsupported fire-and-forget async-void methods or delegates. Unhandled exceptions will crash the process.
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            _oldResultBlobsCleanerTimer.Enabled = true;
        }

        public async Task WorkAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (true)
                {
                    _telemetryClient.Flush();
                    var jobBlobs = (await GetBlobsAsync(_container, "jobs/"))
                        .Where(blob => !blob.StorageUri.PrimaryUri.ToString().Contains("$$$ORCHARD$$$.$$$", StringComparison.Ordinal))
                        .Cast<CloudBlockBlob>()
                        .Where(blob => blob.Properties.LeaseStatus == LeaseStatus.Unlocked && blob.Properties.Length != 0);

                    foreach (var jobBlob in jobBlobs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        _transformationTasks[jobBlob.Name] = _blobProcessor.StartJobAsync(jobBlob, cancellationToken)
                            .ContinueWith(
                                (task, blobNameObject) => _transformationTasks
                                    .TryRemove((string)blobNameObject, out _),
                                jobBlob.Name,
                                cancellationToken,
                                TaskContinuationOptions.None,
                                TaskScheduler.Default);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // Waiting a bit between cycles not to have excessive Blob Storage usage due to polling (otherwise
                    // it's not an issue, this loop barely uses any CPU).
                    await Task.Delay(1_000, cancellationToken);
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
                    await Task.Delay(10_000, cancellationToken);

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
            Task.Delay(10_000).Wait();
        }

        public static async Task<IHastlayer> CreateHastlayerAsync(
            ITransformationWorkerConfiguration configuration,
            Action<IHastlayerConfiguration, IServiceCollection> onServiceRegistration = null,
            CancellationToken cancellationToken = default)
        {
            var container = CloudStorageAccount
                .Parse(configuration.StorageConnectionString)
                .CreateCloudBlobClient()
                .GetContainerReference("transformation");
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);

            var hastlayerConfiguration = new HastlayerConfiguration
            {
                Flavor = HastlayerFlavor.Developer,
                // These extensions need to be added explicitly because when deployed as a flat folder of binaries they
                // won't be automatically found under the Hast.Core and Hast.Abstractions folders.
                Extensions = new[]
                {
                    typeof(DefaultTransformer).Assembly,
                    typeof(VhdlTransformingEngine).Assembly,
                    typeof(NexysA7Driver).Assembly,
                    typeof(TimingReportParser).Assembly,
                    typeof(CatapultDriver).Assembly,
                },
                ConfigureLogging = builder => builder
                    .AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Trace)
                    .AddApplicationInsights(ApplicationInsightsTelemetryManager.GetInstrumentationKey())
                    .AddNLog("NLog.config"),
                OnServiceRegistration = (sender, services) =>
                {
                    services.AddSingleton(configuration);
                    services.AddSingleton(container);
                    services.AddSingleton<ITransformationWorker, TransformationWorker>();
                    services.AddLocalization();

                    onServiceRegistration?.Invoke(sender, services);
                },
            };

            cancellationToken.ThrowIfCancellationRequested();

            var hastlayer = Hastlayer.Create(hastlayerConfiguration);
            return hastlayer;
        }

        private static async Task<List<IListBlobItem>> GetBlobsAsync(CloudBlobContainer container, string prefix)
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
    }
}
