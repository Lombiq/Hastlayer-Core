using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class SyntaxTreeExtensions
    {
        public static IEnumerable<TypeDeclaration> GetAllTypeDeclarations(this SyntaxTree syntaxTree)
        {
            return syntaxTree.GetTypes(true).Where(type => type is TypeDeclaration).Cast<TypeDeclaration>();
        }
    }
}
