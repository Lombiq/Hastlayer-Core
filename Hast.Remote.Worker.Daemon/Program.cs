using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
        public static void Main()
        {
            // Below code taken mostly from http://www.codeproject.com/Articles/27112/Installing-NET-Windows-Services-the-easiest-way
            var service = ServiceController
                .GetServices()
                .SingleOrDefault(controller => controller.ServiceName == Service.Name);

            // Service not installed
            if (service == null)
            {
                SelfInstaller.InstallMe();
            }
            // Service is not starting
            else if (service.Status != ServiceControllerStatus.StartPending)
            {
                SelfInstaller.UninstallMe();
            }
            // Started from the SCM
            else
            {
                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }


    internal static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;


        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { _exePath });
            }
            catch(Exception exception)
            {
                NoDependencyFatalExceptionLogger.Log(exception);
                return false;
            }

            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", _exePath });
            }
            catch(Exception exception)
            {
                NoDependencyFatalExceptionLogger.Log(exception);
                return false;
            }

            return true;
        }
    }
}
