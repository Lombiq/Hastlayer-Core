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
            var sw = Stopwatch.StartNew();
            var z = IsPrimeNumber(893428937);
            sw.Stop();
            var nanoseconds = (1.0 / Stopwatch.Frequency) * sw.ElapsedTicks * 1000000000;
            var y = z;
            var options = new Options();

            if (!new CommandLineParser().ParseArguments(args, options))
            {
                Console.WriteLine("Bad arguments.");
                Console.ReadKey();
                return;
            }

            var transpiler = new Transpiler(new TranspilingEngine(new TranspilingSettings { MaxDegreeOfParallelism = 10 }));
            var csharp = @"
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
                    }
                }";

            var hardwareDefinition = transpiler.Transpile(csharp, Language.CSharp);
            //var hardwareDefinition = transpiler.Transpile(options.InputFilePath);
            if (hardwareDefinition.Language == "VHDL")
            {
                var vhdlHardwareDefiniont = (HardwareDefinition)hardwareDefinition;
                var vhdl = vhdlHardwareDefiniont.Manifest.TopModule.ToVhdl();
                File.WriteAllText("test.txt", vhdl);
                new HardwareRepresentationComposer().Compose(vhdlHardwareDefiniont);
            }

            //Console.ReadKey();
        }

        static bool IsPrimeNumber(int num)
        {
            var isPrime = true;
            int factor = num / 2;

            for (int i = 2; i <= factor; i++)
            {
                if ((num % i) == 0) isPrime = false;
            }

            return isPrime;
        }
    }
}
