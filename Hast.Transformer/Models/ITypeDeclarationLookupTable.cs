using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    public interface ITypeDeclarationLookupTable
    {
        /// <summary>
        /// Retrieves the type declaration, given an AST type.
        /// </summary>
        /// <param name="type">The AST type to look up the type declaration for.</param>
        /// <returns>The retrieved <see cref="TypeDeclaration"/> if found or <c>null</c> otherwise.</returns>
        TypeDeclaration Lookup(AstType type);
    }


    public static class TypeDeclarationLookupTable
    {
        public static TypeDeclaration Lookup(this ITypeDeclarationLookupTable typeDeclarationLookupTable, TypeReferenceExpression typeReferenceExpression)
        {
            return typeDeclarationLookupTable.Lookup(typeReferenceExpression.Type);
        }
    }
}
