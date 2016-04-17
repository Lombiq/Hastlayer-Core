using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Layer;

namespace Hast.Samples.Psc
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
                    //hardwareConfiguration.PublicHardwareMemberPrefixes.Add("Hast.Samples.Psc.PrimeCalculator");
                    var hardwareRepresentation = await hastlayer.GenerateHardware(
                        new[]
                            {
                                typeof(Program).Assembly
                            },
                        hardwareConfiguration);

                    var primeCalculator = await hastlayer
                        .GenerateProxy(hardwareRepresentation, new PrimeCalculator());


                    var isPrime = primeCalculator.IsPrimeNumber(15);
                    var isPrime2 = primeCalculator.IsPrimeNumber(13);
                    var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 });
                    var arePrimes2 = primeCalculator.ArePrimeNumbers(new uint[] { 13, 493 });

                    var numberCount = 4000;
                    var numbers = new uint[numberCount];
                    for (uint i = (uint)(uint.MaxValue - numberCount); i < uint.MaxValue; i++)
                    {
                        numbers[i - (uint.MaxValue - numberCount)] = (uint)i;
                    }
                    var arePrimes3 = primeCalculator.ArePrimeNumbers(numbers);


                    var parallelLaunchedIsPrimeTasks = new List<Task<bool>>();

                    for (uint i = 1; i <= 10; i++)
                    {
                        parallelLaunchedIsPrimeTasks
                            .Add(Task.Factory.StartNew(indexObject => primeCalculator.IsPrimeNumber((uint)indexObject), i));
                    }

                    var parallelLaunchedArePrimes = await Task.WhenAll(parallelLaunchedIsPrimeTasks);
                }
            }).Wait();
        }
    }
}
