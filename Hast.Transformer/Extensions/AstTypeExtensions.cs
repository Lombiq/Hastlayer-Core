using System;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstTypeExtensions
    {
        public static bool AstTypeEquals(this AstType astType, AstType other, Func<AstType, TypeDeclaration> lookupDeclaration)
        {
            if (astType is PrimitiveType && other is PrimitiveType)
            {
                return ((PrimitiveType)astType).Keyword == ((PrimitiveType)other).Keyword;
            }
            else if (astType is ComposedType && other is ComposedType)
            {
                return ((ComposedType)astType).BaseType.AstTypeEquals(((ComposedType)other).BaseType, lookupDeclaration);
            }
            else
            {
                return lookupDeclaration(astType) == lookupDeclaration(other);
            }
        }
    }
}
