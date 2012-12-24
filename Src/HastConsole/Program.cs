using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using HastTranspiler;
using HastTranspiler.Vhdl;

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
                namespace TestNamespace
                {
                    public class SimpleClass
                    {
                        public virtual int CalcMethod(int number)
                        {
                            number += 1;
                            number += Two();
                            return number;
                        }

                        public virtual int StaticMethod()
                        {
                            return CalcMethod(Two());
                        }

                        int Two()
                        {
                            return 2;
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
    }
}
