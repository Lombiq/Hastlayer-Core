using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon.Helpers
{
    public static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;


        public static void InstallMe()
        {
            Console.WriteLine("Installing service...");
            ManagedInstallerClass.InstallHelper(new[] { _exePath });
        }

        public static void UninstallMe()
        {
            Console.WriteLine("Uninstalling service...");
            var serviceInstaller = new ServiceInstaller
            {
                Context = new InstallContext(),
                ServiceName = Service.Name,
            };
            serviceInstaller.Uninstall(null);
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
                case DaemonOperation.Start:
                    var servicesToRun = new ServiceBase[] { new Service() };
                    ServiceBase.Run(servicesToRun);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
