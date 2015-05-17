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

namespace Hast.Transformer.Vhdl
{
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly IMethodTransformer _methodTransformer;


        public VhdlTransformingEngine(IMethodTransformer methodTransformer)
        {
            _methodTransformer = methodTransformer;
        }


        public Task<IHardwareDescription> Transform(ITransformationContext transformationContext)
        {
            return Task.Run<IHardwareDescription>(() =>
                {
                    var vhdlTransformationContext = new VhdlTransformationContext(transformationContext)
                    {
                        Module = new Module { Architecture = new Architecture { Name = "Imp" } },
                        MethodCallChainTable = new MethodCallChainTable()
                    };

                    // The top module should have as few and as small inputs as possible. Its name can't be an extended identifier.
                    var module = vhdlTransformationContext.Module;
                    module.Entity = new Entity { Name = Entity.ToSafeEntityName("Hastlayer" + vhdlTransformationContext.Id.GetHashCode().ToString()) };

                    Traverse(vhdlTransformationContext.SyntaxTree, vhdlTransformationContext);

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
                    var methodIdTable = ProcessInterfaceMethods(vhdlTransformationContext);

                    if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
                    {
                        AddSimpleMemoryPorts(module);
                    }

                    ProcessUtility.AddClockToProcesses(module, "Clock".ToExtendedVhdlId());

                    return new VhdlHardwareDescription(new VhdlManifest { TopModule = module }, methodIdTable);
                });
        }


        private void Traverse(AstNode node, VhdlTransformationContext transformationContext)
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
                        _methodTransformer.Transform(method, transformationContext);
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
                Traverse(target, transformationContext);
            }
        }


        private static MethodIdTable ProcessInterfaceMethods(VhdlTransformationContext transformationContext)
        {
            if (!transformationContext.InterfaceMethods.Any()) return MethodIdTable.Empty;

            var proxyProcess = new Process { Label = "CallProxy".ToExtendedVhdlId() };
            var ports = transformationContext.Module.Entity.Ports;
            var methodIdTable = new MethodIdTable();

            var methodIdPort = new Port
            {
                Name = "MethodId".ToExtendedVhdlId(),
                Mode = PortMode.In,
                DataType = KnownDataTypes.UnrangedInt,
            };

            ports.Add(methodIdPort);

            var caseExpression = new Case { Expression = methodIdPort.Name.ToVhdlIdValue() };

            var id = 1;
            foreach (var method in transformationContext.InterfaceMethods)
            {
                ports.AddRange(method.Ports);

                var when = new When { Expression = new Value { DataType = KnownDataTypes.Int32, Content = id.ToString() } };

                if (transformationContext.GetTransformerConfiguration().UseSimpleMemory)
                {
                    // Calling corresponding procedure.
                    var invokation = new Invokation
                    {
                        Target = method.Procedure.Name.ToVhdlIdValue()
                    };

                    when.Body.Add(new Terminated(invokation));
                }
                else
                {
                    // Copying input signals to variables.
                    var portVariables = new Dictionary<Port, Variable>();
                    foreach (var port in method.Ports)
                    {
                        var variable = new Variable
                        {
                            Name = (port.Name.TrimExtendedVhdlIdDelimiters() + ".var").ToExtendedVhdlId(),
                            DataType = port.DataType
                        };

                        proxyProcess.Declarations.Add(variable);

                        if (port.Mode == PortMode.In)
                        {
                            when.Body.Add(new Terminated(new Assignment { AssignTo = variable, Expression = port.Name.ToVhdlIdValue() }));
                        }

                        portVariables[port] = variable;
                    }

                    // Calling corresponding procedure and taking care of its input/output parameters.
                    var invokation = new Invokation
                    {
                        Target = method.Procedure.Name.ToVhdlIdValue(),
                        // Using named parameters as the order of ports is not necessarily right
                        Parameters = method.ParameterMappings
                            .Select(mapping => new NamedInvokationParameter { FormalParameter = mapping.Parameter, ActualParameter = portVariables[mapping.Port] })
                            .Cast<IVhdlElement>()
                            .ToList()
                    };

                    when.Body.Add(new Terminated(invokation));

                    // Copying output variables to output ports.
                    foreach (var port in method.Ports.Where(p => p.Mode == PortMode.Out))
                    {
                        when.Body.Add(new Terminated(new Assignment { AssignTo = port, Expression = portVariables[port].Name.ToVhdlIdValue() }));
                    }
                }


                caseExpression.Whens.Add(when);
                methodIdTable.SetMapping(method.Name, id);
                id++;
            }

            caseExpression.Whens.Add(new When { Expression = new Value { DataType = KnownDataTypes.Identifier, Content = "others" } });

            proxyProcess.Body.Add(caseExpression);

            transformationContext.Module.Architecture.Body.Add(proxyProcess);

            return methodIdTable;
        }

        /// <summary>
        /// In VHDL procedures should be declared before they're used. Because of this we re-order them if necessary.
        /// </summary>
        private static void ReorderProcedures(VhdlTransformationContext transformationContext)
        {
            var chains = transformationContext.MethodCallChainTable.Chains;

            transformationContext.Module.Architecture.Declarations =
                TopologicalSortHelper.Sort(
                transformationContext.Module.Architecture.Declarations,
                declaration =>
                {
                    if (!(declaration is Procedure)) return Enumerable.Empty<IVhdlElement>();

                    var procedure = (Procedure)declaration;

                    if (!chains.ContainsKey(procedure.Name)) return Enumerable.Empty<IVhdlElement>();

                    var targetNames = chains[procedure.Name].Targets.Select(chain => chain.ProcedureName);
                    return transformationContext.Module.Architecture.Declarations
                        .Where(element => element is Procedure && targetNames.Contains(((Procedure)element).Name));
                });
        }

        private static void AddSimpleMemoryPorts(Module module)
        {
            var dataWidthVector = new StdLogicVector { Size = 32 };
            var addressWidthVector = dataWidthVector;

            module.Entity.Ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.DataIn.ToExtendedVhdlId(),
                Mode = PortMode.In,
                DataType = dataWidthVector
            });

            module.Entity.Ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.DataOut.ToExtendedVhdlId(),
                Mode = PortMode.Out,
                DataType = dataWidthVector
            });

            module.Entity.Ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.ReadAddress.ToExtendedVhdlId(),
                Mode = PortMode.Out,
                DataType = addressWidthVector
            });

            module.Entity.Ports.Add(new Port
            {
                Name = SimpleMemoryPortNames.WriteAddress.ToExtendedVhdlId(),
                Mode = PortMode.Out,
                DataType = addressWidthVector
            });
        }
    }
}
