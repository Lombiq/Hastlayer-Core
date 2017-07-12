using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Synthesis.Services
{
    public interface IDeviceDriverSelector : IDependency
    {
        IDeviceDriver GetDriver(string deviceName);
    }
}
