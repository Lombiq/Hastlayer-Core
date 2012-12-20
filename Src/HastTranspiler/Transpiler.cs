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


        public string Transpile(string assemplyPath)
        {
            assemplyPath = Path.GetFullPath(assemplyPath);

            var assembly = AssemblyDefinition.ReadAssembly(assemplyPath);
            var astBuilder = new AstBuilder(new DecompilerContext(assembly.MainModule)) { DecompileMethodBodies = true };
            astBuilder.AddAssembly(assembly, onlyAssemblyLevel: false);

            return _engine.Transpile(astBuilder.SyntaxTree);
        }

        public string Transpile(Assembly assembly)
        {
            if (String.IsNullOrEmpty(assembly.Location)) throw new ArgumentException("The assembly can't be an in-memory one.");

            return Transpile(assembly.Location);
        }
    }
}
