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
            var vectorSize = 20000;
            var vector = new int[vectorSize];
            for (int i = int.MaxValue - vectorSize; i < int.MaxValue; i++)
            {
                vector[i - int.MaxValue + vectorSize] = i;
            }

            var simdCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new SimdCalculator());

            var sumVector = simdCalculator.AddVectors(vector, vector);
        }
    }
}
