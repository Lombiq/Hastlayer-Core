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
using Hast.Common;
using System.Collections.Generic;
using Orchard.Validation;

namespace Hast.Transformer
{
    public class DefaultTransformer : ITransformer
    {
        private readonly ITransformingEngine _engine;


        public DefaultTransformer(ITransformingEngine engine)
        {
            _engine = engine;
        }


        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            var firstAssembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPaths.First()));
            var transformationId = firstAssembly.FullName;
            var astBuilder = new AstBuilder(new DecompilerContext(firstAssembly.MainModule));
            astBuilder.AddAssembly(firstAssembly);

            foreach (var assemblyPath in assemblyPaths.Skip(1))
            {
                var assembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPath));
                transformationId += "-" + assembly.FullName;
                astBuilder.AddAssembly(assembly);
            }

            //((TypeReferenceExpression)new object()).Type.ToTypeReference().Resolve(null).GetDefinition()

            //astBuilder.AddAssembly(AssemblyDefinition.ReadAssembly(typeof(Math).Assembly.Location));
            //astBuilder.AddType(new TypeDefinition("System", "Math", Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public));

            //This would be the decompiled output
            //using (var output = new StringWriter())
            //{
            //    astBuilder.GenerateCode(new PlainTextOutput(output));
            //    var z = output.ToString();
            //    var y = z;
            //}

            //astBuilder.SyntaxTree.AcceptVisitor(new UnusedTypeDefinitionCleanerAstVisitor());

            return _engine.Transform(transformationId, astBuilder.SyntaxTree, configuration);
        }

        public Task<IHardwareDescription> Transform(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration)
        {
            foreach (var assembly in assemblies)
            {
                if (string.IsNullOrEmpty(assembly.Location))
                {
                    throw new ArgumentException("No assembly used for hardware generation can be an in-memory one, but the assembly named \"" + assembly.FullName + "\" is.");
                }
            }

            return Transform(assemblies.Select(assembly => assembly.Location), configuration);
        }
    }
}
