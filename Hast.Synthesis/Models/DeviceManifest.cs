using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Synthesis.Models
{
    public class DeviceManifest : IDeviceManifest
    {
        public string TechnicalName { get; set; }
        public uint ClockFrequencyHz { get; set; }
    }
}
