﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.SubTransformers;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.Common.Configuration;
using Hast.Common;

namespace Hast.Transformer.Vhdl
{
    public class TransformingWorkflow
    {
        private readonly MethodTransformer _methodTransformer;
        private readonly VhdlTransformationContext _transformationContext;


        public TransformingWorkflow(ITransformationContext transformationContext)
            : this(transformationContext, new MethodTransformer())
        {
        }

        public TransformingWorkflow(ITransformationContext transformationContext, MethodTransformer methodTransformer)
        {
            _methodTransformer = methodTransformer;

            _transformationContext = new VhdlTransformationContext(transformationContext)
                {
                    Module = new Module { Architecture = new Architecture { Name = "Behavioural" } },
                    MethodCallChainTable = new MethodCallChainTable()
                };
        }


        public Task<IHardwareDescription> Transform()
        {
            return Task.Run<IHardwareDescription>(() =>
                {
                    // The top module should have as few and as small inputs as possible. Its name can't be an extended identifier.
                    var module = _transformationContext.Module;
                    module.Entity = new Entity { Name = Entity.ToSafeEntityName(_transformationContext.Id) };

                    Traverse(_transformationContext.SyntaxTree);

                    module.Libraries.Add(new Library
                    {
                        Name = "IEEE",
                        Uses = new List<string> { "IEEE.STD_LOGIC_1164.ALL", "IEEE.NUMERIC_STD.ALL" }
                    });

                    module.Architecture.Entity = module.Entity;

                    ReorderProcedures();
                    var callIdTable = ProcessInterfaceMethods();

                    ProcessUtility.AddClockToProcesses(module, "clk");

                    return new VhdlHardwareDescription(new VhdlManifest { TopModule = module }, callIdTable);
                });
        }


        private void Traverse(AstNode node)
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
                        _methodTransformer.Transform(method, _transformationContext);
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
                Traverse(target);
            }
        }

        private MethodIdTable ProcessInterfaceMethods()
        {
            if (!_transformationContext.InterfaceMethods.Any()) return MethodIdTable.Empty;

            var proxyProcess = new Process { Name = "CallProxy" };
            var ports = _transformationContext.Module.Entity.Ports;
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
            foreach (var method in _transformationContext.InterfaceMethods)
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

            _transformationContext.Module.Architecture.Body.Add(proxyProcess);

            return callIdTable;
        }

        /// <summary>
        /// In VHDL procedures should be declared before they're used. Because of this we re-order them if necessary.
        /// </summary>
        private void ReorderProcedures()
        {
            var chains = _transformationContext.MethodCallChainTable.Values.ToDictionary(chain => chain.ProcedureName);

            var declarations = _transformationContext.Module.Architecture.Declarations;
            for (int i = 0; i < declarations.Count; i++)
            {
                if (!(declarations[i] is Procedure)) continue;

                var procedure = declarations[i] as Procedure;

                if (!chains.ContainsKey(procedure.Name)) continue;

                var targets = chains[procedure.Name].Targets.ToDictionary(chain => chain.ProcedureName);

                for (int x = i + 1; x < declarations.Count; x++)
                {
                    if (!(declarations[x] is Procedure)) continue;

                    var otherProcedure = declarations[x] as Procedure;
                    if (targets.ContainsKey(otherProcedure.Name)) declarations.MoveBefore(x, i);
                }
            }
        }
    }


    static class ListExtensions
    {
        public static void MoveBefore<T>(this List<T> list, int itemIndex, int beforeIndex)
        {
            var item = list[itemIndex];
            list.RemoveAt(itemIndex);
            list.Insert(beforeIndex, item);
        }
    }
}
