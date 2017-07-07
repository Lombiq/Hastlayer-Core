using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;
using Orchard;

namespace Hast.Synthesis.Services
{
    public interface IDeviceDriverSelector : IDependency
    {
        IEnumerable<IDeviceManifest> GetSupporteDevices();
        IDeviceDriver GetDriver(string deviceName);
    }
}
