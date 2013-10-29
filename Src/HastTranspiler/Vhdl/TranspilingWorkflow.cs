using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HastTranspiler.Vhdl.SubTranspilers;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder;
using VhdlBuilder.Representation;
using VhdlBuilder.Representation.Declaration;
using VhdlBuilder.Representation.Expression;

namespace HastTranspiler.Vhdl
{
    public class TranspilingWorkflow
    {
        private readonly TranspilingSettings _settings;
        private readonly string _id;
        private readonly MethodTranspiler _methodTranspiler;
        private TranspilingContext _context;


        public TranspilingWorkflow(TranspilingSettings settings, string id)
            : this(settings, id, new MethodTranspiler())
        {
        }

        public TranspilingWorkflow(TranspilingSettings settings, string id, MethodTranspiler methodTranspiler)
        {
            _settings = settings;
            _id = id;
            _methodTranspiler = methodTranspiler;
        }


        public IHardwareDefinition Transpile(SyntaxTree syntaxTree)
        {
            _context =
                new TranspilingContext(
                    syntaxTree,
                    new Module { Architecture = new Architecture { Name = "Behavioural" } },
                    new CallChainTable());

            var module = _context.Module;
            module.Entity = new Entity { Name = _id };

            Traverse(syntaxTree);

            module.Libraries.Add(new Library
            {
                Name = "IEEE",
                Uses = new List<string> { "IEEE.STD_LOGIC_1164.ALL", "IEEE.NUMERIC_STD.ALL" }
            });

            module.Architecture.Entity = module.Entity;

            ProcessCallChainTable();
            var callIdTable = ProcessInterfaceMethods();

            ProcessUtility.AddClockToProcesses(module, "clk");

            return new VhdlHardwareDefinition(new VhdlManifest { TopModule = module }, callIdTable);
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
                        _methodTranspiler.Transpile(method, _context);
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

        private CallIdTable ProcessInterfaceMethods()
        {
            if (_context.InterfaceMethods.Count == 0) return CallIdTable.Empty;

            var proxyProcess = new Process { Name = "CallProxy" };
            var ports = _context.Module.Entity.Ports;
            var callIdTable = new CallIdTable();

            var callIdPort = new Port
            {
                Name = "CallId",
                Mode = PortMode.In,
                DataType = new RangedDataType { Name = "integer", RangeMin = 0, RangeMax = 999999 },
            };
            //proxyProcess.SesitivityList.Add(callIdPort); // Not needed, will have clock

            ports.Add(callIdPort);

            var caseExpression = new Case { Expression = "CallId".ToVhdlId() };

            var id = 1;
            foreach (var method in _context.InterfaceMethods)
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

            _context.Module.Architecture.Body.Add(proxyProcess);

            return callIdTable;
        }

        private void ProcessCallChainTable()
        {
            var chains = _context.CallChainTable.Values.ToDictionary(chain => chain.ProcedureName);

            var declarations = _context.Module.Architecture.Declarations;
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
