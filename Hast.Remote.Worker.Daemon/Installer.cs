using System.ComponentModel;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon
{
    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            var process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            var serviceAdmin = new ServiceInstaller();
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            serviceAdmin.DelayedAutoStart = true;
            serviceAdmin.ServiceName = Service.Name;
            serviceAdmin.DisplayName = Service.DisplayName;
            serviceAdmin.Description = "Runs the Hastlayer Remote Worker.";
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
