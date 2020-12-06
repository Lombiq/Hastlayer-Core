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
                _replacements = configuration.GetOrAddReplacements();
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
                    .FindFirstChildOfType<VariableInitializer>();
                var value = initializer?.FindFirstChildOfType<PrimitiveExpression>();
                if (initializer == null || value == null) return;

                var targetAttributeName = typeof(ReplaceableAttribute).FullName;
                var replaceable = fieldDeclaration
                    .Attributes
                    .SelectMany(attributeSection => attributeSection.FindAllChildrenOfType<Attribute>())
                    .SingleOrDefault(attribute => attribute.Type.GetFullName() == targetAttributeName);

                if (replaceable == null) return;

                var typeDeclaration = initializer.FindFirstParentOfType<TypeDeclaration>();

                var key = replaceable.FindFirstChildOfType<PrimitiveExpression>().Value.ToString();

                // If a replacement value is not set then there is nothing to do.
                if (key == null || !_replacements.TryGetValue(key, out var result)) return;

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

            private static IType GetTypeOfStaticIdentifier(Identifier identifier) =>
                (identifier.Parent as MemberReferenceExpression)?
                .FindFirstChildOfType<TypeReferenceExpression>()?
                .Type
                .GetActualType();
        }
    }
}
