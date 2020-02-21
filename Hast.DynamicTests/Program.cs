using Hast.Layer;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using System;
using System.Threading.Tasks;

namespace Hast.DynamicTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var hastlayer = await Hastlayer.Create())
            {
                var configuration = new HardwareGenerationConfiguration("Nexys A7");

                //configuration.AddHardwareEntryPointType<ParallelAlgorithm>();

                configuration.VhdlTransformerConfiguration().VhdlGenerationConfiguration = VhdlGenerationConfiguration.Debug;

                hastlayer.ExecutedOnHardware += (sender, e) =>
                {
                    Console.WriteLine(
                        "Executing on hardware took " +
                        e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds +
                        " milliseconds (net) " +
                        e.HardwareExecutionInformation.FullExecutionTimeMilliseconds +
                        " milliseconds (all together).");
                };

                Console.WriteLine("Hardware generation starts.");
                var hardwareRepresentation = await hastlayer.GenerateHardware(
                    new[]
                    {
                        typeof(Program).Assembly
                    },
                    configuration);

                await hardwareRepresentation.HardwareDescription.WriteSource("Hast_IP.vhd");

                Console.WriteLine("Hardware generated, starting software execution.");
                var proxyGenerationConfiguration = new ProxyGenerationConfiguration { VerifyHardwareResults = true };
                //var parallelAlgorithm = await hastlayer.GenerateProxy(hardwareRepresentation, new ParallelAlgorithm(), proxyGenerationConfiguration);

                //Console.WriteLine();
                //var sw = System.Diagnostics.Stopwatch.StartNew();
                //var cpuOutput = new ParallelAlgorithm().Run(234234);
                //sw.Stop();
                //Console.WriteLine("On CPU it took " + sw.ElapsedMilliseconds + " milliseconds.");

                //Console.WriteLine();
                //Console.WriteLine("Starting hardware execution.");
                //var output1 = parallelAlgorithm.Run(234234);
                //var output2 = parallelAlgorithm.Run(123);
                //var output3 = parallelAlgorithm.Run(9999);
            }

            Console.ReadKey();
        }
    }
}
