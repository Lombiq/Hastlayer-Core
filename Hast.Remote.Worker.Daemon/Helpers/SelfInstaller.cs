using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon.Helpers
{
    public static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;


        public static void InstallMe() => ManagedInstallerClass.InstallHelper(new[] { _exePath });

        public static void UninstallMe()
        {
            var s = new ServiceInstaller
            {
                Context = new InstallContext(),
                ServiceName = Service.Name,
            };
            s.Uninstall(null);
        }

        public static void Evaluate(DaemonOperation operation)
        {
            switch (operation)
            {
                case DaemonOperation.Install:
                    InstallMe();
                    break;
                case DaemonOperation.Uninstall:
                    UninstallMe();
                    break;
                case DaemonOperation.StartFromScm:
                    var servicesToRun = new ServiceBase[] { new Service() };
                    ServiceBase.Run(servicesToRun);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
