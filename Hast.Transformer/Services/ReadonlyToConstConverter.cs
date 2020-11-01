using Hast.Layer;
using Hast.Synthesis.Abstractions;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
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

                var targetAttributeName = typeof(ReplaceableAttribute).FullName;
                var replaceable = fieldDeclaration
                    .Attributes
                    .SelectMany(attributeSection => attributeSection.Descendants.OfType<Attribute>())
                    .SingleOrDefault(attribute => attribute.Type.GetFullName() == targetAttributeName);

                if (replaceable == null) return;

                var typeDeclaration = initializer.Ancestors.OfType<TypeDeclaration>().First();

                var key = replaceable.Descendants.OfType<PrimitiveExpression>().Single().Value.ToString();
                // If a replacement value is set, override the result.
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

                // This is not optional. Even if there is no custom replacement found, the literal substitution must be
                // performed to restore the the const-like behavior.
                _syntaxTree.AcceptVisitor(new ReplaceReadonlyVisitor(initializer.Name, value.Value, typeDeclaration));
            }
        }

        private class ReplaceReadonlyVisitor : DepthFirstAstVisitor
        {
            private readonly string _name;
            private readonly object _value;
            private readonly TypeDeclaration _typeDeclaration;

            public ReplaceReadonlyVisitor(string name, object value, TypeDeclaration typeDeclaration)
            {
                _name = name;
                _value = value;
                _typeDeclaration = typeDeclaration;
            }

            public override void VisitIdentifier(Identifier identifier)
            {
                base.VisitIdentifier(identifier);
                if (!(identifier.Parent is MemberReferenceExpression member) ||
                    identifier.Name != _name ||
                    !GetTypeOfStaticIdentifier(identifier).Equals(_typeDeclaration.GetActualType()))
                {
                    return;
                }

                var primitive = new PrimitiveExpression(_value);
                primitive.AddAnnotation(member.GetResolveResult());
                member.ReplaceWith(primitive);
            }

            private IType GetTypeOfStaticIdentifier(Identifier identifier) =>
                (identifier.Parent as MemberReferenceExpression)?
                .Children
                .OfType<TypeReferenceExpression>()
                .FirstOrDefault()?
                .Type
                .GetActualType();
        }
    }
}
