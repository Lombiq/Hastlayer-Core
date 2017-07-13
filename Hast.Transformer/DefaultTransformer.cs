﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Extensibility.Events;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using Hast.Transformer.Services.ConstantValuesSubstitution;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.FileSystems.AppData;
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
        private readonly IAutoPropertyInitializationFixer _autoPropertyInitializationFixer;
        private readonly IConstructorsToMethodsConverter _constructorsToMethodsConverter;
        private readonly IConditionalExpressionsToIfElsesConverter _conditionalExpressionsToIfElsesConverter;
        private readonly IConstantValuesSubstitutor _constantValuesSubstitutor;
        private readonly IOperatorsToMethodsConverter _operatorsToMethodsConverter;
        private readonly IOperatorAssignmentsToSimpleAssignmentsConverter _operatorAssignmentsToSimpleAssignmentsConverter;
        private readonly ICustomPropertiesToMethodsConverter _customPropertiesToMethodsConverter;
        private readonly IImmutableArraysToStandardArraysConverter _immutableArraysToStandardArraysConverter;
        private readonly IDirectlyAccessedNewObjectVariablesCreator _directlyAccessedNewObjectVariablesCreator;
        private readonly IAppDataFolder _appDataFolder;


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
            IArrayInitializerExpander arrayInitializerExpander,
            IAutoPropertyInitializationFixer autoPropertyInitializationFixer,
            IConstructorsToMethodsConverter constructorsToMethodsConverter,
            IConditionalExpressionsToIfElsesConverter conditionalExpressionsToIfElsesConverter,
            IConstantValuesSubstitutor constantValuesSubstitutor,
            IOperatorsToMethodsConverter operatorsToMethodsConverter,
            IOperatorAssignmentsToSimpleAssignmentsConverter operatorAssignmentsToSimpleAssignmentsConverter,
            ICustomPropertiesToMethodsConverter customPropertiesToMethodsConverter,
            IImmutableArraysToStandardArraysConverter immutableArraysToStandardArraysConverter,
            IDirectlyAccessedNewObjectVariablesCreator directlyAccessedNewObjectVariablesCreator,
            IAppDataFolder appDataFolder)
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
            _autoPropertyInitializationFixer = autoPropertyInitializationFixer;
            _constructorsToMethodsConverter = constructorsToMethodsConverter;
            _conditionalExpressionsToIfElsesConverter = conditionalExpressionsToIfElsesConverter;
            _constantValuesSubstitutor = constantValuesSubstitutor;
            _operatorsToMethodsConverter = operatorsToMethodsConverter;
            _operatorAssignmentsToSimpleAssignmentsConverter = operatorAssignmentsToSimpleAssignmentsConverter;
            _customPropertiesToMethodsConverter = customPropertiesToMethodsConverter;
            _immutableArraysToStandardArraysConverter = immutableArraysToStandardArraysConverter;
            _directlyAccessedNewObjectVariablesCreator = directlyAccessedNewObjectVariablesCreator;
            _appDataFolder = appDataFolder;
        }


        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            // When executed as a Windows service not all Hastlayer assemblies references from transformed assemblies
            // will be found. Particularly loading Hast.Transformer.Abstractions seems to fail. So helping Cecil found
            // it here.
            var resolver = new AssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(GetType().Assembly.Location));
            resolver.AddSearchDirectory(_appDataFolder.MapPath("Dependencies"));
            resolver.AddSearchDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var parameters = new ReaderParameters
            {
                AssemblyResolver = resolver,
            };

            var firstAssembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPaths.First()), parameters);
            var transformationId = firstAssembly.FullName;
            var decompiledContext = new DecompilerContext(firstAssembly.MainModule);

            var decompilerSettings = decompiledContext.Settings;
            decompilerSettings.AnonymousMethods = false;

            var astBuilder = new AstBuilder(decompiledContext);
            astBuilder.AddAssembly(firstAssembly);

            foreach (var assemblyPath in assemblyPaths.Skip(1))
            {
                var assembly = AssemblyDefinition.ReadAssembly(Path.GetFullPath(assemblyPath), parameters);
                transformationId += "-" + assembly.FullName;
                astBuilder.AddAssembly(assembly);
            }

            transformationId +=
                string.Join("-", configuration.HardwareEntryPointMemberFullNames) +
                string.Join("-", configuration.HardwareEntryPointMemberNamePrefixes) +
                _jsonConverter.Serialize(configuration.CustomConfiguration) +
                // Adding the assembly name so the Hastlayer version is included too, to prevent stale caches after a 
                // Hastlayer update.
                GetType().Assembly.FullName;


            // astBuilder.RunTransformations() is needed for the syntax tree to be ready, 
            // see: https://github.com/icsharpcode/ILSpy/issues/686. But we can't run that directly since that would
            // also transform some low-level constructs that are useful to have as simple as possible (e.g. it's OK if
            // we only have while statements in the AST, not for statements mixed in). So we need to remove the not 
            // useful pipeline steps and run them by hand.
            var syntaxTree = astBuilder.SyntaxTree;

            IEnumerable<IAstTransform> pipeline = TransformationPipeline.CreatePipeline(decompiledContext);
            // We allow the commented out pipeline steps. Must revisit after ILSpy update.
            pipeline = pipeline
                // Converts e.g. !num6 == 0 expression to num6 != 0 and other simplifications.
                //.Without("PushNegation")

                // Re-creates delegates e.g. from compiler-generated DisplayClasses.
                //.Without("DelegateConstruction")

                // Re-creates e.g. for statements from while statements. Instead we use NoForPatternStatementTransform.
                .Without("PatternStatementTransform")
                .Union(new[] { new NoForPatternStatementTransform(decompiledContext) })

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

                // Removes empty ctors or ctors that can be substituted with field initializers.
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


            _autoPropertyInitializationFixer.FixAutoPropertyInitializations(syntaxTree);

            File.WriteAllText("source-orig.cs", syntaxTree.ToString());
            // Removing the unnecessary bits.
            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);
            File.WriteAllText("source.cs", syntaxTree.ToString());

            // Conversions making the syntax tree easier to process.
            _immutableArraysToStandardArraysConverter.ConvertImmutableArraysToStandardArrays(syntaxTree);
            _generatedTaskArraysInliner.InlineGeneratedTaskArrays(syntaxTree);
            _objectVariableTypesConverter.ConvertObjectVariableTypes(syntaxTree);
            _constructorsToMethodsConverter.ConvertConstructorsToMethods(syntaxTree);
            _operatorsToMethodsConverter.ConvertOperatorsToMethods(syntaxTree);
            _customPropertiesToMethodsConverter.ConvertCustomPropertiesToMethods(syntaxTree);
            _instanceMethodsToStaticConverter.ConvertInstanceMethodsToStatic(syntaxTree);
            _arrayInitializerExpander.ExpandArrayInitializers(syntaxTree);
            _conditionalExpressionsToIfElsesConverter.ConvertConditionalExpressionsToIfElses(syntaxTree);
            _operatorAssignmentsToSimpleAssignmentsConverter.ConvertOperatorAssignmentExpressionsToSimpleAssignments(syntaxTree);
            _directlyAccessedNewObjectVariablesCreator.CreateVariablesForDirectlyAccessedNewObjects(syntaxTree);
            var arraySizeHolder = _constantValuesSubstitutor.SubstituteConstantValues(syntaxTree, configuration);

            // If the conversions removed something let's clean them up here.
            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);

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
                TypeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree),
                ArraySizeHolder = arraySizeHolder
            };

            _eventHandler.SyntaxTreeBuilt(context);

            return _engine.Transform(context);
        }


        private static void CheckSimpleMemoryUsage(SyntaxTree syntaxTree)
        {
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var member in type.Members.Where(m => m.IsHardwareEntryPointMember()))
                {
                    var method = member as MethodDeclaration;

                    if (method != null)
                    {
                        var methodName = member.GetFullName();

                        if (string.IsNullOrEmpty(method.GetSimpleMemoryParameterName()))
                        {
                            throw new InvalidOperationException(
                                "The method " + methodName + " doesn't have a necessary SimpleMemory parameter. Hardware entry points should have one.");
                        }

                        if (method.Parameters.Count > 1)
                        {
                            throw new InvalidOperationException(
                                "The method " + methodName + " contains parameters apart from the SimpleMemory parameter. Hardware entry points should only have a single SimpleMemory parameter and nothing else.");
                        }
                    }
                }
            }
        }


        public class AssemblyResolver : DefaultAssemblyResolver
        {
            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                try
                {
                    return base.Resolve(name);
                }
                catch { }
                return null;
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                try
                {
                    return base.Resolve(name, parameters);
                }
                catch { }
                return null;
            }

            public override AssemblyDefinition Resolve(string fullName)
            {
                try
                {
                    return base.Resolve(fullName);
                }
                catch { }
                return null;
            }

            public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                try
                {
                    return base.Resolve(fullName, parameters);
                }
                catch { }
                return null;
            }
        }
    }
}
