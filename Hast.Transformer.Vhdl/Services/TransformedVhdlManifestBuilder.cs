using Hast.Common.Extensions;
using Hast.Common.Services;
using Hast.Layer;
using Hast.Synthesis.Abstractions;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.InvocationProxyBuilders;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SimpleMemory;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.Transformer.Vhdl.Verifiers;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Services
{
    public class TransformedVhdlManifestBuilder : ITransformedVhdlManifestBuilder
    {
        private readonly ICompilerGeneratedClassesVerifier _compilerGeneratedClassesVerifier;
        private readonly IHardwareEntryPointsVerifier _hardwareEntryPointsVerifier;
        private readonly IClock _clock;
        private readonly IArrayTypesCreator _arrayTypesCreator;
        private readonly IMethodTransformer _methodTransformer;
        private readonly IDisplayClassFieldTransformer _displayClassFieldTransformer;
        private readonly IExternalInvocationProxyBuilder _externalInvocationProxyBuilder;
        private readonly IInternalInvocationProxyBuilder _internalInvocationProxyBuilder;
        private readonly Lazy<ISimpleMemoryComponentBuilder> _simpleMemoryComponentBuilderLazy;
        private readonly IEnumTypesCreator _enumTypesCreator;
        private readonly IPocoTransformer _pocoTransformer;
        private readonly IRemainderOperatorExpressionsExpander _remainderOperatorExpressionsExpander;
        private readonly IUnsupportedConstructsVerifier _unsupportedConstructsVerifier;

        public TransformedVhdlManifestBuilder(
            ICompilerGeneratedClassesVerifier compilerGeneratedClassesVerifier,
            IHardwareEntryPointsVerifier hardwareEntryPointsVerifier,
            IClock clock,
            IArrayTypesCreator arrayTypesCreator,
            IMethodTransformer methodTransformer,
            IDisplayClassFieldTransformer displayClassFieldTransformer,
            IExternalInvocationProxyBuilder externalInvocationProxyBuilder,
            IInternalInvocationProxyBuilder internalInvocationProxyBuilder,
            Lazy<ISimpleMemoryComponentBuilder> simpleMemoryComponentBuilderLazy,
            IEnumTypesCreator enumTypesCreator,
            IPocoTransformer pocoTransformer,
            IRemainderOperatorExpressionsExpander remainderOperatorExpressionsExpander,
            IUnsupportedConstructsVerifier unsupportedConstructsVerifier)
        {
            _compilerGeneratedClassesVerifier = compilerGeneratedClassesVerifier;
            _hardwareEntryPointsVerifier = hardwareEntryPointsVerifier;
            _clock = clock;
            _arrayTypesCreator = arrayTypesCreator;
            _methodTransformer = methodTransformer;
            _displayClassFieldTransformer = displayClassFieldTransformer;
            _externalInvocationProxyBuilder = externalInvocationProxyBuilder;
            _internalInvocationProxyBuilder = internalInvocationProxyBuilder;
            _simpleMemoryComponentBuilderLazy = simpleMemoryComponentBuilderLazy;
            _enumTypesCreator = enumTypesCreator;
            _pocoTransformer = pocoTransformer;
            _remainderOperatorExpressionsExpander = remainderOperatorExpressionsExpander;
            _unsupportedConstructsVerifier = unsupportedConstructsVerifier;
        }

        public async Task<ITransformedVhdlManifest> BuildManifestAsync(ITransformationContext transformationContext)
        {
            var syntaxTree = transformationContext.SyntaxTree;

            // Running verifications.
            _unsupportedConstructsVerifier.ThrowIfUnsupportedConstructsFound(syntaxTree);
            _compilerGeneratedClassesVerifier.VerifyCompilerGeneratedClasses(syntaxTree);
            _hardwareEntryPointsVerifier.VerifyHardwareEntryPoints(syntaxTree, transformationContext.TypeDeclarationLookupTable);

            // Running AST changes.
            _remainderOperatorExpressionsExpander.ExpandRemainderOperatorExpressions(syntaxTree);

            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext);
            var useSimpleMemory = transformationContext.GetTransformerConfiguration().UseSimpleMemory;

            var hastIpArchitecture = new Architecture { Name = "Imp" };
            var hastIpModule = new VhdlBuilder.Representation.Declaration.Module { Architecture = hastIpArchitecture };

            // Adding libraries
            hastIpModule.Libraries.Add(new Library(
                name: "ieee",
                uses: new List<string> { "std_logic_1164.all", "numeric_std.all" }));

            // Creating the Hast_IP entity. Its name can't be an extended identifier.
            var hastIpEntity = hastIpModule.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            hastIpArchitecture.Entity = hastIpEntity;

            var generationDateTimeUtcText = _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";

            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Hast_IP ID: " + transformationContext.Id));
            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Date and time: " + generationDateTimeUtcText));
            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Generated by Hastlayer - hastlayer.com"));

            var portsComment = useSimpleMemory ? LongGeneratedSimpleMemoryCodeComments.Ports : LongGeneratedCodeComments.Ports;
            hastIpEntity.Declarations.Add(new LogicalBlock(new BlockComment(portsComment)));

            hastIpArchitecture.Declarations.Add(new BlockComment(LongGeneratedCodeComments.Overview));

            var dependentTypesTables = new List<DependentTypesTable>();

            // Adding array types for any arrays created in code.
            // This is necessary in a separate step because in VHDL the array types themselves should be created too
            // (like in C# we'd need to first define what an int[] is before being able to create one).
            var arrayTypeDependentTypes = new DependentTypesTable();
            foreach (var arrayDeclaration in _arrayTypesCreator.CreateArrayTypes(syntaxTree, vhdlTransformationContext))
            {
                arrayTypeDependentTypes.AddDependency(arrayDeclaration, arrayDeclaration.ElementType.Name);
            }

            arrayTypeDependentTypes.AddToIfNotEmpty(dependentTypesTables);

            // Adding enum types.
            var enumDeclarations = _enumTypesCreator.CreateEnumTypes(syntaxTree);
            if (enumDeclarations.Any())
            {
                var enumDeclarationsBlock = new LogicalBlock(new LineComment("Enum declarations start"));
                enumDeclarationsBlock.Body.AddRange(enumDeclarations);
                enumDeclarationsBlock.Add(new LineComment("Enum declarations end"));
                hastIpArchitecture.Declarations.Add(enumDeclarationsBlock);
            }

            // Doing transformations.
            var transformerResults = await Task.WhenAll(TransformMembers(transformationContext.SyntaxTree, vhdlTransformationContext));
            var warnings = new List<ITransformationWarning>();
            var potentiallyInvokingArchitectureComponents = transformerResults
                .SelectMany(result => result
                    .ArchitectureComponentResults
                    .Select(componentResult =>
                    {
                        warnings.AddRange(componentResult.Warnings);
                        return componentResult.ArchitectureComponent;
                    }))
                .ToList();
            var architectureComponentResults = transformerResults
                .SelectMany(transformerResult => transformerResult.ArchitectureComponentResults)
                .ToList();
            foreach (var architectureComponentResult in architectureComponentResults)
            {
                if (architectureComponentResult.ArchitectureComponent.DependentTypesTable.Types.Any())
                {
                    dependentTypesTables.Add(architectureComponentResult.ArchitectureComponent.DependentTypesTable);
                }
            }

            XdcFile xdcFile = null;
            if (transformationContext.DeviceDriver.DeviceManifest.ToolChainName == CommonToolChainNames.QuartusPrime)
            {
                BuildManifestQuartusPrime(architectureComponentResults, hastIpArchitecture);
            }
            else if (transformationContext.DeviceDriver.DeviceManifest.UsesVivadoInToolChain())
            {
                xdcFile = BuildManifestVivado(architectureComponentResults, hastIpArchitecture);
            }

            // Processing inter-dependent types. In VHDL if a type depends another type (e.g. an array stores elements
            // of a record type) than the type depending on the other one should come after the other one in the code
            // file.
            var allDependentTypes = dependentTypesTables
                .SelectMany(table => table.Types)
                .GroupBy(type => type.Name) // A dependency relation can be present multiple times, so need to group first.
                .ToDictionary(group => group.Key, group => group.First());
            var sortedDependentTypes = TopologicalSortHelper.Sort(
                allDependentTypes.Values,
                sortedType => dependentTypesTables
                    .SelectMany(table => table.GetDependencies(sortedType))
                    .Where(type => type != null && allDependentTypes.ContainsKey(type))
                    .Select(type => allDependentTypes[type]));
            if (sortedDependentTypes.Any())
            {
                var dependentTypesDeclarationsBlock = new LogicalBlock(new LineComment("Custom inter-dependent type declarations start"));
                dependentTypesDeclarationsBlock.Body.AddRange(sortedDependentTypes);
                dependentTypesDeclarationsBlock.Add(new LineComment("Custom inter-dependent type declarations end"));
                hastIpArchitecture.Declarations.Add(dependentTypesDeclarationsBlock);
            }

            // Adding architecture component declarations. These should come after custom inter-dependent type declarations.
            foreach (var architectureComponentResult in architectureComponentResults)
            {
                hastIpArchitecture.Declarations.Add(architectureComponentResult.Declarations);
                hastIpArchitecture.Add(architectureComponentResult.Body);
            }

            // Proxying external invocations.
            var hardwareEntryPointMemberResults = transformerResults
                .Where(result => result.IsHardwareEntryPointMember)
                .ToList();
            if (!hardwareEntryPointMemberResults.Any())
            {
                throw new InvalidOperationException(
                    "There aren't any hardware entry point members, however at least one is needed to execute " +
                    "anything on hardware. Did you forget to pass all the assemblies to Hastlayer? Are there " +
                    "methods suitable as hardware entry points (see the documentation)?");
            }

            var memberIdTable = BuildMemberIdTable(hardwareEntryPointMemberResults);
            var externalInvocationProxy = _externalInvocationProxyBuilder.BuildProxy(hardwareEntryPointMemberResults, memberIdTable);
            potentiallyInvokingArchitectureComponents.Add(externalInvocationProxy);
            hastIpArchitecture.Declarations.Add(externalInvocationProxy.BuildDeclarations());
            hastIpArchitecture.Add(externalInvocationProxy.BuildBody());

            // Proxying internal invocations.
            var internaInvocationProxies = _internalInvocationProxyBuilder.BuildProxy(
                potentiallyInvokingArchitectureComponents,
                vhdlTransformationContext);
            foreach (var proxy in internaInvocationProxies)
            {
                hastIpArchitecture.Declarations.Add(proxy.BuildDeclarations());
                hastIpArchitecture.Add(proxy.BuildBody());
            }

            // Proxying SimpleMemory operations.
            if (useSimpleMemory)
            {
                _simpleMemoryComponentBuilderLazy.Value.AddSimpleMemoryComponentsToArchitecture(
                    potentiallyInvokingArchitectureComponents,
                    hastIpArchitecture);
            }

            // Adding common ports.
            var ports = hastIpEntity.Ports;
            ports.Add(new Port
            {
                Name = CommonPortNames.MemberId,
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt,
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Reset,
                Mode = PortMode.In,
                DataType = KnownDataTypes.StdLogic,
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Started,
                Mode = PortMode.In,
                DataType = KnownDataTypes.Boolean,
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Finished,
                Mode = PortMode.Out,
                DataType = KnownDataTypes.Boolean,
            });

            ProcessUtility.AddClockToProcesses(hastIpModule, CommonPortNames.Clock);

            var manifest = new VhdlManifest();

            manifest.Modules.Add(new UnOmittableBlockComment(
                new[] { "Generated by Hastlayer (hastlayer.com) at " + generationDateTimeUtcText + " for the following hardware entry points: " }
                .Union(hardwareEntryPointMemberResults.Select(result => "* " + result.Member.GetFullName()))
                .ToArray()));
            manifest.Modules.Add(new Raw(Environment.NewLine));

            manifest.Modules.Add(new BlockComment(LongGeneratedCodeComments.Libraries));

            // If the TypeConversion functions change those changes need to be applied to the Timing Tester app too.
            ReadAndAddEmbedLibrary("TypeConversion", manifest, hastIpModule);

            if (useSimpleMemory)
            {
                ReadAndAddEmbedLibrary("SimpleMemory", manifest, hastIpModule);
            }

            manifest.Modules.Add(new LineComment("Hast_IP, logic generated from the input .NET assemblies starts here."));
            manifest.Modules.Add(hastIpModule);

            return new TransformedVhdlManifest
            {
                Manifest = manifest,
                MemberIdTable = memberIdTable,
                Warnings = warnings,
                XdcFile = xdcFile,
            };
        }

        private static XdcFile BuildManifestVivado(
            IEnumerable<IArchitectureComponentResult> architectureComponentResults,
            Architecture hastIpArchitecture)
        {
            // Adding multi-cycle path constraints for Vivado.

            var xdcFile = new XdcFile();

            var anyMultiCycleOperations = false;

            foreach (var architectureComponentResult in architectureComponentResults)
            {
                foreach (var operation in architectureComponentResult.ArchitectureComponent.MultiCycleOperations)
                {
                    xdcFile.AddPath(operation.OperationResultReference, operation.RequiredClockCyclesCeiling, true);
                    anyMultiCycleOperations = true;
                }
            }

            if (anyMultiCycleOperations)
            {
                hastIpArchitecture.Declarations.Add(new LogicalBlock(
                    new LineComment("When put on variables and signals this attribute instructs Vivado not to merge them, thus allowing us to define multi-cycle paths properly."),
                    KnownDataTypes.DontTouchAttribute));
            }

            return xdcFile;
        }

        private static void BuildManifestQuartusPrime(
            IEnumerable<IArchitectureComponentResult> architectureComponentResults,
            Architecture hastIpArchitecture)
        {
            // Adding multi-cycle path constraints for Quartus.

            var anyMultiCycleOperations = false;
            var sdcExpression = new MultiCycleSdcStatementsAttributeExpression();

            foreach (var architectureComponentResult in architectureComponentResults)
            {
                foreach (var operation in architectureComponentResult.ArchitectureComponent.MultiCycleOperations)
                {
                    // If the path is through a global signal (i.e. that doesn't have a parent process) then
                    // the parent should be empty.
                    sdcExpression.AddPath(
                        operation.OperationResultReference.DataObjectKind == DataObjectKind.Variable ?
                            ProcessUtility.FindProcesses(new[] { architectureComponentResult.Body }).Single().Name :
                            string.Empty,
                        operation.OperationResultReference,
                        operation.RequiredClockCyclesCeiling);

                    anyMultiCycleOperations = true;
                }
            }

            if (anyMultiCycleOperations)
            {
                var alteraAttribute = new VhdlBuilder.Representation.Declaration.Attribute
                {
                    Name = "altera_attribute",
                    ValueType = KnownDataTypes.UnrangedString,
                };

                hastIpArchitecture.Declarations.Add(new LogicalBlock(
                    new LineComment("Adding multi-cycle path constraints for Quartus Prime. See: " +
                    "https://www.intel.com/content/www/us/en/programmable/support/support-resources/knowledge-base/solutions/rd05162013_635.html"),
                    alteraAttribute,
                    new AttributeSpecification
                    {
                        Attribute = alteraAttribute,
                        Of = hastIpArchitecture.ToReference(),
                        ItemClass = "architecture",
                        Expression = sdcExpression,
                    }));
            }
        }

        private IEnumerable<Task<IMemberTransformerResult>> TransformMembers(
            AstNode node,
            VhdlTransformationContext transformationContext,
            List<Task<IMemberTransformerResult>> memberTransformerTasks = null)
        {
            memberTransformerTasks ??= new List<Task<IMemberTransformerResult>>();

            var traverseTo = node.Children;

            // If for debugging you want to make the below processing serial instead of it running in parallel then
            // add .Result to every transformation call and wrap them into Task.FromResult() methods.
            return node.NodeType switch
            {
                NodeType.Member =>
                    TransformSingleMember(node, memberTransformerTasks, traverseTo, transformationContext),
                NodeType.TypeDeclaration =>
                    TransformTypeDeclaration(node, memberTransformerTasks, traverseTo, transformationContext),
                _ => TransformChildMembers(memberTransformerTasks, traverseTo, transformationContext),
            };
        }

        private IEnumerable<Task<IMemberTransformerResult>> TransformSingleMember(
            AstNode node,
            List<Task<IMemberTransformerResult>> memberTransformerTasks,
            IEnumerable<AstNode> traverseTo,
            VhdlTransformationContext transformationContext)
        {
            if (node is MethodDeclaration methodDeclaration)
            {
                memberTransformerTasks
                    .Add(_methodTransformer.TransformAsync(methodDeclaration, transformationContext));
            }
            else if (node is FieldDeclaration fieldDeclaration &&
                     _displayClassFieldTransformer.IsDisplayClassField(fieldDeclaration))
            {
                memberTransformerTasks
                    .Add(_displayClassFieldTransformer.TransformAsync(fieldDeclaration, transformationContext));
            }
            else if (!_pocoTransformer.IsSupportedMember(node))
            {
                throw new NotSupportedException($"The member {node} is not supported for transformation.");
            }

            return TransformChildMembers(memberTransformerTasks, traverseTo, transformationContext);
        }

        private IEnumerable<Task<IMemberTransformerResult>> TransformTypeDeclaration(
            AstNode node,
            List<Task<IMemberTransformerResult>> memberTransformerTasks,
            IEnumerable<AstNode> traverseTo,
            VhdlTransformationContext transformationContext)
        {
            var typeDeclaration = node as TypeDeclaration;
            switch (typeDeclaration?.ClassType)
            {
                case ClassType.Class:
                case ClassType.Struct:
                    if (typeDeclaration.BaseTypes.Any(baseType => baseType.GetActualType().Kind != TypeKind.Interface))
                    {
                        throw new NotSupportedException(
                            "Class inheritance is not supported. Affected class: " + node.GetFullName() + ".");
                    }

                    // Records need to be created only for those types that are neither display classes, nor
                    // hardware entry point types or static types
                    if (!typeDeclaration.GetFullName().IsDisplayOrClosureClassName() &&
                        !typeDeclaration.Members.Any(member => member.IsHardwareEntryPointMember()) &&
                        !typeDeclaration.Modifiers.HasFlag(Modifiers.Static))
                    {
                        memberTransformerTasks.Add(_pocoTransformer.TransformAsync(typeDeclaration, transformationContext));
                    }

                    traverseTo = traverseTo.Where(n => n.NodeType is NodeType.Member or NodeType.TypeDeclaration);
                    return TransformChildMembers(memberTransformerTasks, traverseTo, transformationContext);
                case ClassType.Enum:
                    return memberTransformerTasks; // Enums are transformed separately.
                case ClassType.Interface:
                    return memberTransformerTasks; // Interfaces are irrelevant here.
                case ClassType.RecordClass:
                default:
                    return TransformChildMembers(memberTransformerTasks, traverseTo, transformationContext);
            }
        }

        private List<Task<IMemberTransformerResult>> TransformChildMembers(
            List<Task<IMemberTransformerResult>> memberTransformerTasks,
            IEnumerable<AstNode> traverseTo,
            VhdlTransformationContext transformationContext)
        {
            foreach (var target in traverseTo)
            {
                TransformMembers(target, transformationContext, memberTransformerTasks);
            }

            return memberTransformerTasks;
        }

        private static MemberIdTable BuildMemberIdTable(IEnumerable<IMemberTransformerResult> hardwareEntryPointMemberResults)
        {
            var memberIdTable = new MemberIdTable();
            var memberId = 0;

            foreach (var interfaceMemberResult in hardwareEntryPointMemberResults)
            {
                var methodFullName = interfaceMemberResult.Member.GetFullName();
                memberIdTable.SetMapping(methodFullName, memberId);
                foreach (var methodNameAlternate in methodFullName.GetMemberNameAlternates())
                {
                    memberIdTable.SetMapping(methodNameAlternate, memberId);
                }

                memberId++;
            }

            return memberIdTable;
        }

        private static void ReadAndAddEmbedLibrary(
            string libraryName,
            VhdlManifest manifest,
            VhdlBuilder.Representation.Declaration.Module hastIpModule)
        {
            var resourceName = "Hast.Transformer.Vhdl.VhdlLibraries." + libraryName + ".vhd";
#pragma warning disable S3902 // "Assembly.GetExecutingAssembly" should not be called. Except we are in a library.
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
#pragma warning restore S3902 // "Assembly.GetExecutingAssembly" should not be called. Except we are in a library.
            using (var reader = new StreamReader(stream))
            {
                manifest.Modules.Add(new LogicalBlock(new Raw(reader.ReadToEnd())));
            }

            hastIpModule.Libraries.Add(new Library(
                name: "work",
                uses: new List<string> { libraryName + ".all" }));
        }
    }
}
