using Hast.Remote.Worker.Daemon.Helpers;
using System;
using System.Linq;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
        public static int Main()
        {
            int errorCode = -1;

            try
            {
                var service = ServiceController
                    .GetServices()
                    .SingleOrDefault(controller => controller.ServiceName == Service.Name);

                var operation = service?.Status switch
                {
                    null => DaemonOperation.Install,
                    ServiceControllerStatus.StartPending => DaemonOperation.StartFromScm,
                    _ => DaemonOperation.Uninstall,
                };
                errorCode = (int)operation;

                SelfInstaller.Evaluate(operation);
            }
            catch(Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                return errorCode;
            }

            return 0;
        }
    }
}
