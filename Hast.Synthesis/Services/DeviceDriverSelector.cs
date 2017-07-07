using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _drivers.FirstOrDefault(driver => driver.DeviceManifest.TechnicalName == deviceName);
    }
}
