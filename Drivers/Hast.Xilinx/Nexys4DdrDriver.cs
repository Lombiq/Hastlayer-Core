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
    public class Nexys4DdrDriver : IHardwareDriver
    {
        public IHardwareManifest HardwareManifest { get; private set; }


        public Nexys4DdrDriver()
        {
            HardwareManifest = new HardwareManifest
            {
                TechnicalName = "Nexys4 DDR",
                ClockFrequencyHz = 100000000
            };
        }


        public uint GetClockCyclesNeededForOperation(BinaryOperatorType operation)
        {
            if (operation == BinaryOperatorType.Modulus)
            {
                return 3;
            }

            return 1;
        }
    }
}
