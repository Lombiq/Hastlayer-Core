using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Tests.TestAssembly1.ComplexTypes;
using Hast.Tests.TestAssembly2;
using System.Drawing;
using Hast.VhdlBuilder.Representation;
using System.Diagnostics;

namespace Hast.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
                {
                    // Generating hardware from samples:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        hastlayer.ExecutedOnHardware += (sender, e) =>
                            {
                                Console.WriteLine(
                                    "Executing " +
                                    e.MemberFullName +
                                    " on hardware took " +
                                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds +
                                    "ms (net) " +
                                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds +
                                    " milliseconds (all together)");
                            };


                        var configuration = new HardwareGenerationConfiguration();

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm");

                        configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                            new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm.Run.LambdaExpression.0")
                            {
                                MaxDegreeOfParallelism = HastlayerOptimizedAlgorithm.MaxDegreeOfParallelism
                            });

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                            },
                            configuration);


                        File.WriteAllText(@"C:\Users\Zoltán\Desktop\GPU Day\Hast_IP.vhd", ToVhdl(hardwareRepresentation.HardwareDescription));


                        var sw = Stopwatch.StartNew();
                        var cpuOutput = new HastlayerOptimizedAlgorithm().Run(234234);
                        sw.Stop();
                        Debugger.Break();

                        var hastlayerOptimizedAlgorithm = await hastlayer.GenerateProxy(hardwareRepresentation, new HastlayerOptimizedAlgorithm());

                        var output = hastlayerOptimizedAlgorithm.Run(234234);
                        Debugger.Break();
                    }
                }).Wait();
        }


        private static string ToVhdl(IHardwareDescription hardwareDescription)
        {
            return ((Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareDescription)
                .Manifest.TopModule.ToVhdl(new VhdlGenerationOptions { FormatCode = true, NameShortener = VhdlGenerationOptions.SimpleNameShortener });
        }
    }
}
