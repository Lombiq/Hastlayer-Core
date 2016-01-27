using System.Collections.Generic;
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

namespace Hast.Transformer.Vhdl
{
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly IMethodTransformer _methodTransformer;


        public VhdlTransformingEngine(IMethodTransformer methodTransformer)
        {
            _methodTransformer = methodTransformer;
        }


        public async Task<IHardwareDescription> Transform(ITransformationContext transformationContext)
        {
            var vhdlTransformationContext = new VhdlTransformationContext(transformationContext)
            {
                Module = new Module { Architecture = new Architecture { Name = "Imp" } }
            };

            // The top module should have as few and as small inputs as possible. Its name can't be an extended identifier.
            var module = vhdlTransformationContext.Module;
            module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hast_IP") };
            module.Entity.Declarations.Add(new LineComment("IP ID: " + vhdlTransformationContext.Id.GetHashCode().ToString()));

            await Traverse(vhdlTransformationContext.SyntaxTree, vhdlTransformationContext);

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

            var memberIdTable = ProcessInterfaceMethods(vhdlTransformationContext);

            if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
            {
                AddSimpleMemoryPorts(module);
            }

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


        private async Task Traverse(AstNode node, VhdlTransformationContext transformationContext)
        {
            var traverseTo = node.Children;

            switch (node.NodeType)
            {
                case NodeType.Expression:
                    break;
                case NodeType.Member:
                    if (node is MethodDeclaration)
                    {
                        var method = node as MethodDeclaration;
                        await _methodTransformer.Transform(method, transformationContext);
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
                            return; // Interfaces are irrelevant here.
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
                await Traverse(target, transformationContext);
            }
        }


        private static MemberIdTable ProcessInterfaceMethods(VhdlTransformationContext transformationContext)
        {
            if (!transformationContext.InterfaceMethods.Any()) return MemberIdTable.Empty;

            var callProxyProcess = new Process { Label = "CallProxy".ToExtendedVhdlId() };
            var ports = transformationContext.Module.Entity.Ports;
            var memberIdTable = new MemberIdTable();

            var memberIdPort = new Port
            {
                Name = CommonPortNames.MemberId,
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt
            };

            ports.Add(memberIdPort);

            // Since the Finished port is an out port, it can't be read. Adding an internal proxy signal so we can also 
            // read it.
            var finishedSignal = new Signal
            {
                Name = "FinishedInternal".ToExtendedVhdlId(),
                DataType = KnownDataTypes.StdLogic,
                InitialValue = Value.ZeroCharacter
            };
            transformationContext.Module.Architecture.Declarations.Add(finishedSignal);
            var finishedSignalReference = finishedSignal.ToReference();
            transformationContext.Module.Architecture.Body.Add(new Assignment
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

            var memberId = 0;
            foreach (var interfaceMethod in transformationContext.InterfaceMethods)
            {
                var when = new When { Expression = new Value { DataType = KnownDataTypes.Int32, Content = memberId.ToString() } };


                var stateMachine = interfaceMethod.StateMachine;
                var startVariableReference = stateMachine.CreateStartVariableName().ToVhdlVariableReference();

                var stateMachineIsFinishedIfElse = new IfElse
                {
                    Condition = new Binary
                    {
                        Left = stateMachine.CreateFinishedVariableName().ToVhdlVariableReference(),
                        Operator = Operator.Equality,
                        Right = Value.True
                    },
                    True = new Assignment
                    {
                        AssignTo = finishedSignalReference,
                        Expression = Value.OneCharacter
                    },
                    Else = new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = stateMachine.CreateStartVariableName().ToVhdlVariableReference(),
                            Operator = Operator.Equality,
                            Right = Value.False
                        },
                        True = new Assignment
                        {
                            AssignTo = startVariableReference,
                            Expression = Value.True
                        }
                    }
                };

                when.Add(stateMachineIsFinishedIfElse);


                memberSelectingCase.Whens.Add(when);

                var methodFullName = interfaceMethod.Method.GetFullName();
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
                True = memberSelectingCase,
                Else = new IfElse
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
                }
            };

            resetIf.Else = startedIfElse;

            transformationContext.Module.Architecture.Add(callProxyProcess);

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
    }
}
