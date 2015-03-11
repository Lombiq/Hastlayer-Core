using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Transformer;

namespace Hast.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var transformer = new DefaultTransformer(new Hast.Transformer.Vhdl.VhdlTransformingEngine(new Hast.Transformer.Vhdl.TransformingSettings { MaxDegreeOfParallelism = 10 }));
            var csharp = @"
                        using System;
                        namespace TestNamespace
                        {
                            public class SimpleClass
                            {
                                public virtual bool IsPrimeNumber(int num)
                                {
                                    var isPrime = true;
                                    int factor = num / 2;
                                    //var factor = Math.Sqrt(num); Math.Sqrt() can't be processed yet

                                    for (int i = 2; i <= factor; i++)
                                    {
                                        if ((num % i) == 0) isPrime = false;
                                    }

                                    return isPrime;
                                }

                                // Arrays not yet supported
                                /*public virtual int[] PrimeFactors(int num)
                                {
                                    var i = 0;
                                    var result = new int[50];

                                    int divisor = 2;

                                    while (divisor <= num)
                                    {
                                        if (num % divisor == 0)
                                        {
                                            result[i++] = divisor;
                                            num /= divisor;
                                        }
                                        else divisor++;
                                    }

                                    return result;
                                }*/
                            }
                        }";

            var hardwareDefinition = transformer.Transform(csharp, Language.CSharp);
            //var hardwareDefinition = transformer.Transform(options.InputFilePath);
            if (hardwareDefinition.Language == "VHDL")
            {
                var vhdlHardwareDefinion = (Hast.Transformer.Vhdl.VhdlHardwareDefinition)hardwareDefinition;
                var vhdl = vhdlHardwareDefinion.Manifest.TopModule.ToVhdl();
                System.IO.File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                new Hast.Transformer.Vhdl.HardwareRepresentationComposer().Compose(vhdlHardwareDefinion);
            }

            return;

            Task.Run(async () =>
                {
                    IHastLayer hastLayer = null;

                    var hardwareAssembly = await hastLayer.GenerateHardware(typeof(PrimeCalculator).Assembly);

                    var primeCalculator = hastLayer.GenerateProxy(hardwareAssembly, new PrimeCalculator());
                    var isPrime = primeCalculator.IsPrimeNumber(15); // Maybe only allow methods that return a Task or its derivatives?

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }
    }
}
