using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.PrimeCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
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

                    var hardwareConfiguration = new HardwareGenerationConfiguration();
                    hardwareConfiguration.PublicHardwareMemberPrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");
                    var hardwareRepresentation = await hastlayer.GenerateHardware(
                        new[]
                            {
                                typeof(Hast.Samples.SampleAssembly.PrimeCalculator).Assembly
                            },
                        hardwareConfiguration);

                    var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new Hast.Samples.SampleAssembly.PrimeCalculator());
                    var isPrime = primeCalculator.IsPrimeNumber(15);
                    var isPrime2 = primeCalculator.IsPrimeNumber(13);
                    var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 }); // Only 2341 is prime
                    var arePrimes2 = primeCalculator.ArePrimeNumbers(new uint[] { 13, 493 });

                    // With 210 numbers this takes about 2,1s all together (with UART) on an FPGA and 166s on a 3,2GHz i7.
                    // With 4000 numbers it takes 38s on an FPGA and 3550s (about an hour) on the same PC.
                    var numberCount = 210;
                    var numbers = new uint[numberCount];
                    for (uint i = (uint)(uint.MaxValue - numberCount); i < uint.MaxValue; i++)
                    {
                        numbers[i - (uint.MaxValue - numberCount)] = (uint)i;
                    }
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var arePrimes3 = primeCalculator.ArePrimeNumbers(numbers);
                    sw.Stop();
                }
            }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
