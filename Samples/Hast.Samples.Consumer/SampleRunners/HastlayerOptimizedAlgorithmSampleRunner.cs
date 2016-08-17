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
    internal static class HastlayerOptimizedAlgorithmSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm");

            configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm.Run.LambdaExpression.0")
                {
                    MaxDegreeOfParallelism = HastlayerOptimizedAlgorithm.MaxDegreeOfParallelism
                });
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var hastlayerOptimizedAlgorithm = await hastlayer.GenerateProxy(hardwareRepresentation, new HastlayerOptimizedAlgorithm());

            // This takes about 2700ms on an i7 processor with 4 physical (8 logical) cores and 100ms on an FPGA (with 
            // a MaxDegreeOfParallelism of 200 while the device is just about 55% utilized).
            var output1 = hastlayerOptimizedAlgorithm.Run(234234);
            var output2 = hastlayerOptimizedAlgorithm.Run(123);
            var outpu3 = hastlayerOptimizedAlgorithm.Run(9999);
        }
    }
}
