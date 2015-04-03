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
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        hastlayer.Transformed += (sender, e) =>
                            {
                                var vhdlHardwareDescription = (Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)e.HardwareDescription;
                                var vhdl = vhdlHardwareDescription.Manifest.TopModule.ToVhdl();
                                //System.IO.File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                            };

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly,
                                typeof(Math).Assembly
                            },
                            HardwareGenerationConfiguration.Default);

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());
                        var isPrime = primeCalculator.IsPrimeNumber(15);
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
