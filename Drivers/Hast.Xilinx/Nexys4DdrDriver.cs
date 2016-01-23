using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis;
using Hast.Synthesis.Models;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : IHardwareDriver
    {
        public IHardwareManifest HardwareManifest { get; private set; }


        public Nexys4DdrDriver()
        {
            HardwareManifest = new HardwareManifest
            {
                ClockFrequencyHz = 100000000
            };
        }
    }
}
