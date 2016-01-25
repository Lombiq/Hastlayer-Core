using System;
using System.Linq;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstNodeExtensions
    {
        /// <summary>
        /// Retrieves the node's full name, including (if applicable) information about return type, type parameters, 
        /// arguments...
        /// </summary>
        public static string GetFullName(this AstNode node)
        {
            var memberDefinition = node.Annotation<IMemberDefinition>();
            if (memberDefinition != null) return memberDefinition.FullName;

            var memberReference = node.Annotation<MemberReference>();
            if (memberReference != null) return memberReference.FullName;

            throw new InvalidOperationException("This node doesn't have a name.");
        }

        /// <summary>
        /// Retrieves the simple dot-delimited name of a type, including the parent types' and the wrapping namespace's 
        /// name.
        /// </summary>
        public static string GetSimpleName(this AstNode node)
        {
            string name = null;
            TypeReference declaringType = null;

            var memberDefinition = node.Annotation<IMemberDefinition>();
            if (memberDefinition != null)
            {
                name = memberDefinition.Name;
                declaringType = memberDefinition.DeclaringType;
                if (declaringType == null) name = memberDefinition.FullName;
            }
            else
            {
                var memberReference = node.Annotation<MemberReference>();
                if (memberReference != null)
                {
                    name = memberReference.Name;
                    declaringType = memberReference.DeclaringType;
                    if (declaringType == null) name = memberDefinition.FullName;
                }
            }

            // The name is already a full name, but for a different declaring type. This is the case for explicitly 
            // implemented interface methods.
            if (name.Contains('.'))
            {
                name = name.Substring(name.LastIndexOf('.') + 1);
            }

            while (declaringType != null)
            {
                if (declaringType.DeclaringType == null)
                {
                    name = declaringType.FullName + "." + name;
                }
                else
                {
                    name = declaringType.Name + "." + name;
                }

                declaringType = declaringType.DeclaringType;
            }

            return name;
        }

        public static TypeDeclaration FindParentType(this AstNode node)
        {
            while (!(node is TypeDeclaration))
            {
                node = node.Parent;
            }

            return (TypeDeclaration)node;
        }
    }
}
