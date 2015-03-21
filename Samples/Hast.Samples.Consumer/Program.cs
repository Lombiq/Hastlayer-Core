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

                    var extensions = new[]
                        {
                            typeof(Hast.Transformer.Vhdl.VhdlTransformingEngine).Assembly,
                            typeof(Hast.Xilinx.XilinxHardwareRepresentationComposer).Assembly
                        };

                    using (var hastlayer = Hastlayer.Create(extensions))
                    {
                        var hardwareAssembly = await hastlayer.GenerateHardware(typeof(PrimeCalculator).Assembly, HardwareGenerationConfiguration.Default);

                        IService serviceParameter = new ServiceSample();
                        var service = await hastlayer.GenerateProxy(hardwareAssembly, serviceParameter);
                        service.Method();

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareAssembly, new PrimeCalculator());
                        var isPrime = primeCalculator.IsPrimeNumber(15);
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
