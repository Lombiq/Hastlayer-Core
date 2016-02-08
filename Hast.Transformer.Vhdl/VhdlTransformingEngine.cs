﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.Common.Extensions;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using System;
using Orchard.Services;
using Hast.Transformer.Vhdl.InvokationProxyBuilders;

namespace Hast.Transformer.Vhdl
{
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly IClock _clock;
        private readonly IMethodTransformer _methodTransformer;
        private readonly IExternalInvokationProxyBuilder _externalInvokationProxyBuilder;
        private readonly IInternalInvokationProxyBuilder _internalInvokationProxyBuilder;


        public VhdlTransformingEngine(
            IClock clock,
            IMethodTransformer methodTransformer,
            IExternalInvokationProxyBuilder externalInvokationProxyBuilder,
            IInternalInvokationProxyBuilder internalInvokationProxyBuilder)
        {
            _clock = clock;
            _methodTransformer = methodTransformer;
            _externalInvokationProxyBuilder = externalInvokationProxyBuilder;
            _internalInvokationProxyBuilder = internalInvokationProxyBuilder;
        }


        public async Task<IHardwareDescription> Transform(ITransformationContext transformationContext)
        {
            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext);

            var architecture = new Architecture { Name = "Imp" };
            var module = new Module { Architecture = architecture };


            // Adding libraries
            module.Libraries.Add(new Library
            {
                Name = "ieee",
                Uses = new List<string> { "ieee.std_logic_1164.all", "ieee.numeric_std.all" }
            });
            module.Libraries.Add(new Library
            {
                Name = "Hast",
                Uses = new List<string> { "Hast.SimpleMemory.all" }
            });


            // Creating the Hast_IP entity. Its name can't be an extended identifier.
            var entity = module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            architecture.Entity = entity;
            entity.Declarations.Add(new LineComment("Hast_IP ID: " + vhdlTransformationContext.Id.GetHashCode().ToString()));
            entity.Declarations.Add(new LineComment("Date and time: " + _clock.UtcNow.ToString()));
            entity.Declarations.Add(new LineComment("Generated by Hastlayer - hastlayer.com"));

            architecture.Declarations.Add(new BlockComment(GeneratedCodeOverviewComment.Comment));


            // Doing transformations
            var transformerResults = await Task.WhenAll(Traverse(vhdlTransformationContext.SyntaxTree, vhdlTransformationContext));
            var potentiallyInvokingArchitectureComponents = transformerResults
                .SelectMany(result => result.StateMachineResults.Select(smResult => smResult.StateMachine).Cast<IArchitectureComponent>())
                .ToList();
            foreach (var transformerResult in transformerResults)
            {
                foreach (var stateMachineResult in transformerResult.StateMachineResults)
                {
                    architecture.Declarations.Add(stateMachineResult.Declarations);
                    architecture.Add(stateMachineResult.Body);
                }
            }


            // Proxying external invokations
            var interfaceMemberResults = transformerResults.Where(result => result.IsInterfaceMember);
            if (!interfaceMemberResults.Any())
            {
                throw new InvalidOperationException("There aren't any interface members, however at least one interface member is needed to execute anything on hardware.");
            }
            var memberIdTable = BuildMemberIdTable(interfaceMemberResults);
            var externalInvocationProxy = _externalInvokationProxyBuilder.BuildProxy(interfaceMemberResults, memberIdTable);
            potentiallyInvokingArchitectureComponents.Add(externalInvocationProxy);
            architecture.Declarations.Add(externalInvocationProxy.BuildDeclarations());
            architecture.Add(externalInvocationProxy.BuildBody());


            // Proxying internal invokations
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var internaInvokationProxies = _internalInvokationProxyBuilder.BuildProxy(
                potentiallyInvokingArchitectureComponents,
                vhdlTransformationContext);
            sw.Stop();
            foreach (var proxy in internaInvokationProxies)
            {
                architecture.Declarations.Add(proxy.BuildDeclarations());
                architecture.Add(proxy.BuildBody());
            }


            // Adding common ports
            var ports = entity.Ports;
            if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
            {
                AddSimpleMemoryPorts(ports);
            }
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
                DataType = KnownDataTypes.StdLogic
            });
            ports.Add(new Port
            {
                Name = CommonPortNames.Finished,
                Mode = PortMode.Out,
                DataType = KnownDataTypes.StdLogic
            });


            ProcessUtility.AddClockToProcesses(module, CommonPortNames.Clock);


            return new VhdlHardwareDescription(new VhdlManifest { TopModule = module }, memberIdTable);
        }


        private IEnumerable<Task<IMemberTransformerResult>> Traverse(
            AstNode node, 
            VhdlTransformationContext transformationContext,
            List<Task<IMemberTransformerResult>> methodTransformerTasks = null)
        {
            if (methodTransformerTasks == null)
            {
                methodTransformerTasks = new List<Task<IMemberTransformerResult>>();
            }

            var traverseTo = node.Children;

            switch (node.NodeType)
            {
                case NodeType.Expression:
                    break;
                case NodeType.Member:
                    if (node is MethodDeclaration)
                    {
                        var method = node as MethodDeclaration;
                        methodTransformerTasks.Add(_methodTransformer.Transform(method, transformationContext));
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
                            traverseTo = traverseTo.Skip(traverseTo.Count(n => n.NodeType != NodeType.Member));
                            break;
                        case ClassType.Enum:
                            break;
                        case ClassType.Interface:
                            return methodTransformerTasks; // Interfaces are irrelevant here.
                        case ClassType.Struct:
                            break;
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
                Traverse(target, transformationContext, methodTransformerTasks);
            }

            return methodTransformerTasks;
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

        private static void AddSimpleMemoryPorts(List<Port> ports)
        {
            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.DataIn,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DataPortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.DataOut,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.DataPortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.CellIndexOut,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.CellIndexOutPortDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.ReadEnable,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.EnablePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.WriteEnable,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.EnablePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.ReadsDone,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DonePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.WritesDone,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DonePortsDataType
            });
        }
    }
}
