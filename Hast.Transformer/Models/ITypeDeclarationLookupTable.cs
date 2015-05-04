using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Models
{
    public interface ITypeDeclarationLookupTable
    {
        /// <summary>
        /// Retrieves the type declaration, given the type's full name.
        /// </summary>
        /// <param name="fullName">The type's full name (including the namespace) to look up the type declaration for.</param>
        /// <returns>The retrieved <see cref="TypeDeclaration"/> if found or <c>null</c> otherwise.</returns>
        TypeDeclaration Lookup(string fullName);
    }


    public static class TypeDeclarationLookupTableExtensions
    {
        /// <summary>
        /// Retrieves the type declaration, given an AST type.
        /// </summary>
        /// <param name="type">The AST type to look up the type declaration for.</param>
        /// <returns>The retrieved <see cref="TypeDeclaration"/> if found or <c>null</c> otherwise.</returns>
        public static TypeDeclaration Lookup(this ITypeDeclarationLookupTable typeDeclarationLookupTable, AstType type)
        {
            var typeReference = type.Annotation<TypeReference>();
            if (typeReference == null) return null;
            return typeDeclarationLookupTable.Lookup(typeReference.FullName);
        }

        public static TypeDeclaration Lookup(this ITypeDeclarationLookupTable typeDeclarationLookupTable, TypeReferenceExpression typeReferenceExpression)
        {
            return typeDeclarationLookupTable.Lookup(typeReferenceExpression.Type);
        }
    }
}
