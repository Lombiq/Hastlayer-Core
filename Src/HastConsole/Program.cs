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

            var transpiler = new Transpiler(new TranspilingEngine());
            var csharp = @"
                public class SimpleClass
                {
                    public int CalcMethod(int number)
                    {
                        var temp = 10;
                        temp += number + 15;
                        temp++;
                        return temp;
                    }
                }";

            var vhdl = transpiler.Transpile(csharp, Language.CSharp);

            //var vhdl = transpiler.Transpile(options.InputFilePath);
            Console.Write(vhdl);
            Console.ReadKey();
        }
    }
}
