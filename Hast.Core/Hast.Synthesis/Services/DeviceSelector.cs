using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;
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

        public IEnumerable<IDeviceManifest> GetSupporteDevices() => _drivers.Select(driver => driver.DeviceManifest);

        public IDeviceDriver GetDriver(string deviceName) =>
            _drivers.FirstOrDefault(driver => driver.DeviceManifest.Name == deviceName);

    }
}
