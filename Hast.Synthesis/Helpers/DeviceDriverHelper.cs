using System;
using Hast.Layer;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Synthesis.Helpers
{
    public static class DeviceDriverHelper
    {
        public static bool IsInstantBinaryOperation(BinaryOperatorExpression expression)
        {
            var binaryOperator = expression.Operator;

            // If the Right expression results in 2^n then since the operations will be implemented with a very compact 
            // circuit (just with wiring) we can assume that it's "instant".
            if ((binaryOperator == BinaryOperatorType.Multiply || binaryOperator == BinaryOperatorType.Divide) &&
                expression.Right is PrimitiveExpression)
            {
                // LiteralValue somehow is an empty string for PrimitiveExpressions.
                var valueObject = ((PrimitiveExpression)expression.Right).Value;
                var literalValue = valueObject != null ? valueObject.ToString() : string.Empty;

                if (int.TryParse(literalValue, out var intValue))
                {
                    var log = Math.Log(intValue, 2);
                    // If the logarithm is a whole number that means that the value can be expressed as a power of 2.
                    return log == Math.Floor(log);
                }
            }

            return false;
        }

        public static decimal ComputeClockCyclesFromLatency(IDeviceManifest deviceManifest, decimal latencyNs)
        {
            var latencyClockCycles = latencyNs * (deviceManifest.ClockFrequencyHz * 0.000000001M);

            // If there is no latency then let's try with a basic default (unless the operation is "instant" there should
            // be latency data).
            if (latencyClockCycles < 0) return 0.1M;

            return latencyClockCycles;
        }
    }
}
