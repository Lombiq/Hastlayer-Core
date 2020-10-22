using Hast.Common.Helpers;
using Hast.Common.Services;
using Hast.Layer;
using Hast.Synthesis.Services;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using Hast.Transformer.Services.ConstantValuesSubstitution;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Hast.Transformer
{
    public class DefaultTransformer : ITransformer
    {
        // Set this to true to save the unprocessed and processed syntax tree to files. This is useful for debugging
        // any syntax tree-modifying logic and also to check what an assembly was decompiled into.
        private const bool SaveSyntaxTree =
#if DEBUG
        true;
#else
        false;
#endif

        private readonly IEnumerable<EventHandler<ITransformationContext>> _eventHandlers;
        private readonly IJsonConverter _jsonConverter;
        private readonly ISyntaxTreeCleaner _syntaxTreeCleaner;
        private readonly IInvocationInstanceCountAdjuster _invocationInstanceCountAdjuster;
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly ITransformingEngine _engine;
        private readonly IGeneratedTaskArraysInliner _generatedTaskArraysInliner;
        private readonly IObjectVariableTypesConverter _objectVariableTypesConverter;
        private readonly IInstanceMethodsToStaticConverter _instanceMethodsToStaticConverter;
        private readonly IConstructorsToMethodsConverter _constructorsToMethodsConverter;
        private readonly IConditionalExpressionsToIfElsesConverter _conditionalExpressionsToIfElsesConverter;
        private readonly IConstantValuesSubstitutor _constantValuesSubstitutor;
        private readonly IOperatorsToMethodsConverter _operatorsToMethodsConverter;
        private readonly ICustomPropertiesToMethodsConverter _customPropertiesToMethodsConverter;
        private readonly IImmutableArraysToStandardArraysConverter _immutableArraysToStandardArraysConverter;
        private readonly IDirectlyAccessedNewObjectVariablesCreator _directlyAccessedNewObjectVariablesCreator;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IEmbeddedAssignmentExpressionsExpander _embeddedAssignmentExpressionsExpander;
        private readonly ITransformationContextCacheService _transformationContextCacheService;
        private readonly IMethodInliner _methodInliner;
        private readonly IObjectInitializerExpander _objectInitializerExpander;
        private readonly ITaskBodyInvocationInstanceCountsSetter _taskBodyInvocationInstanceCountsSetter;
        private readonly ISimpleMemoryUsageVerifier _simpleMemoryUsageVerifier;
        private readonly IBinaryAndUnaryOperatorExpressionsCastAdjuster _binaryAndUnaryOperatorExpressionsCastAdjuster;
        private readonly IDeviceDriverSelector _deviceDriverSelector;
        private readonly IFSharpIdiosyncrasiesAdjuster _fSharpIdiosyncrasiesAdjuster;
        private readonly IKnownTypeLookupTableFactory _knownTypeLookupTableFactory;
        private readonly IMemberIdentifiersFixer _memberIdentifiersFixer;
        private readonly IUnneededReferenceVariablesRemover _unneededReferenceVariablesRemover;
        private readonly IRefLocalVariablesRemover _refLocalVariablesRemover;
        private readonly IOptionalParameterFiller _optionalParameterFiller;
        private readonly IReadonlyToConstConverter _readonlyToConstConverter;


        public DefaultTransformer(
            IEnumerable<EventHandler<ITransformationContext>> eventHandlers,
            IJsonConverter jsonConverter,
            ISyntaxTreeCleaner syntaxTreeCleaner,
            IInvocationInstanceCountAdjuster invocationInstanceCountAdjuster,
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            ITransformingEngine engine,
            IGeneratedTaskArraysInliner generatedTaskArraysInliner,
            IObjectVariableTypesConverter objectVariableTypesConverter,
            IInstanceMethodsToStaticConverter instanceMethodsToStaticConverter,
            IConstructorsToMethodsConverter constructorsToMethodsConverter,
            IConditionalExpressionsToIfElsesConverter conditionalExpressionsToIfElsesConverter,
            IConstantValuesSubstitutor constantValuesSubstitutor,
            IOperatorsToMethodsConverter operatorsToMethodsConverter,
            ICustomPropertiesToMethodsConverter customPropertiesToMethodsConverter,
            IImmutableArraysToStandardArraysConverter immutableArraysToStandardArraysConverter,
            IDirectlyAccessedNewObjectVariablesCreator directlyAccessedNewObjectVariablesCreator,
            IAppDataFolder appDataFolder,
            IEmbeddedAssignmentExpressionsExpander embeddedAssignmentExpressionsExpander,
            ITransformationContextCacheService transformationContextCacheService,
            IMethodInliner methodInliner,
            IObjectInitializerExpander objectInitializerExpander,
            ITaskBodyInvocationInstanceCountsSetter taskBodyInvocationInstanceCountsSetter,
            ISimpleMemoryUsageVerifier simpleMemoryUsageVerifier,
            IBinaryAndUnaryOperatorExpressionsCastAdjuster binaryAndUnaryOperatorExpressionsCastAdjuster,
            IDeviceDriverSelector deviceDriverSelector,
            IFSharpIdiosyncrasiesAdjuster fSharpIdiosyncrasiesAdjuster,
            IKnownTypeLookupTableFactory knownTypeLookupTableFactory,
            IMemberIdentifiersFixer memberIdentifiersFixer,
            IUnneededReferenceVariablesRemover unneededReferenceVariablesRemover,
            IRefLocalVariablesRemover refLocalVariablesRemover,
            IOptionalParameterFiller optionalParameterFiller,
            IReadonlyToConstConverter readonlyToConstConverter)
        {
            _eventHandlers = eventHandlers;
            _jsonConverter = jsonConverter;
            _syntaxTreeCleaner = syntaxTreeCleaner;
            _invocationInstanceCountAdjuster = invocationInstanceCountAdjuster;
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
            _engine = engine;
            _generatedTaskArraysInliner = generatedTaskArraysInliner;
            _objectVariableTypesConverter = objectVariableTypesConverter;
            _instanceMethodsToStaticConverter = instanceMethodsToStaticConverter;
            _constructorsToMethodsConverter = constructorsToMethodsConverter;
            _conditionalExpressionsToIfElsesConverter = conditionalExpressionsToIfElsesConverter;
            _constantValuesSubstitutor = constantValuesSubstitutor;
            _operatorsToMethodsConverter = operatorsToMethodsConverter;
            _customPropertiesToMethodsConverter = customPropertiesToMethodsConverter;
            _immutableArraysToStandardArraysConverter = immutableArraysToStandardArraysConverter;
            _directlyAccessedNewObjectVariablesCreator = directlyAccessedNewObjectVariablesCreator;
            _appDataFolder = appDataFolder;
            _embeddedAssignmentExpressionsExpander = embeddedAssignmentExpressionsExpander;
            _transformationContextCacheService = transformationContextCacheService;
            _methodInliner = methodInliner;
            _objectInitializerExpander = objectInitializerExpander;
            _taskBodyInvocationInstanceCountsSetter = taskBodyInvocationInstanceCountsSetter;
            _simpleMemoryUsageVerifier = simpleMemoryUsageVerifier;
            _binaryAndUnaryOperatorExpressionsCastAdjuster = binaryAndUnaryOperatorExpressionsCastAdjuster;
            _deviceDriverSelector = deviceDriverSelector;
            _fSharpIdiosyncrasiesAdjuster = fSharpIdiosyncrasiesAdjuster;
            _knownTypeLookupTableFactory = knownTypeLookupTableFactory;
            _memberIdentifiersFixer = memberIdentifiersFixer;
            _unneededReferenceVariablesRemover = unneededReferenceVariablesRemover;
            _refLocalVariablesRemover = refLocalVariablesRemover;
            _optionalParameterFiller = optionalParameterFiller;
            _readonlyToConstConverter = readonlyToConstConverter;
        }

        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            var transformerConfiguration = configuration.TransformerConfiguration();

            // Need to use assembly names instead of paths for the ID, because paths can change (as in the random ones
            // with Remote Worker). Just file names wouldn't be enough because two assemblies can have the same simple
            // name while their full names being different.
            var transformationIdComponents = new List<string>();

            var decompilers = new List<CSharpDecompiler>();

            foreach (var assemblyPath in assemblyPaths)
            {
                var module = new PEFile(assemblyPath, PEStreamOptions.PrefetchEntireImage);
                transformationIdComponents.Add(module.FullName);

                var resolver = new UniversalAssemblyResolver(
                    Path.GetFullPath(assemblyPath),
                    true,
                    module.Reader.DetectTargetFrameworkId(),
                    PEStreamOptions.PrefetchMetadata);

                // When executed as a Windows service not all Hastlayer assemblies references' from transformed assemblies
                // will be found. Particularly loading Hast.Transformer.Abstractions seems to fail. Also, if a remote
                // transformation needs multiple assemblies those will need to be loaded like this too.
                // So helping the decompiler find them here.
                resolver.AddSearchDirectory(Path.GetDirectoryName(GetType().Assembly.Location));
                var dependenciesFolderPath = _appDataFolder.MapPath("Dependencies");
                if (dependenciesFolderPath != null) resolver.AddSearchDirectory(dependenciesFolderPath);
                resolver.AddSearchDirectory(AppDomain.CurrentDomain.BaseDirectory);
                foreach (var searchPath in assemblyPaths.Select(path => Path.GetDirectoryName(path)).Distinct())
                {
                    resolver.AddSearchDirectory(searchPath);
                }

                // Turning off language features to make processing easier.
                var decompilerSettings = new DecompilerSettings
                {
                    AlwaysShowEnumMemberValues = false,
                    AnonymousMethods = false,
                    AnonymousTypes = false,
                    ArrayInitializers = false,
                    Discards = false,
                    DoWhileStatement = false,
                    Dynamic = false,
                    ExpressionTrees = false,
                    // Instead of extension methods there are simple static methods.
                    ExtensionMethods = false,
                    ForStatement = false,
                    IntroduceReadonlyAndInModifiers = true,
                    IntroduceRefModifiersOnStructs = true,
                    // Turn off shorthand form of increment assignments. With this true e.g. x = x * 2 would be x *= 2.
                    // The former is easier to transform. Works in conjunction with the disabling of
                    // ReplaceMethodCallsWithOperators, see below.
                    IntroduceIncrementAndDecrement = false,
                    LocalFunctions = false,
                    NamedArguments = false,
                    NonTrailingNamedArguments = false,
                    NullPropagation = false,
                    NullableReferenceTypes = false,
                    OptionalArguments = false,
                    OutVariables = false,
                    PatternBasedFixedStatement = false,
                    ReadOnlyMethods = true, // Can help const substitution.
                    RefExtensionMethods = false,
                    SeparateLocalVariableDeclarations = true,
                    ShowXmlDocumentation = false,
                    StringInterpolation = false,
                    TupleComparisons = false,
                    TupleConversions = false,
                    TupleTypes = false,
                    ThrowExpressions = false,
                    UseExpressionBodyForCalculatedGetterOnlyProperties = false,
                    UseLambdaSyntax = false,
                    YieldReturn = false
                };

                var typeSystem = new DecompilerTypeSystem(module, resolver, decompilerSettings);
                var decompiler = new CSharpDecompiler(typeSystem, decompilerSettings);

                // We don't want to run all transforms since they would also transform some low-level constructs that are
                // useful to have as simple as possible (e.g. it's OK if we only have while statements in the AST, not for
                // statements mixed in). So we need to remove the problematic transforms.
                // Must revisit after an ILSpy update.

                decompiler.ILTransforms
                    // InlineReturnTransform might need to be removed: it creates returns with ternary operators and
                    // introduces multiple return statements.

                    // Converts simple while loops into for loops. However, all resulting loops are while (true) ones
                    // with a break statement inside.
                    // Not necessary to remove it with ForStatement = DoWhileStatement = false
                    //.Remove<HighLevelLoopTransform>()

                    // Creates local variables instead of assigning them to DisplayClasses. E.g. instead of:
                    //
                    //      ParallelAlgorithm.<> c__DisplayClass3_0 <> c__DisplayClass3_;
                    //      <> c__DisplayClass3_ = new ParallelAlgorithm.<> c__DisplayClass3_0();
                    //      <> c__DisplayClass3_.input = memory.ReadUInt32(0);
                    //
                    // ...we'd get:
                    //
                    //      uint input;
                    //      input = memory.ReadUInt32(0);
                    //      Func<object, uint> func = default(Func<object, uint>);
                    //      <> c__DisplayClass3_0 @object;
                    //
                    // Note that the DisplayClass is not instantiated either.
                    .Remove("TransformDisplayClassUsage")
                    ;

                decompiler.AstTransforms
                    // Replaces op_* methods with operators but these methods are simpler to transform. Works in
                    // conjunction with IntroduceIncrementAndDecrement = false, see above.
                    .Remove<ReplaceMethodCallsWithOperators>()

                    // Converts e.g. num6 = num6 + 1; to num6 += 1.
                    .Remove("PrettifyAssignments")

                    // Deals with the unsafe modifier but we don't support PInvoke any way.
                    .Remove<IntroduceUnsafeModifier>()

                    // Re-adds checked() blocks that are used for compile-time overflow checking in C#, see:
                    // https://msdn.microsoft.com/en-us/library/74b4xzyw.aspx. We don't need this for transformation.
                    .Remove<AddCheckedBlocks>()

                    // Adds using declarations that aren't needed for transformation.
                    .Remove<IntroduceUsingDeclarations>()

                    // Converts ExtensionsClass.ExtensionMethod(this) calls to this.ExtensionMethod(). This would make
                    // the future transformation of extension methods difficult, since this makes them look like instance
                    // methods (however those instance methods don't exist).
                    .Remove<IntroduceExtensionMethods>()

                    // These two deal with LINQ elements that we don't support yet any way.
                    .Remove<IntroduceQueryExpressions>()
                    .Remove<CombineQueryExpressions>()
                    ;

                decompilers.Add(decompiler);
            }

            transformationIdComponents.AddRange(configuration.HardwareEntryPointMemberFullNames);
            transformationIdComponents.AddRange(configuration.HardwareEntryPointMemberNamePrefixes);
            transformationIdComponents.Add(_jsonConverter.Serialize(configuration.CustomConfiguration));

            // Adding the assembly name so the Hastlayer version is included too, to prevent stale caches after a
            // Hastlayer update.
            transformationIdComponents.Add(GetType().Assembly.FullName);

            // Adding the device name to ensure different a cached program for a different hardware doesn't get used.
            transformationIdComponents.Add(configuration.DeviceName);

#if DEBUG
            File.WriteAllLines("HashSource.txt", transformationIdComponents);
#endif

            var transformationId = Sha2456Helper.ComputeHash(string.Join("\n", transformationIdComponents));

            if (configuration.EnableCaching)
            {
                var cachedTransformationContext = _transformationContextCacheService
                    .GetTransformationContext(assemblyPaths, transformationId);

                if (cachedTransformationContext != null) return _engine.Transform(cachedTransformationContext);
            }

            var decompilerTasks = decompilers
                .Select(decompiler => Task.Run(() => decompiler.DecompileWholeModuleAsSingleFile(true)))
                .ToArray();

            Task.WaitAll(decompilerTasks);

            // Unlike with the ILSpy v2 libraries multiple unrelated assemblies can't be decompiled into a single AST
            // so we need to decompile them separately and merge them like this.
            var syntaxTree = decompilerTasks[0].Result;
            for (int i = 1; i < decompilerTasks.Length; i++)
            {
                syntaxTree.Members.AddRange(decompilerTasks[i].Result.Members.Select(member => member.Detach()));
            }


            if (SaveSyntaxTree) WriteSyntaxTree(syntaxTree, "UnprocessedSyntaxTree.cs");

            // Since this is about known (i.e. .NET built-in) types it doesn't matter which type system we use.
            var knownTypeLookupTable = _knownTypeLookupTableFactory.Create(decompilers.First().TypeSystem);

            _memberIdentifiersFixer.FixMemberIdentifiers(syntaxTree);
            _fSharpIdiosyncrasiesAdjuster.AdjustFSharpIdiosyncrasies(syntaxTree);

            // Removing the unnecessary bits.
            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);

            _readonlyToConstConverter.ConvertReadonlyPrimitives(syntaxTree, configuration);

            // Conversions making the syntax tree easier to process. Note that the order is NOT arbitrary but these
            // services sometimes depend on each other.
            _immutableArraysToStandardArraysConverter.ConvertImmutableArraysToStandardArrays(syntaxTree, knownTypeLookupTable);
            _binaryAndUnaryOperatorExpressionsCastAdjuster.AdjustBinaryAndUnaryOperatorExpressions(syntaxTree, knownTypeLookupTable);
            _generatedTaskArraysInliner.InlineGeneratedTaskArrays(syntaxTree);
            _objectVariableTypesConverter.ConvertObjectVariableTypes(syntaxTree);
            _constructorsToMethodsConverter.ConvertConstructorsToMethods(syntaxTree);
            _operatorsToMethodsConverter.ConvertOperatorsToMethods(syntaxTree);
            _customPropertiesToMethodsConverter.ConvertCustomPropertiesToMethods(syntaxTree);
            _instanceMethodsToStaticConverter.ConvertInstanceMethodsToStatic(syntaxTree);
            _conditionalExpressionsToIfElsesConverter.ConvertConditionalExpressionsToIfElses(syntaxTree);
            _directlyAccessedNewObjectVariablesCreator.CreateVariablesForDirectlyAccessedNewObjects(syntaxTree);
            _objectInitializerExpander.ExpandObjectInitializers(syntaxTree);
            _embeddedAssignmentExpressionsExpander.ExpandEmbeddedAssignmentExpressions(syntaxTree);
            // Needs to run before method inlining but after anything that otherwise modified method signatures or
            // invocations.
            _optionalParameterFiller.FillOptionalParamters(syntaxTree);
            if (transformerConfiguration.EnableMethodInlining) _methodInliner.InlineMethods(syntaxTree, configuration);
            // This needs to run before UnneededReferenceVariablesRemover.
            _refLocalVariablesRemover.RemoveRefLocalVariables(syntaxTree);
            _unneededReferenceVariablesRemover.RemoveUnneededVariables(syntaxTree);

            var preConfiguredArrayLengths = configuration
                .TransformerConfiguration()
                .ArrayLengths
                .ToDictionary(kvp => kvp.Key, kvp => (IArraySize)new ArraySize { Length = kvp.Value });
            var arraySizeHolder = new ArraySizeHolder(preConfiguredArrayLengths);
            if (transformerConfiguration.EnableConstantSubstitution)
            {
                _constantValuesSubstitutor.SubstituteConstantValues(syntaxTree, arraySizeHolder, configuration, knownTypeLookupTable);
            }

            // Needs to run after const substitution.
            _taskBodyInvocationInstanceCountsSetter.SetTaskBodyInvocationInstanceCounts(syntaxTree, configuration);

            // If the conversions removed something let's clean them up here.
            _syntaxTreeCleaner.CleanUnusedDeclarations(syntaxTree, configuration);

            if (SaveSyntaxTree) WriteSyntaxTree(syntaxTree, "ProcessedSyntaxTree.cs");

            _invocationInstanceCountAdjuster.AdjustInvocationInstanceCounts(syntaxTree, configuration);


            if (transformerConfiguration.UseSimpleMemory)
            {
                _simpleMemoryUsageVerifier.VerifySimpleMemoryUsage(syntaxTree);
            }

            var deviceDriver = _deviceDriverSelector.GetDriver(configuration.DeviceName);

            if (deviceDriver == null)
            {
                throw new InvalidOperationException(
                    "No device driver with the name \"" + configuration.DeviceName + "\" was found.");
            }

            var context = new TransformationContext
            {
                Id = transformationId,
                HardwareGenerationConfiguration = configuration,
                SyntaxTree = syntaxTree,
                TypeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree),
                KnownTypeLookupTable = knownTypeLookupTable,
                ArraySizeHolder = arraySizeHolder,
                DeviceDriver = deviceDriver
            };

            foreach (var eventHandler in _eventHandlers) eventHandler?.Invoke(this, context);

            if (configuration.EnableCaching)
            {
                _transformationContextCacheService.SetTransformationContext(context, assemblyPaths);
            }

            return _engine.Transform(context);
        }

        private void WriteSyntaxTree(SyntaxTree syntaxTree, string fileName)
        {
            while (true)
            {
                try
                {
                    File.WriteAllText(fileName, syntaxTree.ToString());
                    return;
                }
                catch (IOException) { }
            }
        }
    }
}
