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
            // This takes about 112ms on an i5 processor with two physical (four logical) cores and 21ms on
            // an FPGA.
            var output = hastlayerOptimizedAlgorithm.Run(234234);
        }
    }
}
