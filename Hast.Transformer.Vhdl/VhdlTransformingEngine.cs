using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.Common.Extensions;

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
                Module = new Module { Architecture = new Architecture { Name = "Imp" } },
                MemberCallChainTable = new MemberCallChainTable()
            };

            // The top module should have as few and as small inputs as possible. Its name can't be an extended identifier.
            var module = vhdlTransformationContext.Module;
            module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hastlayer" + vhdlTransformationContext.Id.GetHashCode().ToString()) };

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

            ReorderProcedures(vhdlTransformationContext);
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
                Name = CommonPortNames.MemberId,
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt
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
            return new MemberIdTable();
            //if (!transformationContext.InterfaceMethods.Any()) return MemberIdTable.Empty;

            //var proxyProcess = new Process { Label = "CallProxy".ToExtendedVhdlId() };
            //var ports = transformationContext.Module.Entity.Ports;
            //var memberIdTable = new MemberIdTable();

            //var methodIdPort = new Port
            //{
            //    Name = "MethodId".ToExtendedVhdlId(),
            //    Mode = PortMode.In,
            //    DataType = KnownDataTypes.UnrangedInt,
            //};

            //ports.Add(methodIdPort);

            //var caseExpression = new Case { Expression = methodIdPort.Name.ToVhdlIdValue() };

            //var id = 1;
            //foreach (var interfaceMethod in transformationContext.InterfaceMethods)
            //{
            //    ports.AddRange(interfaceMethod.Ports);

            //    var when = new When { Expression = new Value { DataType = KnownDataTypes.Int32, Content = id.ToString() } };

            //    if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
            //    {
            //        // Calling corresponding procedure with SimpleMemory signals passed in.
            //        var simpleMemoryParameterNames = new[] { SimpleMemoryNames.DataInLocal, SimpleMemoryNames.DataOutLocal, SimpleMemoryNames.ReadAddressLocal, SimpleMemoryNames.WriteAddressLocal };
            //        var simpleMemoryParameters = interfaceMethod.Procedure.Parameters
            //            .Where(parameter => simpleMemoryParameterNames.Contains(parameter.Name))
            //            .Select(parameter =>
            //            {
            //                var reference = new DataObjectReference { DataObjectKind = DataObjectKind.Signal };

            //                if (parameter.Name == SimpleMemoryNames.DataInLocal)
            //                {
            //                    reference.Name = SimpleMemoryNames.DataInPort;
            //                }
            //                else if (parameter.Name == SimpleMemoryNames.DataOutLocal)
            //                {
            //                    reference.Name = SimpleMemoryNames.DataOutPort;
            //                }
            //                else if (parameter.Name == SimpleMemoryNames.ReadAddressLocal)
            //                {
            //                    reference.Name = SimpleMemoryNames.ReadAddressPort;
            //                }
            //                else
            //                {
            //                    reference.Name = SimpleMemoryNames.WriteAddressPort;
            //                }

            //                return reference;
            //            });

            //        var invokation = new Invokation
            //        {
            //            Target = interfaceMethod.Procedure.Name.ToVhdlIdValue(),
            //            Parameters = new List<IVhdlElement>(simpleMemoryParameters)
            //        };

            //        when.Add(invokation.Terminate());
            //    }
            //    else
            //    {
            //        // Copying input signals to variables.
            //        var portVariables = new Dictionary<Port, Variable>();
            //        foreach (var port in interfaceMethod.Ports)
            //        {
            //            var variable = new Variable
            //            {
            //                Name = (port.Name.TrimExtendedVhdlIdDelimiters() + ".var").ToExtendedVhdlId(),
            //                DataType = port.DataType
            //            };

            //            proxyProcess.Declarations.Add(variable);

            //            if (port.Mode == PortMode.In)
            //            {
            //                when.Add(new Assignment { AssignTo = variable, Expression = port.Name.ToVhdlIdValue() });
            //            }

            //            portVariables[port] = variable;
            //        }

            //        // Calling corresponding procedure and taking care of its input/output parameters.
            //        var invokation = new Invokation
            //        {
            //            Target = interfaceMethod.Procedure.Name.ToVhdlIdValue(),
            //            // Using named parameters as the order of ports is not necessarily right
            //            Parameters = interfaceMethod.ParameterMappings
            //                .Select(mapping => new NamedInvokationParameter { FormalParameter = mapping.Parameter, ActualParameter = portVariables[mapping.Port] })
            //                .Cast<IVhdlElement>()
            //                .ToList()
            //        };

            //        when.Add(invokation.Terminate());

            //        // Copying output variables to output ports.
            //        foreach (var port in interfaceMethod.Ports.Where(p => p.Mode == PortMode.Out))
            //        {
            //            when.Add(new Assignment { AssignTo = port, Expression = portVariables[port].Name.ToVhdlIdValue() });
            //        }
            //    }


            //    caseExpression.Whens.Add(when);

            //    var methodFullName = interfaceMethod.Method.GetFullName();
            //    memberIdTable.SetMapping(methodFullName, id);
            //    foreach (var methodNameAlternate in methodFullName.GetMemberNameAlternates())
            //    {
            //        memberIdTable.SetMapping(methodNameAlternate, id);
            //    }

            //    id++;
            //}

            //caseExpression.Whens.Add(new When { Expression = new Value { DataType = KnownDataTypes.Identifier, Content = "others" } });

            //proxyProcess.Add(caseExpression);

            //transformationContext.Module.Architecture.Add(proxyProcess);

            //return memberIdTable;
        }

        /// <summary>
        /// In VHDL procedures should be declared before they're used. Because of this we re-order them if necessary.
        /// </summary>
        private static void ReorderProcedures(VhdlTransformationContext transformationContext)
        {
            var chains = transformationContext.MemberCallChainTable.Chains;

            transformationContext.Module.Architecture.Declarations =
                TopologicalSortHelper.Sort(
                transformationContext.Module.Architecture.Declarations,
                declaration =>
                {
                    if (!(declaration is Procedure)) return Enumerable.Empty<IVhdlElement>();

                    var procedure = (Procedure)declaration;

                    if (!chains.ContainsKey(procedure.Name)) return Enumerable.Empty<IVhdlElement>();

                    var targetNames = chains[procedure.Name].Targets.Select(chain => chain.MemberName);
                    return transformationContext.Module.Architecture.Declarations
                        .Where(element => element is Procedure && targetNames.Contains(((Procedure)element).Name));
                });
        }

        private static void AddSimpleMemoryPorts(Module module)
        {
            var ports = module.Entity.Ports;

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.DataInPort,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DataPortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.DataOutPort,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.DataPortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.CellIndexOutPort,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.CellIndexOutPortDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.ReadEnablePort,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.EnablePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.WriteEnablePort,
                Mode = PortMode.Out,
                DataType = SimpleMemoryTypes.EnablePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.ReadsDonePort,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DonePortsDataType
            });

            ports.Add(new Port
            {
                Name = SimpleMemoryNames.WritesDonePort,
                Mode = PortMode.In,
                DataType = SimpleMemoryTypes.DonePortsDataType
            });
        }
    }
}
