using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HastTranspiler.Vhdl.SubTranspilers;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl
{
    public class TranspilingWorkflow
    {
        private readonly TranspilingSettings _settings;
        private readonly string _id;
        private TranspilingContext _context;

        public MethodTranspiler MethodTranspiler { get; set; }


        public TranspilingWorkflow(TranspilingSettings settings, string id)
        {
            _settings = settings;
            _id = id;
            _context = new TranspilingContext(new Module { Architecture = new Architecture { Name = "Behavioural" } });

            MethodTranspiler = new MethodTranspiler();
        }


        public IHardwareDefinition Transpile(SyntaxTree syntaxTree)
        {
            var module = _context.Module;
            module.Entity = new Entity { Name = _id };

            Traverse(syntaxTree);

            module.Libraries.Add(new Library
            {
                Name = "IEEE",
                Uses = new List<string> { "IEEE.STD_LOGIC_1164.ALL", "IEEE.NUMERIC_STD.ALL" }
            });

            module.Architecture.Entity = module.Entity;

            var callIdTable = ProcessInterfaceMethods();

            ClockUtility.AddClockSignalToProcesses(module, "clk");

            return new HardwareDefinition(new VhdlManifest { TopModule = module }, callIdTable);
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
                        MethodTranspiler.Transpile(method, _context);
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
                default:
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
                    proxyProcess.Declarations.Add(new DataObject
                    {
                        Type = ObjectType.Variable,
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

                
                callIdTable[method.Name] = id;
                id++;
            }

            caseExpression.Whens.Add(new When { Expression = "others" });

            proxyProcess.Body.Add(caseExpression);

            _context.Module.Architecture.Body.Add(proxyProcess);

            return callIdTable;
        }
    }
}
