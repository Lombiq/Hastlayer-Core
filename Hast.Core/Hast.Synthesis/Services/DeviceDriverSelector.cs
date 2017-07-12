using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Synthesis.Abstractions;

namespace Hast.Synthesis.Services
{
    public class DeviceDriverSelector : IDeviceDriverSelector
    {
        private readonly IEnumerable<IDeviceDriver> _drivers;


        public DeviceDriverSelector(IEnumerable<IDeviceDriver> drivers)
        {
            _drivers = drivers;
        }


        public IDeviceDriver GetDriver(string deviceName) =>
            _drivers.FirstOrDefault(driver => driver.DeviceManifest.Name == deviceName);

    }
}
