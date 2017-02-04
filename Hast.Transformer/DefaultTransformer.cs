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
using ICSharpCode.Decompiler.Ast.Transforms;
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
        private readonly IInvocationInstanceCountAdjuster _invocationInstanceCountAdjuster;
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly ITransformingEngine _engine;
        private readonly IGeneratedTaskArraysInliner _generatedTaskArraysInliner;
        private readonly IObjectVariableTypesConverter _objectVariableTypesConverter;
        private readonly IInstanceMethodsToStaticConverter _instanceMethodsToStaticConverter;
        private readonly IArrayInitializerExpander _arrayInitializerExpander;


        public DefaultTransformer(
            ITransformerEventHandler eventHandler,
            IJsonConverter jsonConverter,
            ISyntaxTreeCleaner syntaxTreeCleaner,
            IInvocationInstanceCountAdjuster invocationInstanceCountAdjuster,
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            ITransformingEngine engine,
            IGeneratedTaskArraysInliner generatedTaskArraysInliner,
            IObjectVariableTypesConverter objectVariableTypesConverter,
            IInstanceMethodsToStaticConverter instanceMethodsToStaticConverter,
            IArrayInitializerExpander arrayInitializerExpander)
        {
            _eventHandler = eventHandler;
            _jsonConverter = jsonConverter;
            _syntaxTreeCleaner = syntaxTreeCleaner;
            _invocationInstanceCountAdjuster = invocationInstanceCountAdjuster;
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
            _engine = engine;
            _generatedTaskArraysInliner = generatedTaskArraysInliner;
            _objectVariableTypesConverter = objectVariableTypesConverter;
            _instanceMethodsToStaticConverter = instanceMethodsToStaticConverter;
            _arrayInitializerExpander = arrayInitializerExpander;
        }


        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            var firstAssembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPaths.First()));
            var transformationId = firstAssembly.FullName;
            var decompiledContext = new DecompilerContext(firstAssembly.MainModule);
            decompiledContext.Settings.AnonymousMethods = false;
            var astBuilder = new AstBuilder(decompiledContext);
            astBuilder.AddAssembly(firstAssembly);

            foreach (var assemblyPath in assemblyPaths.Skip(1))
            {
                var assembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPath));
                transformationId += "-" + assembly.FullName;
                astBuilder.AddAssembly(assembly);
            }

            transformationId +=
                string.Join("-", configuration.PublicHardwareMemberFullNames) +
                string.Join("-", configuration.PublicHardwareMemberNamePrefixes) +
                _jsonConverter.Serialize(configuration.CustomConfiguration);


            // astBuilder.RunTransformations() is needed for the syntax tree to be ready, 
            // see: https://github.com/icsharpcode/ILSpy/issues/686. But we can't run that directly since that would
            // also transform some low-level constructs that are useful to have as simple as possible (e.g. it's OK if
            // we only have while statements in the AST, not for statements mixed in). So we need to remove the unuseful
            // pipeline steps and run them by hand.
            var syntaxTree = astBuilder.SyntaxTree;
            IEnumerable<IAstTransform> pipeline = TransformationPipeline.CreatePipeline(decompiledContext);
            // We allow the commented out pipeline steps. Must revisit after ILSpy update.
            pipeline = pipeline
                // Converts e.g. !num6 == 0 expression to num6 != 0 and other simplifications.
                //.Without("PushNegation")

                // Re-creates delegates e.g. from compiler-generated DisplayClasses.
                //.Without("DelegateConstruction")

                // Re-creates e.g. for statements from while statements.
                .Without("PatternStatementTransform")

                // Converts e.g. num6 = num6 + 1; to num6 += 1.
                .Without("ReplaceMethodCallsWithOperators")

                // Deals with the unsafe modifier but we don't support PInvoke any way.
                .Without("IntroduceUnsafeModifier")

                // Re-adds checked() blocks that are used for compile-time overflow checking in C#, see:
                // https://msdn.microsoft.com/en-us/library/74b4xzyw.aspx. We don't need this for transformation.
                .Without("AddCheckedBlocks")

                // Merges separate variable declarations with variable initializations what would make transformation
                // more complicated.
                .Without("DeclareVariables")

                // Removes empty ctors or ctors that can be subsctituted with field initializers.
                //.Without("ConvertConstructorCallIntoInitializer")

                // Converts decimal const fields to more readable variants, e.g. this:
                // [DecimalConstant (0, 0, 0u, 0u, 234u)]
                // private static readonly decimal a = 234m;
                // To this (which is closer to the original):
                // private const decimal a = 234m;
                //.Without("DecimalConstantTransform")

                // Adds using declarations that aren't needed for transformation.
                .Without("IntroduceUsingDeclarations")

                // Converts ExtensionsClass.ExtensionMethod(this) calls to this.ExtensionMethod(). This would make
                // the future transformation of extension methods difficult, since this makes them look like instance
                // methods (however those instance methods don't exist).
                .Without("IntroduceExtensionMethods")

                // These two deal with LINQ elements that we don't support yet any way.
                .Without("IntroduceQueryExpressions")
                .Without("CombineQueryExpressions")

                // Removes an unnecessary BlockStatement level from switch statements.
                //.Without("FlattenSwitchBlocks")
                ;
            foreach (var transform in pipeline)
            {
                transform.Run(syntaxTree);
            }


            // Clean-up.
            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);

            // Transformations making the syntax tree easier to process.
            _generatedTaskArraysInliner.InlineGeneratedTaskArrays(syntaxTree);
            _objectVariableTypesConverter.ConvertObjectVariableTypes(syntaxTree);
            _instanceMethodsToStaticConverter.ConvertInstanceMethodsToStatic(syntaxTree);
            _arrayInitializerExpander.ExpandArrayInitializers(syntaxTree);

            _invocationInstanceCountAdjuster.AdjustInvocationInstanceCounts(syntaxTree, configuration);


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
                    throw new ArgumentException(
                        "No assembly used for hardware generation can be an in-memory one, but the assembly named \"" + 
                        assembly.FullName + 
                        "\" is.");
                }
            }

            return Transform(assemblies.Select(assembly => assembly.Location), configuration);
        }


        private static void CheckSimpleMemoryUsage(SyntaxTree syntaxTree)
        {
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var member in type.Members.Where(m => m.IsInterfaceMember()))
                {
                    if (member.Is<MethodDeclaration>(method => string.IsNullOrEmpty(method.GetSimpleMemoryParameterName())))
                    {
                        throw new InvalidOperationException(
                            "The method " + member.GetFullName() + " doesn't have a necessary SimpleMemory parameter.");
                    }
                }
            }
        }
    }
}
