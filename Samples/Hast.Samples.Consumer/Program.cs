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
                        hastlayer.Transformed += (sender, e) =>
                            {
                                var vhdlHardwareDescription = (Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)e.HardwareDescription;
                                var vhdl = vhdlHardwareDescription.Manifest.TopModule.ToVhdl();
                                System.IO.File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                            };

                        var configuration = new HardwareGenerationConfiguration
                        {
                            //IncludedMembers = new[]
                            //{
                            //    "System.Boolean Hast.Samples.SampleAssembly.PrimeCalculator::IsPrimeNumber(System.Int32)",
                            //    "System.Int32 Hast.Samples.SampleAssembly.ServiceSample::Hast.Samples.SampleAssembly.IService.Method1()"
                            //}
                        };

                        var hardwareAssembly = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                                //typeof(Math).Assembly
                            }, 
                            configuration);

                        IService serviceParameter = new ServiceSample();
                        var service = await hastlayer.GenerateProxy(hardwareAssembly, serviceParameter);
                        service.Method1();

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareAssembly, new PrimeCalculator());
                        var isPrime = primeCalculator.IsPrimeNumber(15);
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
