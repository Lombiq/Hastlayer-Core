using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;
using System.Linq;
using Hast.VhdlBuilder;
using Hast.Transformer.Vhdl.SubTransformers;
using System;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common;
using Hast.Common.Models;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation.Expression;

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
                        Name = "IEEE",
                        Uses = new List<string> { "IEEE.STD_LOGIC_1164.ALL", "IEEE.NUMERIC_STD.ALL" }
                    });

                    module.Architecture.Entity = module.Entity;

                    ReorderProcedures(vhdlTransformationContext);
                    var callIdTable = ProcessInterfaceMethods(vhdlTransformationContext);

                    ProcessUtility.AddClockToProcesses(module, "clk");

                    return new VhdlHardwareDescription(new VhdlManifest { TopModule = module }, callIdTable);
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

            var proxyProcess = new Process { Name = "CallProxy" };
            var ports = transformationContext.Module.Entity.Ports;
            var callIdTable = new MethodIdTable();

            var callIdPort = new Port
            {
                Name = "CallId",
                Mode = PortMode.In,
                DataType = new RangedDataType { Name = "integer", RangeMin = 0, RangeMax = 999999 },
            };

            ports.Add(callIdPort);

            var caseExpression = new Case { Expression = "CallId".ToVhdlId() };

            var id = 1;
            foreach (var method in transformationContext.InterfaceMethods)
            {
                ports.AddRange(method.Ports);

                string source = string.Empty;

                // Copying signals to variables and vice versa
                foreach (var port in method.Ports)
                {
                    var variableName = port.Name + ".var";
                    proxyProcess.Declarations.Add(new Variable
                    {
                        Name = variableName,
                        DataType = port.DataType
                    });

                    if (port.Mode == PortMode.In) source += variableName.ToVhdlId() + " := " + port.Name.ToVhdlId() + ";";
                }

                var when = new When { Expression = id.ToString() };
                source += method.Name.ToVhdlId() + "(";
                source += string.Join(
                    ", ",
                    method.ParameterMappings.Select(mapping =>
                        {
                            // Using named parameters as the order of ports is not necessarily right
                            return mapping.Parameter.Name.ToVhdlId() + " => " + (mapping.Port.Name + ".var").ToVhdlId();
                        }));
                source += ");";

                foreach (var port in method.Ports.Where(p => p.Mode == PortMode.Out))
                {
                    var variableName = port.Name + ".var";
                    source += port.Name.ToVhdlId() + " <=" + variableName.ToVhdlId() + ";";
                }

                when.Body.Add(new Raw(source));
                caseExpression.Whens.Add(when);


                callIdTable.SetMapping(method.Name, id);
                id++;
            }

            caseExpression.Whens.Add(new When { Expression = "others" });

            proxyProcess.Body.Add(caseExpression);

            transformationContext.Module.Architecture.Body.Add(proxyProcess);

            return callIdTable;
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
    }
}
