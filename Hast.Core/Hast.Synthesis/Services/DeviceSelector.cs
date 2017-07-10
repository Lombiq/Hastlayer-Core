using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Synthesis.Abstractions;

namespace Hast.Synthesis.Services
{
    // IDeviceManifestSelector is a separate service so it's not necessary to share IDeviceDriver with clients.
    public class DeviceSelector : IDeviceManifestSelector, IDeviceDriverSelector
    {
        private readonly IEnumerable<IDeviceDriver> _drivers;


        public DeviceSelector(IEnumerable<IDeviceDriver> drivers)
        {
            _drivers = drivers;
        }

        public Task<IEnumerable<IDeviceManifest>> GetSupporteDevices() => 
            Task.FromResult(_drivers.Select(driver => driver.DeviceManifest));

        public IDeviceDriver GetDriver(string deviceName) =>
            _drivers.FirstOrDefault(driver => driver.DeviceManifest.Name == deviceName);

    }
}
