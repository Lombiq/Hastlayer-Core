using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstNodeExtensions
    {
        public static string GetFullName(this AstNode node)
        {
            var memberDefinition = node.Annotation<IMemberDefinition>();
            if (memberDefinition == null)
            {
                throw new InvalidOperationException("This node doesn't have a name.");
            }
            return memberDefinition.FullName;
        }
    }
}
