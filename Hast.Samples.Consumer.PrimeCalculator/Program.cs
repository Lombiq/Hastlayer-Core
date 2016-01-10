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
                    var hardwareConfiguration = new HardwareGenerationConfiguration();
                    hardwareConfiguration.PublicHardwareMemberPrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");
                    var hardwareRepresentation = await hastlayer.GenerateHardware(
                        new[]
                            {
                                typeof(Hast.Samples.SampleAssembly.PrimeCalculator).Assembly
                            },
                        hardwareConfiguration);

                    var materializedHardware = await hastlayer.MaterializeHardware(hardwareRepresentation);

                    var primeCalculator = await hastlayer.GenerateProxy(materializedHardware, new Hast.Samples.SampleAssembly.PrimeCalculator());
                    var isPrime = primeCalculator.IsPrimeNumber(15);
                    var isPrime2 = primeCalculator.IsPrimeNumber(13);
                    var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 }); // Only 2341 is prime
                }
            }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
