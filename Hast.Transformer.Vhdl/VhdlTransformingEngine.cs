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
using Hast.Transformer.Vhdl.StateMachineGeneration;
using System;
using Orchard.Services;

namespace Hast.Transformer.Vhdl
{
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly IMethodTransformer _methodTransformer;
        private readonly IClock _clock;


        public VhdlTransformingEngine(IMethodTransformer methodTransformer, IClock clock)
        {
            _methodTransformer = methodTransformer;
            _clock = clock;
        }


        public async Task<IHardwareDescription> Transform(ITransformationContext transformationContext)
        {
            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext);

            var module = new Module { Architecture = new Architecture { Name = "Imp" } };

            // The top module should have as few and as small inputs as possible. Its name can't be an extended identifier.
            module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            module.Entity.Declarations.Add(new LineComment("Hast_IP ID: " + vhdlTransformationContext.Id.GetHashCode().ToString()));
            module.Entity.Declarations.Add(new LineComment("Date and time: " + _clock.UtcNow.ToString()));
            module.Entity.Declarations.Add(new LineComment("Generated by Hastlayer - hastlayer.com"));

            module.Architecture.Declarations.Add(new BlockComment(GeneratedCodeOverviewComment.Comment));

            var transformerResults = await Task.WhenAll(Traverse(vhdlTransformationContext.SyntaxTree, vhdlTransformationContext));
            foreach (var transformerResult in transformerResults)
            {
                foreach (var stateMachineResult in transformerResult.StateMachineResults)
                {
                    module.Architecture.Declarations.Add(stateMachineResult.Declarations);
                    module.Architecture.Add(stateMachineResult.Body); 
                }
            }

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

            module.Architecture.Entity = module.Entity;

            var memberIdTable = ProcessInterfaceMembers(module, vhdlTransformationContext, transformerResults);

            if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
            {
                AddSimpleMemoryPorts(module);
            }

            ProcessStateMachineStartSignalFunnel(module, vhdlTransformationContext);

            // Adding common ports
            var ports = module.Entity.Ports;
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


