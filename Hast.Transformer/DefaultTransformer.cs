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


            //astBuilder.SyntaxTree.AcceptVisitor(new UnusedTypeDefinitionCleanerAstVisitor());

            var typeDeclarationLookup = astBuilder.SyntaxTree
                .GetTypes(true)
                .ToDictionary(d => d.Annotation<TypeDefinition>().FullName);

            var context = new TransformationContext
            {
                Id = transformationId,
                HardwareGenerationConfiguration = configuration,
                SyntaxTree = astBuilder.SyntaxTree,
                LookupDeclarationDelegate = type =>
                    {
                        TypeDeclaration declaration;
                        typeDeclarationLookup.TryGetValue(type.Annotation<TypeReference>().FullName, out declaration);
                        return declaration;
                    }
            };

            return _engine.Transform(context);
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
