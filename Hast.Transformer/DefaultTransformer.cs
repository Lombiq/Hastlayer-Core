using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Transformer.Extensibility.Events;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Services;

namespace Hast.Transformer
{
    public class DefaultTransformer : ITransformer
    {
        private readonly ITransformerEventHandler _eventHandler;
        private readonly IJsonConverter _jsonConverter;
        private readonly ISyntaxTreeCleaner _syntaxTreeCleaner;
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly ITransformingEngine _engine;


        public DefaultTransformer(
            ITransformerEventHandler eventHandler,
            IJsonConverter jsonConverter,
            ISyntaxTreeCleaner syntaxTreeCleaner,
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            ITransformingEngine engine)
        {
            _eventHandler = eventHandler;
            _jsonConverter = jsonConverter;
            _syntaxTreeCleaner = syntaxTreeCleaner;
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
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

            transformationId +=
                string.Join("-", configuration.PublicHardwareMembers) +
                string.Join("-", configuration.PublicHardwareMemberPrefixes) +
                _jsonConverter.Serialize(configuration.CustomConfiguration);

            var syntaxTree = astBuilder.SyntaxTree;


            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);


            if (configuration.TransformerConfiguration().UseSimpleMemory)
            {
                CheckSimpleMemoryUsage(syntaxTree);
            }


            var context = new TransformationContext
            {
                Id = transformationId,
                HardwareGenerationConfiguration = configuration,
                SyntaxTree = syntaxTree,
                TypeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree)
            };

            _eventHandler.SyntaxTreeBuilt(context);


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


        private static void CheckSimpleMemoryUsage(SyntaxTree syntaxTree)
        {
            foreach (var type in syntaxTree.GetTypes(true))
            {
                foreach (var member in type.Members.Where(m => m.IsInterfaceMember()))
                {
                    if (member is MethodDeclaration && string.IsNullOrEmpty(((MethodDeclaration)member).GetSimpleMemoryParameterName()))
                    {
                        throw new InvalidOperationException("The method " + member.GetFullName() + " doesn't have a necessary SimpleMemory parameter.");
                    }
                }
            }
        }
    }
}
