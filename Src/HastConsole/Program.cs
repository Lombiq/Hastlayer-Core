using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

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


            var path = Path.GetFullPath(options.InputFilePath);

            var assembly = AssemblyDefinition.ReadAssembly(path);
            var astBuilder = new AstBuilder(new DecompilerContext(assembly.MainModule)) { DecompileMethodBodies = true };
            astBuilder.AddAssembly(assembly, onlyAssemblyLevel: false);
            astBuilder.RunTransformations();
            var output = new StringWriter();
            astBuilder.GenerateCode(new PlainTextOutput(output));
            var code = output.ToString();
            Console.Write(code);
            Console.ReadKey();
        }
    }
}
