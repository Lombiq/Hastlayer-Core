﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Extensions;
using Hast.Common.Helpers;
using Hast.Layer;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
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
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Services;

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
            IPocoTransformer pocoTransformer)
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
        }


        public async Task<ITransformedVhdlManifest> BuildManifest(ITransformationContext transformationContext)
        {
            var syntaxTree = transformationContext.SyntaxTree;

            _compilerGeneratedClassesVerifier.VerifyCompilerGeneratedClasses(syntaxTree);
            _hardwareEntryPointsVerifier.VerifyHardwareEntryPoints(syntaxTree, transformationContext.TypeDeclarationLookupTable);

            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext);
            var useSimpleMemory = transformationContext.GetTransformerConfiguration().UseSimpleMemory;

            var hastIpArchitecture = new Architecture { Name = "Imp" };
            var hastIpModule = new VhdlBuilder.Representation.Declaration.Module { Architecture = hastIpArchitecture };


            // Adding libraries
            hastIpModule.Libraries.Add(new Library
            {
                Name = "ieee",
                Uses = new List<string> { "std_logic_1164.all", "numeric_std.all" }
            });
            if (useSimpleMemory)
            {
                hastIpModule.Libraries.Add(new Library
                {
                    Name = "Hast",
                    Uses = new List<string> { "SimpleMemory.all" }
                });
            }


            // Creating the Hast_IP entity. Its name can't be an extended identifier.
            var hastIpEntity = hastIpModule.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            hastIpArchitecture.Entity = hastIpEntity;


            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Hast_IP ID: " + transformationContext.Id));
            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Date and time: " + _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC"));
            hastIpEntity.Declarations.Add(new UnOmittableLineComment("Generated by Hastlayer - hastlayer.com"));

            hastIpArchitecture.Declarations.Add(new BlockComment(GeneratedCodeOverviewComment.Comment));

            var dependentTypesTables = new List<DependentTypesTable>();

            // Adding array types for any arrays created in code
            // This is necessary in a separate step because in VHDL the array types themselves should be created too
            // (like in C# we'd need to first define what an int[] is before being able to create one).
            var arrayDeclarations = _arrayTypesCreator.CreateArrayTypes(syntaxTree, vhdlTransformationContext);
            if (arrayDeclarations.Any())
            {
                var arrayTypeDependentTypes = new DependentTypesTable();
                foreach (var arrayDeclaration in arrayDeclarations)
                {
                    arrayTypeDependentTypes.AddDependency(arrayDeclaration, arrayDeclaration.ElementType.Name);
                }
                dependentTypesTables.Add(arrayTypeDependentTypes);
            }


            // Adding enum types
            var enumDeclarations = _enumTypesCreator.CreateEnumTypes(syntaxTree);
            if (enumDeclarations.Any())
            {
                var enumDeclarationsBlock = new LogicalBlock(new LineComment("Enum declarations start"));
                enumDeclarationsBlock.Body.AddRange(enumDeclarations);
                enumDeclarationsBlock.Add(new LineComment("Enum declarations end"));
                hastIpArchitecture.Declarations.Add(enumDeclarationsBlock);
            }


            // Doing transformations
            var transformerResults = await Task.WhenAll(TransformMembers(transformationContext.SyntaxTree, vhdlTransformationContext));
            var warnings = new List<ITransformationWarning>();
            var potentiallyInvokingArchitectureComponents = transformerResults
                .SelectMany(result =>
                    result.ArchitectureComponentResults
                        .Select(componentResult =>
                        {
                            warnings.AddRange(componentResult.Warnings);
                            return componentResult.ArchitectureComponent;
                        })
                        .Cast<IArchitectureComponent>())
                .ToList();
            var architectureComponentResults = transformerResults.SelectMany(transformerResult => transformerResult.ArchitectureComponentResults);
            foreach (var architectureComponentResult in architectureComponentResults)
            {
                if (architectureComponentResult.ArchitectureComponent.DependentTypesTable.GetTypes().Any())
                {
                    dependentTypesTables.Add(architectureComponentResult.ArchitectureComponent.DependentTypesTable);
                }
            }


            // Processing inter-dependent types. In VHDL if a type depends another type (e.g. an array stores elements
            // of a record type) than the type depending on the other one should come after the other one in the code
            // file.
            var allDependentTypes = dependentTypesTables
                .SelectMany(table => table.GetTypes())
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


            // Proxying external invocations
            var hardwareEntryPointMemberResults = transformerResults.Where(result => result.IsHardwareEntryPointMember);
            if (!hardwareEntryPointMemberResults.Any())
            {
                throw new InvalidOperationException(
                    "There aren't any hardware entry point members, however at least one is needed to execute anything on hardware.");
            }
            var memberIdTable = BuildMemberIdTable(hardwareEntryPointMemberResults);
            var externalInvocationProxy = _externalInvocationProxyBuilder.BuildProxy(hardwareEntryPointMemberResults, memberIdTable);
            potentiallyInvokingArchitectureComponents.Add(externalInvocationProxy);
            hastIpArchitecture.Declarations.Add(externalInvocationProxy.BuildDeclarations());
            hastIpArchitecture.Add(externalInvocationProxy.BuildBody());


            // Proxying internal invocations
            var internaInvocationProxies = _internalInvocationProxyBuilder.BuildProxy(
                potentiallyInvokingArchitectureComponents,
                vhdlTransformationContext);
            foreach (var proxy in internaInvocationProxies)
            {
                hastIpArchitecture.Declarations.Add(proxy.BuildDeclarations());
                hastIpArchitecture.Add(proxy.BuildBody());
            }


            // Proxying SimpleMemory operations
            if (useSimpleMemory)
            {
                _simpleMemoryComponentBuilderLazy.Value.AddSimpleMemoryComponentsToArchitecture(
                    potentiallyInvokingArchitectureComponents,
                    hastIpArchitecture);
            }


            // Adding common ports
            var ports = hastIpEntity.Ports;
            ports.Add(new Port
            {
                Name = CommonPortNames.MemberId,
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Reset,
                Mode = PortMode.In,
                DataType = KnownDataTypes.StdLogic
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Started,
                Mode = PortMode.In,
                DataType = KnownDataTypes.Boolean
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Finished,
                Mode = PortMode.Out,
                DataType = KnownDataTypes.Boolean
            });


            ProcessUtility.AddClockToProcesses(hastIpModule, CommonPortNames.Clock);


            var manifest = new VhdlManifest();

            ReadAndAddEmbeddedLibrary("TypeConversion", manifest, hastIpModule);

            manifest.Modules.Add(hastIpModule);

            return new TransformedVhdlManifest
            {
                Manifest = manifest,
                MemberIdTable = memberIdTable,
                Warnings = warnings
            };
        }


        private IEnumerable<Task<IMemberTransformerResult>> TransformMembers(
            AstNode node,
            VhdlTransformationContext transformationContext,
            List<Task<IMemberTransformerResult>> memberTransformerTasks = null)
        {
            if (memberTransformerTasks == null)
            {
                memberTransformerTasks = new List<Task<IMemberTransformerResult>>();
            }

            var traverseTo = node.Children;

            switch (node.NodeType)
            {
                case NodeType.Expression:
                    break;
                case NodeType.Member:
                    if (node is MethodDeclaration)
                    {
                        memberTransformerTasks.Add(_methodTransformer.Transform((MethodDeclaration)node, transformationContext));
                    }
                    else if (node is FieldDeclaration && _displayClassFieldTransformer.IsDisplayClassField((FieldDeclaration)node))
                    {
                        memberTransformerTasks.Add(_displayClassFieldTransformer.Transform((FieldDeclaration)node, transformationContext));
                    }
                    else if (!_pocoTransformer.IsSupportedMember(node))
                    {
                        throw new NotSupportedException("The member " + node.ToString() + " is not supported for transformation.");
                    }
                    break;
                case NodeType.Pattern:
                    break;
                case NodeType.QueryClause:
                    break;
                case NodeType.Statement:
                    break;
                case NodeType.Token:
                    if (node is CSharpModifierToken)
                    {
                        var modifier = node as CSharpModifierToken;
                    }
                    break;
                case NodeType.TypeDeclaration:
                    var typeDeclaration = node as TypeDeclaration;
                    switch (typeDeclaration.ClassType)
                    {
                        case ClassType.Class:
                        case ClassType.Struct:
                            if (typeDeclaration.BaseTypes.Any(baseType => !baseType.Annotation<TypeDefinition>().IsInterface))
                            {
                                throw new NotSupportedException(
                                    "Class inheritance is not supported. Affected class: " + node.GetFullName() + ".");
                            }

                            // Records need to be created only for those types that are neither display classes, nor
                            // hardware entry point types or static types 
                            if (!typeDeclaration.GetFullName().IsDisplayClassName() && 
                                !typeDeclaration.Members.Any(member => member.IsHardwareEntryPointMember()) &&
                                !typeDeclaration.Modifiers.HasFlag(Modifiers.Static))
                            {
                                memberTransformerTasks.Add(_pocoTransformer.Transform(typeDeclaration, transformationContext)); 
                            }
                            traverseTo = traverseTo.Where(n =>
                                n.NodeType == NodeType.Member || n.NodeType == NodeType.TypeDeclaration);
                            break;
                        case ClassType.Enum:
                            return memberTransformerTasks; // Enums are transformed separately.
                        case ClassType.Interface:
                            return memberTransformerTasks; // Interfaces are irrelevant here.
                    }
                    break;
                case NodeType.TypeReference:
                    break;
                case NodeType.Unknown:
                    break;
                case NodeType.Whitespace:
                    break;
            }

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

        private static void ReadAndAddEmbeddedLibrary(
            string libraryName,
            VhdlManifest manifest,
            VhdlBuilder.Representation.Declaration.Module hastIpModule)
        {
            var resourceName = "Hast.Transformer.Vhdl.VhdlLibraries." + libraryName + ".vhd";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                manifest.Modules.Add(new LogicalBlock(new Raw(reader.ReadToEnd())));
            }

            hastIpModule.Libraries.Add(new Library
            {
                Name = "work",
                Uses = new List<string> { libraryName + ".all" }
            });
        }
    }
}
