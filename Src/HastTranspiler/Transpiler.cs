using System;
using System.IO;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace HastTranspiler
{
    public class Transpiler : ITranspiler
    {
        private readonly ITranspilingEngine _engine;


        public Transpiler(ITranspilingEngine engine)
        {
            _engine = engine;
        }


        public IHardwareDefinition Transpile(string assemplyPath)
        {
            assemplyPath = Path.GetFullPath(assemplyPath);

            var assembly = AssemblyDefinition.ReadAssembly(assemplyPath);
            var astBuilder = new AstBuilder(new DecompilerContext(assembly.MainModule));
            astBuilder.AddAssembly(assembly, onlyAssemblyLevel: false);

            //using (var output = new StringWriter())
            //{
            //    astBuilder.GenerateCode(new PlainTextOutput(output));
            //    var z = output.ToString();
            //    var y = z;
            //}

            return _engine.Transpile(assembly.Name.Name, astBuilder.SyntaxTree);
        }

        public IHardwareDefinition Transpile(Assembly assembly)
        {
            if (String.IsNullOrEmpty(assembly.Location)) throw new ArgumentException("The assembly can't be an in-memory one.");

            return Transpile(assembly.Location);
        }
    }
}
