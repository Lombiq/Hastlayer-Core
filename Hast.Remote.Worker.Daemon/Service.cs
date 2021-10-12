using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Remote.Worker.Daemon.Constants;
using Hast.Remote.Worker.Daemon.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Daemon
{
    public class Service : ServiceBase
    {
        public const string Name = "Hast.Remote.Worker.Daemon";
        public const string DisplayName = "Hastlayer Remote Worker Daemon";

        private readonly EventLog _eventLog;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task _workerTask;
        private int _restartCount;


        public Service()
        {
            _eventLog = new EventLog
            {
                Log = DisplayName,
                // The EventLog source can't contain dots like the service's technical name.
                Source = Name.Replace(".", string.Empty),
            };

            ServiceName = Name;
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }


        protected override void OnStart(string[] args)
        {
            if (!EventLog.Exists(_eventLog.Log))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(_eventLog.Source, _eventLog.Log));
            }

            RunStartTasks();
        }

        protected override void OnStop() => RunStopTasks();


        private void RunStartTasks()
        {
            _workerTask = Task.Run(async () =>
            {
                using var host = await CreateHostAsync();

                try
                {
                    await host.RunAsync<IServiceProvider>(serviceProvider =>
                    {
                        var worker = serviceProvider.GetService<ITransformationWorker>();

                        // Only counting startup crashes.
                        _restartCount = 0;

                        return worker.WorkAsync(_cancellationTokenSource.Token);
                    });
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    await host.RunAsync<ILogger<Service>>(logger =>
                    {
                        logger.LogError(ex, DisplayName + " crashed with an unhandled exception. Restarting...");

                        if (_restartCount < 10)
                        {
                            _restartCount++;

                            // Not exactly the nicest way to restart the worker, and increases memory usage with each
                            // restart. But such restarts should be extremely rare, this should be just a last resort.
                            _workerTask = null;
                            RunStopTasks();
                            RunStartTasks();
                        }
                        else
                        {
                            logger.LogCritical(
                                ex,
                                "{0} crashed with an unhandled exception and was restarted {1} times. It won't be restarted again.",
                                DisplayName,
                                _restartCount);
                        }

                        return Task.CompletedTask;
                    });
                }
            });

            _eventLog.WriteEntry(DisplayName + " started.");
        }

        private async Task<Hastlayer> CreateHostAsync()
        {
            try
            {
                var appSettings = Hastlayer.BuildConfiguration();
                var configuration = new TransformationWorkerConfiguration
                {
                    StorageConnectionString =
                        appSettings.GetConnectionString(ConfigurationKeys.StorageConnectionStringKey),
                };

                return (Hastlayer)await TransformationWorker.CreateHastlayerAsync(
                    configuration,
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                Environment.Exit(-1);
                return null;
            }
        }

        private void RunStopTasks()
        {
            _cancellationTokenSource.Cancel();
            _workerTask?.Wait();

            _eventLog.WriteEntry(DisplayName + " stopped.");
        }
    }
}
