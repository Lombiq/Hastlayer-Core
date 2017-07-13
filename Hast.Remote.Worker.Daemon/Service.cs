using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Hast.Remote.Worker.Configuration;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Exceptions;
using Orchard.Logging;

namespace Hast.Remote.Worker.Daemon
{
    public class Service : ServiceBase
    {
        public const string Name = "Hast.Remote.Worker.Daemon";
        public const string DisplayName = "Hastlayer Remote Worker Daemon";

        private readonly EventLog _eventLog;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _workerTask;
        private int _restartCount = 0;


        public Service()
        {
            _eventLog = new EventLog();
            _eventLog.Log = DisplayName;
            // The EventLog source can't contain dots like the service's technical name.
            _eventLog.Source = "HastRemoteWorkerDaemon";

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
                var settings = new AppHostSettings
                {
                    ImportedExtensions = new[] { typeof(ITransformationWorker).Assembly },
                    DefaultShellFeatureStates = new[]
                    {
                            new DefaultShellFeatureState
                            {
                                EnabledFeatures = new[]
                                {
                                    typeof(ITransformationWorker).Assembly.ShortName()
                                }
                            }
                    }
                };

                using (var host = await OrchardAppHostFactory.StartTransientHost(settings, null, null))
                {
                    try
                    {
                        await host.Run<IAppConfigurationAccessor, ITransformationWorker>((configurationAccessor, worker) =>
                        {
                            var configuration = new TransformationWorkerConfiguration
                            {
                                StorageConnectionString = configurationAccessor
                                    .GetConfiguration(ConfigurationKeys.StorageConnectionStringKey)
                            };

                            // Only counting startup crashes.
                            _restartCount = 0;

                            return worker.Work(configuration, _cancellationTokenSource.Token);
                        });
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        await host.Run<ILoggerService>(logger =>
                        {
                            logger.Error(ex, DisplayName + " crashed with an unhandled exception. Restarting...");

                            if (_restartCount >= 10)
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
                                logger.Fatal(
                                    ex, 
                                    DisplayName + " crashed with an unhandled exception and was restarted " + 
                                    _restartCount + " times. It won't be restarted again.");
                            }

                            return Task.CompletedTask;
                        });
                    }
                }
            });

            _eventLog.WriteEntry(DisplayName + " started.");
        }

        private void RunStopTasks()
        {
            _cancellationTokenSource.Cancel();
            _workerTask?.Wait();

            _eventLog.WriteEntry(DisplayName + " stopped.");
        }
    }
}
