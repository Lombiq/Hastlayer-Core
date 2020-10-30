using Hast.Layer;
using Hast.Synthesis.Abstractions;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class ReadonlyToConstConverter : IReadonlyToConstConverter
    {
        public void ConvertReadonlyPrimitives(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration) =>
            syntaxTree.AcceptVisitor(new ReadonlyToConstVisitor(configuration, syntaxTree));


        private class ReadonlyToConstVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, object> _replacements;
            private readonly SyntaxTree _syntaxTree;

            public ReadonlyToConstVisitor(IHardwareGenerationConfiguration configuration, SyntaxTree syntaxTree)
            {
                _replacements = configuration.GotOrAddReplacements();
                _syntaxTree = syntaxTree;
            }

            public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
                base.VisitFieldDeclaration(fieldDeclaration);

                if (!fieldDeclaration.HasModifier(Modifiers.Static) ||
                    !fieldDeclaration.HasModifier(Modifiers.Readonly))
                {
                    return;
                }

                // We only work with field declarations that are also assignments and the value is of a primitive type,
                // like int or string.
                var initializer = fieldDeclaration
                    .Children
                    .OfType<VariableInitializer>()
                    .FirstOrDefault();
                var value = initializer?.Descendants.OfType<PrimitiveExpression>().FirstOrDefault();
                if (initializer == null || value == null) return;

                var replaceable = fieldDeclaration
                    .Attributes
                    .SelectMany(x => x.Descendants.OfType<Attribute>())
                    .SingleOrDefault(x => x.Type.ToString() == ReplaceableAttribute.Name);

                if (replaceable == null) return;

                var key = replaceable.Descendants.OfType<PrimitiveExpression>().Single().Value.ToString();
                if (_replacements.TryGetValue(key, out var result))
                {
                    if (result is string resultString)
                    {
                        result = value.Value switch
                        {
                            string _ => resultString,
                            int _ => int.Parse(resultString, CultureInfo.InvariantCulture),
                            uint _ => uint.Parse(resultString, CultureInfo.InvariantCulture),
                            long _ => long.Parse(resultString, CultureInfo.InvariantCulture),
                            ulong _ => ulong.Parse(resultString, CultureInfo.InvariantCulture),
                            short _ => short.Parse(resultString, CultureInfo.InvariantCulture),
                            ushort _ => ushort.Parse(resultString, CultureInfo.InvariantCulture),
                            byte _ => byte.Parse(resultString, CultureInfo.InvariantCulture),
                            bool _ => resultString.ToLowerInvariant() == "true",
                            _ => value.Value,
                        };
                    }

                    value = new PrimitiveExpression(result);
                }

                _syntaxTree.AcceptVisitor(new ReplaceReadonlyVisitor(initializer.Name, value.Value));
            }
        }

        private class ReplaceReadonlyVisitor : DepthFirstAstVisitor
        {
            private readonly string _name;
            private readonly object _value;

            public ReplaceReadonlyVisitor(string name, object value)
            {
                _name = name;
                _value = value;
            }

            public override void VisitIdentifier(Identifier identifier)
            {
                base.VisitIdentifier(identifier);

                if (identifier.Parent is MemberReferenceExpression member && identifier.Name == _name)
                {
                    var primitive = new PrimitiveExpression(_value);
                    primitive.AddAnnotation(member.GetResolveResult());
                    member.ReplaceWith(primitive);
                }
            }
        }
    }
}
