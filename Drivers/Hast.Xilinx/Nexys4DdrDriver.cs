using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis;
using Hast.Synthesis.Models;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : IDeviceDriver
    {
        public IDeviceManifest DeviceManifest { get; private set; }


        public Nexys4DdrDriver()
        {
            DeviceManifest = new DeviceManifest
            {
                TechnicalName = "Nexys4 DDR",
                ClockFrequencyHz = 100000000
            };
        }
    }
}
