using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
                {

                    using (var hastLayer = Hastlayer.Create())
                    {
                        var configuration = new HardwareGenerationConfiguration
                        {
                            MaxDegreeOfParallelism = 10
                        };

                        var hardwareAssembly = await hastLayer.GenerateHardware(typeof(PrimeCalculator).Assembly, configuration);

                        var primeCalculator = hastLayer.GenerateProxy(hardwareAssembly, new PrimeCalculator());
                        var isPrime = primeCalculator.IsPrimeNumber(15); // Maybe only allow methods that return a Task or its derivatives?
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
