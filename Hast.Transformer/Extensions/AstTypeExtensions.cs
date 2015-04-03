using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstTypeExtensions
    {
        public static bool TypeEquals(this AstType astType, AstType other, Func<AstType, TypeDeclaration> lookupDeclaration)
        {
            if (astType is PrimitiveType && other is PrimitiveType)
            {
                return ((PrimitiveType)astType).Keyword == ((PrimitiveType)other).Keyword;
            }
            else if (astType is ComposedType && other is ComposedType)
            {
                return ((ComposedType)astType).BaseType.TypeEquals(((ComposedType)other).BaseType, lookupDeclaration);
            }
            else
            {
                return lookupDeclaration(astType) == lookupDeclaration(other);
            }
        }
    }
}
