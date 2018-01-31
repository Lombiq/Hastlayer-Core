using System.Linq;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class ObjectCreateExpressionExtensions
    {
        public static string GetConstructorFullName(this ObjectCreateExpression objectCreateExpression) =>
            objectCreateExpression.Annotation<MethodReference>()?.FullName;

        public static EntityDeclaration FindConstructorDeclaration(
            this ObjectCreateExpression objectCreateExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var constructorName = objectCreateExpression.GetConstructorFullName();

            if (string.IsNullOrEmpty(constructorName)) return null;

            var createdTypeName = objectCreateExpression.Type.GetFullName();

            var constructorType = typeDeclarationLookupTable.Lookup(createdTypeName);

            if (constructorType == null) ExceptionHelper.ThrowDeclarationNotFoundException(createdTypeName);

            return constructorType
                .Members
                .SingleOrDefault(member => member.GetFullName() == constructorName);
        }
    }
}
