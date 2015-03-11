using System;
using System.IO;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using System.Threading.Tasks;
using Hast.Common.Configuration;

namespace Hast.Transformer
{
    public class DefaultTransformer : ITransformer
    {
        private readonly ITransformingEngine _engine;


        public DefaultTransformer(ITransformingEngine engine)
        {
            _engine = engine;
        }


        public Task<IHardwareDescription> Transform(string assemblyPath, IHardwareGenerationConfiguration configuration)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var astBuilder = new AstBuilder(new DecompilerContext(assembly.MainModule));
            astBuilder.AddAssembly(assembly, onlyAssemblyLevel: false);

            //This would be the decompiled output
            //using (var output = new StringWriter())
            //{
            //    astBuilder.GenerateCode(new PlainTextOutput(output));
            //    var z = output.ToString();
            //    var y = z;
            //}

            return _engine.Transform(assembly.Name.Name, astBuilder.SyntaxTree, configuration);
        }

        public Task<IHardwareDescription> Transform(Assembly assembly, IHardwareGenerationConfiguration configuration)
        {
            if (String.IsNullOrEmpty(assembly.Location)) throw new ArgumentException("The assembly can't be an in-memory one.");

            return Transform(assembly.Location, configuration);
        }
    }
}
