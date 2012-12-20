using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder;
using System.Linq;

namespace HastTranspiler.Vhdl
{
    public class TranspilingEngine : ITranspilingEngine
    {
        private readonly Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();


        public string Transpile(SyntaxTree syntaxTree)
        {
            var z = new Library().ToString();
            var y = z;
            Traverse(syntaxTree);
            return "";
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
                        var methodDeclaration = node as MethodDeclaration;

                        // Every public class's public method is a circuit interface
                        if (methodDeclaration.Modifiers == Modifiers.Public && methodDeclaration.Parent is TypeDeclaration)
                        {
                            var parent = methodDeclaration.Parent as TypeDeclaration;

                            if (parent.ClassType == ClassType.Class &&  parent.Modifiers == Modifiers.Public)
                            {
                                var ports = new List<Port>();

                                if (methodDeclaration.ReturnType is PrimitiveType)
                                {
                                    var type = methodDeclaration.ReturnType as PrimitiveType;
                                    if (type.KnownTypeCode != ICSharpCode.NRefactory.TypeSystem.KnownTypeCode.Void)
                                    {
                                        var vhdlType = PrimitiveTypeConverter.Convert(type.KnownTypeCode);
                                        ports.Add(new Port { Type = vhdlType, Mode = PortMode.Out, Name = "Output" }); 
                                    }
                                }

                                if (methodDeclaration.Parameters.Count != 0)
                                {
                                    foreach (var parameter in methodDeclaration.Parameters)
                                    {
                                        DataType vhdlType;

                                        if (parameter.Type is PrimitiveType) vhdlType = PrimitiveTypeConverter.Convert(((PrimitiveType)parameter.Type).KnownTypeCode);
                                        else vhdlType = null;

                                        ports.Add(new Port { Type = vhdlType, Mode = PortMode.In, Name = parameter.Name });
                                    }
                                }

                                var fullName = parent.Name + "." + methodDeclaration.Name;
                                _entities[fullName] = new Entity { Name = fullName, Ports = ports.ToArray() };

                                var z = new Document
                                {
                                    Entity = _entities[fullName],
                                    Architecture = new Architecture
                                    {
                                        Entity = _entities[fullName],
                                        Name = "Behavioural"
                                    }
                                }.ToVhdl();

                                var y = z;
                            }
                        }
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
