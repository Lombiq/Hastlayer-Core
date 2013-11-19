using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using HastTranspiler;
using System.Diagnostics;

namespace HastConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();

            if (!new CommandLineParser().ParseArguments(args, options))
            {
                Console.WriteLine("Bad arguments.");
                Console.ReadKey();
                return;
            }

            var transpiler = new Transpiler(new HastTranspiler.Vhdl.TranspilingEngine(new HastTranspiler.Vhdl.TranspilingSettings { MaxDegreeOfParallelism = 10 }));
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

            var hardwareDefinition = transpiler.Transpile(csharp, Language.CSharp);
            //var hardwareDefinition = transpiler.Transpile(options.InputFilePath);
            if (hardwareDefinition.Language == "VHDL")
            {
                var vhdlHardwareDefinion = (HastTranspiler.Vhdl.VhdlHardwareDefinition)hardwareDefinition;
                var vhdl = vhdlHardwareDefinion.Manifest.TopModule.ToVhdl();
                File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                new HastTranspiler.Vhdl.HardwareRepresentationComposer().Compose(vhdlHardwareDefinion);
            }

            //Console.ReadKey();
        }
    }
}
