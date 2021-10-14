using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Remote.Worker.Daemon.Constants;
using Hast.Remote.Worker.Daemon.Helpers;
using Hast.Remote.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly IEventLogger _eventLogger;
        private readonly ILogger<Worker> _logger;

        public Worker(IEventLogger eventLogger, ILogger<Worker> logger)
        {
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
            catch (Exception exception) when (exception is not TaskCanceledException)
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
            catch (Exception exception) when (exception is not TaskCanceledException)
            {
                _logger.LogError(exception, "Failed to stop {0}", Name);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            ExecuteAsync(MaxTries - 1, stoppingToken);

        private async Task ExecuteAsync(int tries, CancellationToken stoppingToken)
        {
            using var host = await CreateHostAsync(stoppingToken);
            var isStartupCrash = false;

            try
            {
                await host.RunAsync<IServiceProvider>(serviceProvider =>
                {
                    ITransformationWorker worker;
                    try
                    {
                        worker = serviceProvider.GetRequiredService<ITransformationWorker>();
                    }
                    catch
                    {
                        isStartupCrash = true;
                        throw;
                    }

                    return worker.WorkAsync(stoppingToken);
                });
                return;
            }
            catch (Exception exception) when (isStartupCrash && !exception.IsFatal())
            {
                await host.RunAsync<ILogger<Worker>>(logger =>
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

            if (tries > 0) await ExecuteAsync(tries - 1, stoppingToken);
        }

        private async Task<Hastlayer> CreateHostAsync(CancellationToken cancellationToken)
        {
            try
            {
                var appSettings = Hastlayer.BuildConfiguration();
                var configuration = new TransformationWorkerConfiguration
                {
                    StorageConnectionString =
                        appSettings.GetConnectionString(ConfigurationKeys.StorageConnectionStringKey),
                };

                var hastlayerConfiguration = await new TransformationWorkerHastlayerConfigurationProvider()
                    .GetConfiguration(configuration, cancellationToken);
                return (Hastlayer)Hastlayer.Create(hastlayerConfiguration);
            }
            catch (Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                Environment.Exit(-1);
                return null;
            }
        }
    }
}
