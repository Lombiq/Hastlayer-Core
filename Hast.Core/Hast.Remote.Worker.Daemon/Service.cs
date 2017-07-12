using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Daemon
{
    public class Service : ServiceBase
    {
        public const string Name = "Hast.Remote.Worker.Daemon";
        public const string DisplayName = "Hastlayer Remote Worker Daemon";

        private readonly EventLog _eventLog;


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

            _eventLog.WriteEntry(DisplayName + " started.");
        }

        protected override void OnStop()
        {
            _eventLog.WriteEntry(DisplayName + " stopped.");
        }
    }
}
