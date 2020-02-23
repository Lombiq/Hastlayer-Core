using Hast.Layer;
using Hast.TestInputs.Dynamic;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using System;
using System.Threading.Tasks;

namespace Hast.DynamicTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // This all is just the beginning of dynamic testing. Once there will be more tests we'll need to shift to 
            // a more scalable structure and eventually add the ability to run all tests automatically.

            using (var hastlayer = await Hastlayer.Create())
            {
                var configuration = new HardwareGenerationConfiguration("Nexys A7");

                configuration.AddHardwareEntryPointMethod<BinaryAndUnaryOperatorExpressionCases>(b => b.AllUnaryOperatorExpressionVariations(null));

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
                        typeof(BinaryAndUnaryOperatorExpressionCases).Assembly
                    },
                    configuration);

                await hardwareRepresentation.HardwareDescription.WriteSource("Hast_IP.vhd");

                Console.WriteLine("Hardware generated, starting hardware execution.");
                var proxyGenerationConfiguration = new ProxyGenerationConfiguration { VerifyHardwareResults = true };
                var binaryAndUnaryOperatorExpressionCases = await hastlayer.GenerateProxy(
                    hardwareRepresentation,
                    new BinaryAndUnaryOperatorExpressionCases(),
                    proxyGenerationConfiguration);

                binaryAndUnaryOperatorExpressionCases.AllUnaryOperatorExpressionVariations(123);
            }

            Console.ReadKey();
        }
    }
}
