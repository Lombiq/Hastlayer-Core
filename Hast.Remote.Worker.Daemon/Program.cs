using Hast.Remote.Worker.Daemon.Helpers;
using System;
using System.Linq;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            int errorCode = -1;

            try
            {
                var serviceStatus = ServiceController
                    .GetServices()
                    .SingleOrDefault(controller => controller.ServiceName == Service.Name)?
                    .Status;

                var operation =
                    args.Length > 0 &&
                    Enum.TryParse(typeof(DaemonOperation), args[0], ignoreCase: true, out var enumObject)
                        ? (DaemonOperation)enumObject!
                        : serviceStatus switch
                        {
                            null => DaemonOperation.Install,
                            ServiceControllerStatus.StartPending => DaemonOperation.Start,
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
