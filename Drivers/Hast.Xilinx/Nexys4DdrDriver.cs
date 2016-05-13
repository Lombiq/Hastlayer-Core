using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis;
using Hast.Synthesis.Models;
using ICSharpCode.NRefactory.CSharp;

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


        public decimal GetClockCyclesNeededForOperation(BinaryOperatorType operation)
        {
            if (operation == BinaryOperatorType.Modulus)
            {
                return 8;
            }
            else if (operation == BinaryOperatorType.Multiply || operation == BinaryOperatorType.Divide)
            {
                // 8 just for now to be safe, will find the actual threshold.
                return 8;
            }

            return 0.1M;
        }
    }
}
