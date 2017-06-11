﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Extensions;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.InvocationProxyBuilders;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SimpleMemory;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Services;

namespace Hast.Transformer.Vhdl.Services
{
    public class TransformedVhdlManifestBuilder : ITransformedVhdlManifestBuilder
    {
        private readonly ICompilerGeneratedClassesVerifier _compilerGeneratedClassesVerifier;
        private readonly IClock _clock;
        private readonly IArrayTypesCreator _arrayTypesCreator;
        private readonly IMethodTransformer _methodTransformer;
        private readonly IDisplayClassFieldTransformer _displayClassFieldTransformer;
        private readonly IExternalInvocationProxyBuilder _externalInvocationProxyBuilder;
        private readonly IInternalInvocationProxyBuilder _internalInvocationProxyBuilder;
        private readonly Lazy<ISimpleMemoryComponentBuilder> _simpleMemoryComponentBuilderLazy;
        private readonly IEnumTypesCreator _enumTypesCreator;
        private readonly IArrayParameterLengthSetter _arrayParameterLengthSetter;
        private readonly IPocoTransformer _pocoTransformer;


        public TransformedVhdlManifestBuilder(
            ICompilerGeneratedClassesVerifier compilerGeneratedClassesVerifier,
            IClock clock,
            IArrayTypesCreator arrayTypesCreator,
            IMethodTransformer methodTransformer,
            IDisplayClassFieldTransformer displayClassFieldTransformer,
            IExternalInvocationProxyBuilder externalInvocationProxyBuilder,
            IInternalInvocationProxyBuilder internalInvocationProxyBuilder,
            Lazy<ISimpleMemoryComponentBuilder> simpleMemoryComponentBuilderLazy,
            IEnumTypesCreator enumTypesCreator,
            IArrayParameterLengthSetter arrayParameterLengthSetter,
            IPocoTransformer pocoTransformer)
        {
            _compilerGeneratedClassesVerifier = compilerGeneratedClassesVerifier;
            _clock = clock;
            _arrayTypesCreator = arrayTypesCreator;
            _methodTransformer = methodTransformer;
            _displayClassFieldTransformer = displayClassFieldTransformer;
            _externalInvocationProxyBuilder = externalInvocationProxyBuilder;
            _internalInvocationProxyBuilder = internalInvocationProxyBuilder;
            _simpleMemoryComponentBuilderLazy = simpleMemoryComponentBuilderLazy;
            _enumTypesCreator = enumTypesCreator;
            _arrayParameterLengthSetter = arrayParameterLengthSetter;
            _pocoTransformer = pocoTransformer;
        }


        public async Task<ITransformedVhdlManifest> BuildManifest(ITransformationContext transformationContext)
        {
            var syntaxTree = transformationContext.SyntaxTree;

            _compilerGeneratedClassesVerifier.VerifyCompilerGeneratedClasses(syntaxTree);

            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext);
            var useSimpleMemory = transformationContext.GetTransformerConfiguration().UseSimpleMemory;

            var architecture = new Architecture { Name = "Imp" };
            var module = new Module { Architecture = architecture };


            // Adding libraries
            module.Libraries.Add(new Library
            {
                Name = "ieee",
                Uses = new List<string> { "ieee.std_logic_1164.all", "ieee.numeric_std.all" }
            });
            if (useSimpleMemory)
            {
                module.Libraries.Add(new Library
                {
                    Name = "Hast",
                    Uses = new List<string> { "Hast.SimpleMemory.all" }
                });
            }


            // Creating the Hast_IP entity. Its name can't be an extended identifier.
            var entity = module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            architecture.Entity = entity;

            var hashedIdBytes = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(vhdlTransformationContext.Id));
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < hashedIdBytes.Length; i++)
            {
                stringBuilder.Append(hashedIdBytes[i].ToString("x2"));
            }
            entity.Declarations.Add(new LineComment("Hast_IP ID: " + stringBuilder.ToString()));
            entity.Declarations.Add(new LineComment("Date and time: " + _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC"));
            entity.Declarations.Add(new LineComment("Generated by Hastlayer - hastlayer.com"));

            architecture.Declarations.Add(new BlockComment(GeneratedCodeOverviewComment.Comment));

            var dependentTypesTables = new List<DependentTypesTable>();

            // Adding array types for any arrays created in code
            // This is necessary in a separate step because in VHDL the array types themselves should be created too
            // (like in C# we'd need to first define what an int[] is before being able to create one).
            var arrayDeclarations = _arrayTypesCreator.CreateArrayTypes(syntaxTree);
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
                architecture.Declarations.Add(enumDeclarationsBlock);
            }


            // Preparing arrays passed as method parameters
            _arrayParameterLengthSetter.SetArrayParameterSizes(syntaxTree);


            // Doing transformations
            var transformerResults = await Task.WhenAll(TransformMembers(transformationContext.SyntaxTree, vhdlTransformationContext));
            var potentiallyInvokingArchitectureComponents = transformerResults
                .SelectMany(result =>
                    result.ArchitectureComponentResults
                        .Select(componentResult => componentResult.ArchitectureComponent)
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
            var allDependentTypes = dependentTypesTables.SelectMany(table => table.GetTypes()).ToDictionary(type => type.Name);
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
                architecture.Declarations.Add(dependentTypesDeclarationsBlock);
            }


            // Adding architecture component declarations. These should come after custom inter-dependent type declarations.
            foreach (var architectureComponentResult in architectureComponentResults)
            {
                architecture.Declarations.Add(architectureComponentResult.Declarations);
                architecture.Add(architectureComponentResult.Body);
            }


            // Proxying external invocations
            var interfaceMemberResults = transformerResults.Where(result => result.IsInterfaceMember);
            if (!interfaceMemberResults.Any())
            {
                throw new InvalidOperationException("There aren't any interface members, however at least one interface member is needed to execute anything on hardware.");
            }
            var memberIdTable = BuildMemberIdTable(interfaceMemberResults);
            var externalInvocationProxy = _externalInvocationProxyBuilder.BuildProxy(interfaceMemberResults, memberIdTable);
            potentiallyInvokingArchitectureComponents.Add(externalInvocationProxy);
            architecture.Declarations.Add(externalInvocationProxy.BuildDeclarations());
            architecture.Add(externalInvocationProxy.BuildBody());


            // Proxying internal invocations
            var internaInvocationProxies = _internalInvocationProxyBuilder.BuildProxy(
                potentiallyInvokingArchitectureComponents,
                vhdlTransformationContext);
            foreach (var proxy in internaInvocationProxies)
            {
                architecture.Declarations.Add(proxy.BuildDeclarations());
                architecture.Add(proxy.BuildBody());
            }


            // Proxying SimpleMemory operations
            if (useSimpleMemory)
            {
                _simpleMemoryComponentBuilderLazy.Value.AddSimpleMemoryComponentsToArchitecture(
                    potentiallyInvokingArchitectureComponents,
                    architecture);
            }


            // Adding common ports
            var ports = entity.Ports;
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


            ProcessUtility.AddClockToProcesses(module, CommonPortNames.Clock);


            return new TransformedVhdlManifest
            {
                Manifest = new VhdlManifest { TopModule = module },
                MemberIdTable = memberIdTable
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
                            if (!typeDeclaration.GetFullName().IsDisplayClassName())
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


        private static MemberIdTable BuildMemberIdTable(IEnumerable<IMemberTransformerResult> interfaceMemberResults)
        {
            var memberIdTable = new MemberIdTable();
            var memberId = 0;

            foreach (var interfaceMemberResult in interfaceMemberResults)
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
    }
}
