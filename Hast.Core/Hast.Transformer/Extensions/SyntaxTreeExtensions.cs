using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class SyntaxTreeExtensions
    {
        public static IEnumerable<TypeDeclaration> GetAllTypeDeclarations(this SyntaxTree syntaxTree)
        {
            return syntaxTree.GetTypes(true).Where(type => type is TypeDeclaration).Cast<TypeDeclaration>();
        }
    }
}
