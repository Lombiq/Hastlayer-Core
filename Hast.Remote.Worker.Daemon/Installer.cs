using System.ComponentModel;
using System.ServiceProcess;

namespace Hast.Remote.Worker.Daemon
{
    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            var process = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
            };
            var serviceAdmin = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                DelayedAutoStart = true,
                ServiceName = Service.Name,
                DisplayName = Service.DisplayName,
                Description = "Runs the Hastlayer Remote Worker.",
            };
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
