using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;
using System.Linq;
using VhdlBuilder;

namespace HastTranspiler.Vhdl
{
    public class TranspilingEngine : ITranspilingEngine
    {
        private readonly Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();
        private readonly Module _module = new Module { Architecture = new Architecture { Name = "Behavioural" } };


        public string Transpile(SyntaxTree syntaxTree)
        {
            Traverse(syntaxTree);

            _module.Libraries.Add(new Library
            {
                Name = "IEEE",
                Uses = new List<string> { "IEEE.STD_LOGIC_1164.ALL", "IEEE.STD_LOGIC_ARITH.ALL", "IEEE.STD_LOGIC_UNSIGNED.ALL" }
            });

            _module.Architecture.Entity = _module.Entity;

            ClockUtility.AddClockSignal(_module, "clk");

            return _module.ToVhdl();
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

                        var entity = MethodTranspiler.CreateEntityIfPublic(method);

                        if (entity != null) _module.Entity = entity;

                        _module.Architecture.Body.Add(MethodTranspiler.Transpile(method));
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
    }
}
