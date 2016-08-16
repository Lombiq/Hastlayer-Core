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
                ClockFrequencyHz = 100000000,
                SupportedCommunicationChannelNames = new[] { "Serial", "Ethernet" },
                AvailableMemoryBytes = 115343360 // 110MB
            };
        }


        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression)
        {
            var op = expression.Operator;

            if (op == BinaryOperatorType.Modulus)
            {
                return 7;
            }
            else if (op == BinaryOperatorType.Multiply || op == BinaryOperatorType.Divide)
            {
                // If the Right expression results in 2^n then since the operations will be implemented with a very 
                // compact circuit (just with wiring) we can assume that it's "instant".
                if (expression.Right is PrimitiveExpression)
                {
                    // LiteralValue somehow is an empty string for PrimitiveExpressions.
                    var valueObject = ((PrimitiveExpression)expression.Right).Value;
                    var literalValue = valueObject != null ? valueObject.ToString() : string.Empty;
                    int intValue;

                    if (int.TryParse(literalValue, out intValue))
                    {
                        var log = Math.Log(intValue, 2);
                        // If the logarithm is a whole number that means that the value can be expressed as a power of 2.
                        if (log == Math.Floor(log))
                        {
                            return 0.1M; 
                        }
                    }
                }

                // 7 just for now to be safe, will find the actual threshold.
                return 7;
            }

            return 0.1M;
        }
    }
}
