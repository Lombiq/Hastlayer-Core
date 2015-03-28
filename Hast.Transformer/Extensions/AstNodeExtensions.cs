using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstNodeExtensions
    {
        public static string GetFullName(this AstNode node)
        {
            var memberDefinition = node.Annotation<IMemberDefinition>();
            if (memberDefinition != null) return memberDefinition.FullName;

            var memberReference = node.Annotation<MemberReference>();
            if (memberReference != null) return memberReference.FullName;

            throw new InvalidOperationException("This node doesn't have a name.");
        }

        public static TypeDeclaration GetParentType(this AstNode node)
        {
            while (!(node is TypeDeclaration))
            {
                node = node.Parent;
            }

            return (TypeDeclaration)node;
        }
    }
}
