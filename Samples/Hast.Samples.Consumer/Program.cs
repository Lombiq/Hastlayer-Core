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

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");

                        configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                            new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.PrimeCalculator.ParallelizedArePrimeNumbers.LambdaExpression.0")
                            {
                                MaxDegreeOfParallelism = PrimeCalculator.MaxDegreeOfParallelism
                            });

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                            },
                            configuration);


                        File.WriteAllText(@"C:\Users\Zoltán\Desktop\GPU Day\Hast_IP.vhd", ToVhdl(hardwareRepresentation.HardwareDescription));

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());

                        var isPrime = primeCalculator.IsPrimeNumber(15);
                        var isPrime2 = primeCalculator.IsPrimeNumber(13);
                        var isPrime3 = await primeCalculator.IsPrimeNumberAsync(21);
                        Debugger.Break();

                        var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 });
                        var arePrimes2 = primeCalculator.ArePrimeNumbers(new uint[] { 13, 493 });
                        Debugger.Break();

                        var parallelizedArePrimes = primeCalculator.ParallelizedArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 });
                        Debugger.Break();

                        var numberCount = PrimeCalculator.MaxDegreeOfParallelism;
                        var numbers = new uint[numberCount];
                        for (uint i = (uint)(uint.MaxValue - numberCount); i < uint.MaxValue; i++)
                        {
                            numbers[i - (uint.MaxValue - numberCount)] = (uint)i;
                        }

                        var arePrimes4 = await primeCalculator.ParallelizedArePrimeNumbers(numbers);
                        Debugger.Break();
                    }
                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }


        private static string ToVhdl(IHardwareDescription hardwareDescription)
        {
            return ((Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareDescription)
                .Manifest.TopModule.ToVhdl(new VhdlGenerationOptions { FormatCode = true, NameShortener = VhdlGenerationOptions.SimpleNameShortener });
        }
    }
}
