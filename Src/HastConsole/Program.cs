using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using HastTranspiler;
using HastTranspiler.Vhdl;
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

            var transpiler = new Transpiler(new TranspilingEngine(new TranspilingSettings { MaxDegreeOfParallelism = 10 }));
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

                            for (int i = 2; i <= factor; i++)
                            {
                                if ((num % i) == 0) isPrime = false;
                            }

                            return isPrime;
                        }

                        public virtual int[] PrimeFactors(int num)
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
                        }
                    }
                }";

            var hardwareDefinition = transpiler.Transpile(csharp, Language.CSharp);
            //var hardwareDefinition = transpiler.Transpile(options.InputFilePath);
            if (hardwareDefinition.Language == "VHDL")
            {
                var vhdlHardwareDefinion = (VhdlHardwareDefinition)hardwareDefinition;
                var vhdl = vhdlHardwareDefinion.Manifest.TopModule.ToVhdl();
                File.WriteAllText(@"d:\Users\Zoltán\Projects\Saját\Hast\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                new HardwareRepresentationComposer().Compose(vhdlHardwareDefinion);
            }

            //Console.ReadKey();
        }
    }
}