        private static MemberIdTable ProcessInterfaceMembers(
            Module module, 
            VhdlTransformationContext transformationContext,
            IMemberTransformerResult[] memberTransformerResults)
        {
            var callProxyProcess = new Process { Label = "CallProxy".ToExtendedVhdlId() };
            var ports = module.Entity.Ports;
            var architecture = module.Architecture;
            var memberIdTable = new MemberIdTable();

            var memberIdPort = new Port
            {
                Name = CommonPortNames.MemberId,
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt
            };

            ports.Add(memberIdPort);

            var callProxySignalDeclarationsBlock = new LogicalBlock(new LineComment("CallProxy signal declarations start"));
            architecture.Declarations.Add(callProxySignalDeclarationsBlock);

            // Since the Finished port is an out port, it can't be read. Adding an internal proxy signal so we can also 
            // read it.
            var finishedSignal = new Signal
            {
                Name = "FinishedInternal".ToExtendedVhdlId(),
                DataType = KnownDataTypes.StdLogic,
                InitialValue = Value.ZeroCharacter
            };
            callProxySignalDeclarationsBlock.Add(finishedSignal);
            var finishedSignalReference = finishedSignal.ToReference();
            architecture.Body.Add(new Assignment
                {
                    AssignTo = CommonPortNames.Finished.ToVhdlSignalReference(),
                    Expression = finishedSignalReference
                });


            var ifInResetBlock = new InlineBlock(
                new LineComment("Synchronous reset"),
                new Assignment { AssignTo = finishedSignalReference, Expression = Value.ZeroCharacter });

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlSignalReference(),
                    Operator = Operator.Equality,
                    Right = Value.OneCharacter
                },
                True = ifInResetBlock
            };

            callProxyProcess.Add(resetIf);


            var memberSelectingCase = new Case { Expression = memberIdPort.Name.ToVhdlIdValue() };

            var interfaceMemberResults = memberTransformerResults.Where(result => result.IsInterfaceMember);

            if (!interfaceMemberResults.Any())
            {
                throw new InvalidOperationException("There aren't any interface members, however at least one interface member is needed to execute anything on hardware.");
            }

            var memberId = 0;

            foreach (var interfaceMemberResult in interfaceMemberResults)
            {
                var when = new When { Expression = new Value { DataType = KnownDataTypes.Int32, Content = memberId.ToString() } };


                var stateMachine = interfaceMemberResult.StateMachineResults.First().StateMachine;

                // So it's not cut off wrongly we need to use a name for this signal as it would look from a generated
                // state machine.
                var startSignalName = ("System.Void Hast::CallProxy " + stateMachine.CreateStartSignalName().TrimExtendedVhdlIdDelimiters())
                    .ToExtendedVhdlId();
                var startSignal = new Signal
                {
                    DataType = KnownDataTypes.Boolean,
                    Name = startSignalName,
                    InitialValue = Value.False
                };
                callProxySignalDeclarationsBlock.Add(startSignal);
                var startSignalReference = startSignal.ToReference();

                transformationContext.MemberStateMachineStartSignalFunnel
                    .AddDrivingStartSignalForStateMachine(startSignalName, stateMachine.Name);

                var stateMachineIsFinishedIfElse = new IfElse
                {
                    Condition = new Binary
                    {
                        Left = stateMachine.CreateFinishedSignalName().ToVhdlSignalReference(),
                        Operator = Operator.Equality,
                        Right = Value.True
                    },
                    True = new InlineBlock(
                        new Assignment
                        {
                            AssignTo = startSignalReference,
                            Expression = Value.False
                        },
                        new Assignment
                        {
                            AssignTo = finishedSignalReference,
                            Expression = Value.OneCharacter
                        }),
                    Else = new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = stateMachine.CreateStartSignalName().ToVhdlSignalReference(),
                            Operator = Operator.Equality,
                            Right = Value.False
                        },
                        True = new Assignment
                        {
                            AssignTo = startSignalReference,
                            Expression = Value.True
                        }
                    }
                };

                when.Add(stateMachineIsFinishedIfElse);


                memberSelectingCase.Whens.Add(when);

                var methodFullName = interfaceMemberResult.Member.GetFullName();
                memberIdTable.SetMapping(methodFullName, memberId);
                foreach (var methodNameAlternate in methodFullName.GetMemberNameAlternates())
                {
                    memberIdTable.SetMapping(methodNameAlternate, memberId);
                }

                memberId++;
            }

            memberSelectingCase.Whens.Add(new When { Expression = new Value { DataType = KnownDataTypes.Identifier, Content = "others" } });

            var startedPortReference = CommonPortNames.Started.ToVhdlSignalReference();
            var startedIfElse = new IfElse
            {
                Condition = new Binary
                {
                    Left = new Binary
                    {
                        Left = startedPortReference,
                        Operator = Operator.Equality,
                        Right = Value.OneCharacter
                    },
                    Operator = Operator.ConditionalAnd,
                    Right = new Binary
                    {
                        Left = finishedSignalReference,
                        Operator = Operator.Equality,
                        Right = Value.ZeroCharacter
                    }

                },
                True = new InlineBlock(
                    new LineComment("Starting the state machine corresponding to the given member ID."),
                    memberSelectingCase),
                Else = new InlineBlock(
                    new LineComment("Waiting for Started to be pulled back to zero that signals the framework noting the finish."),
                    new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = new Binary
                            {
                                Left = startedPortReference,
                                Operator = Operator.Equality,
                                Right = Value.ZeroCharacter
                            },
                            Operator = Operator.ConditionalAnd,
                            Right = new Binary
                            {
                                Left = finishedSignalReference,
                                Operator = Operator.Equality,
                                Right = Value.OneCharacter
                            }

                        },
                        True = new Assignment
                        {
                            AssignTo = finishedSignalReference,
                            Expression = Value.ZeroCharacter
                        }
                    })
            };

            resetIf.Else = startedIfElse;

            architecture.Add(callProxyProcess);

            callProxySignalDeclarationsBlock.Add(new LineComment("CallProxy signal declarations end"));

            return memberIdTable;
        }

        private static void AddSimpleMemoryPorts(Module module)
        {
            var ports = module.Entity.Ports;

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

        private static void ProcessStateMachineStartSignalFunnel(Module module, VhdlTransformationContext transformationContext)
        {
            var drivingStartSignalsForStateMachines = transformationContext.MemberStateMachineStartSignalFunnel
                .GetDrivingStartSignalsForStateMachines();

            var signalsAssignmentBlock = new LogicalBlock(new LineComment("Driving state machine start signals start"));

            foreach (var stateMachineToSignalsMapping in drivingStartSignalsForStateMachines)
            {

                if (!stateMachineToSignalsMapping.Value.Any())
                {
                    throw new InvalidOperationException("There weren't any driving start signals specified for the state machine " + stateMachineToSignalsMapping.Key + ".");
                }

                IVhdlElement assignmentExpression = stateMachineToSignalsMapping.Value.First().ToVhdlSignalReference();

                // Iteratively build a binary expression chain to OR together all the driving signals.
                if (stateMachineToSignalsMapping.Value.Count() > 1)
                {
                    var currentBinary = new Binary
                    {
                        Left = stateMachineToSignalsMapping.Value.Skip(1).First().ToVhdlSignalReference(),
                        Operator = Operator.ConditionalOr
                    };
                    var firstBinary = currentBinary;

                    foreach (var drivingStartSignal in stateMachineToSignalsMapping.Value.Skip(2))
                    {
                        var newBinary = new Binary
                        {
                            Left = drivingStartSignal.ToVhdlSignalReference(),
                            Operator = Operator.ConditionalOr
                        };

                        currentBinary.Right = newBinary;
                        currentBinary = newBinary;
                    }

                    currentBinary.Right = assignmentExpression;
                    assignmentExpression = firstBinary;
                }

                signalsAssignmentBlock.Add(
                    new Assignment
                    {
                        AssignTo = MemberStateMachineNameFactory
                            .CreateStartSignalName(stateMachineToSignalsMapping.Key)
                            .ToVhdlSignalReference(),
                        Expression = assignmentExpression
                    });
            }

            signalsAssignmentBlock.Add(new LineComment("Driving state machine start signals end"));
            module.Architecture.Add(signalsAssignmentBlock);
        }
    }
}
