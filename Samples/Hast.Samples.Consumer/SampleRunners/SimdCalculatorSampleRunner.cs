using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class SimdCalculatorSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.SimdCalculator");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            // Starting with 1 not to have a divide by zero.
            var vector = Enumerable.Range(1, SimdCalculator.MaxDegreeOfParallelism * 4 + 1).ToArray();

            var simdCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new SimdCalculator());

            var sumVector = simdCalculator.AddVectors(vector, vector);
            var differenceVector = simdCalculator.SubtractVectors(vector, vector);
            var productVector = simdCalculator.MultiplyVectors(vector, vector);
            var quotientVector = simdCalculator.DivideVectors(vector, vector);
        }
    }
}
