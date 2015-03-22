using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class SyntaxTreeExtensions
    {
        public static IEnumerable<TypeDeclaration> GetMatchingTypes(this SyntaxTree syntaxTree, string name)
        {
            return syntaxTree.GetTypes().Where(typeDeclaration => typeDeclaration.GetFullName().EndsWith(name));
        }
    }
}
