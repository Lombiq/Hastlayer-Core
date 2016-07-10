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
    internal static class RecursiveAlgorithmsSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.RecursiveAlgorithms");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var recursiveAlgorithms = await hastlayer.GenerateProxy(hardwareRepresentation, new RecursiveAlgorithms());

            var fibonacci = recursiveAlgorithms.CalculateFibonacchiSeries((short)13); // 233
            var factorial = recursiveAlgorithms.CalculateFactorial((short)6); // 720 
        }
    }
}
