using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

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
