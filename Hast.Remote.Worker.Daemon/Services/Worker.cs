using Hast.Common.Services;
using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Remote.Worker.Daemon.Constants;
using Hast.Remote.Worker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Hast.Remote.Worker.Daemon.Constants.ServiceProperties;

namespace Hast.Remote.Worker.Daemon.Services
{
    public class Worker : BackgroundService
    {
        private const int MaxTries = 10;

        private readonly IHost _host;
        private readonly IEventLogger _eventLogger;
        private ILogger _logger;
        private DisposableContainer<ITransformationWorker> _disposableContainer;
        private Hastlayer _hastlayer;

        public Worker(IHost host, IEventLogger eventLogger, ILogger<Worker> logger)
        {
            _host = host;
            _eventLogger = eventLogger;
            _logger = logger;
        }

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StartAsync(cancellationToken);
                _eventLogger.UpdateStatus("started");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Failed to start {0}", Name);
                await StopAsync(cancellationToken);
            }
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StopAsync(cancellationToken);

                _eventLogger.UpdateStatus("stopped");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Failed to stop {0}", Name);
            }
        }

        public override void Dispose()
        {
            _disposableContainer?.Dispose();
            _hastlayer?.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            ExecuteInnerAsync(MaxTries - 1, stoppingToken);

        private async Task ExecuteInnerAsync(int tries, CancellationToken cancellationToken)
        {
            _hastlayer = await CreateHostAsync(cancellationToken);
            if (_hastlayer == null)
            {
                await StopAsync(cancellationToken);
                await _host.StopAsync(cancellationToken);
                Dispose();
                return;
            }

            var isStartupCrash = false;

            try
            {
                try
                {
                    _disposableContainer = _hastlayer.GetService<ITransformationWorker>();
                }
                catch
                {
                    isStartupCrash = true;
                    throw;
                }

                // Overwrite these loggers with the Application Insights enabled versions from inside the Hastlayer DI.
                _logger = _hastlayer.GetLogger<Worker>();
                _eventLogger.Logger = _hastlayer.GetLogger<EventLogger>();

                var workerTask = _disposableContainer.Value.WorkAsync(cancellationToken);
                // Wait until the task completes or the stop token triggers. Same as in base.StopAsync().
                await Task.WhenAny(workerTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
                return;
            }
            catch (Exception exception) when (isStartupCrash && !exception.IsFatal())
            {
                await _hastlayer.RunAsync<ILogger<Worker>>(logger =>
                {
                    logger.LogError(exception, DisplayName + " crashed with an unhandled exception. Restarting...");

                    if (tries == 0)
                    {
                        logger.LogCritical(
                            exception,
                            "{0} crashed with unhandled exceptions {1} times and it won't be restarted again",
                            DisplayName,
                            MaxTries);
                    }

                    return Task.CompletedTask;
                });
            }

            if (tries > 0) await ExecuteInnerAsync(tries - 1, cancellationToken);
        }

        private async Task<Hastlayer> CreateHostAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configuration = TransformationWorkerConfiguration.Create();

                var hastlayerConfiguration = await new HastlayerConfigurationProvider()
                    .GetConfiguration(configuration, cancellationToken);
                return (Hastlayer)Hastlayer.Create(hastlayerConfiguration);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "failed to initialize Hastlayer");
                Program.ExitCode = ExitCode.FailedToInitializeHastlayer;

                return null;
            }
        }
    }
}
