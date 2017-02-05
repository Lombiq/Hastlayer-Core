using System;
using System.Collections.Generic;
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

            var parameterReference = node.Annotation<ParameterReference>();
            if (parameterReference != null) return CreateEntityBasedName(node, parameterReference.Name);

            if (node is PrimitiveType) return ((PrimitiveType)node).Keyword;

            if (node is ComposedType)
            {
                var composedType = (ComposedType)node;

                var name = composedType.BaseType.GetFullName();

                if (composedType.ArraySpecifiers.Any())
                {
                    foreach (var arraySpecifier in composedType.ArraySpecifiers)
                    {
                        name += arraySpecifier.ToString();
                    }
                }

                return name;
            }

            if (node is IdentifierExpression)
            {
                return CreateEntityBasedName(node, ((IdentifierExpression)node).Identifier);
            }

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
                    if (declaringType == null) name = memberReference.FullName;
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

        public static T FindFirstParentOfType<T>(this AstNode node) where T : AstNode
        {
            node = node.Parent;

            while (node != null && !(node is T))
            {
                node = node.Parent;
            }

            return (T)node;
        }

        public static TypeDeclaration FindFirstParentTypeDeclaration(this AstNode node)
        {
            return node.FindFirstParentOfType<TypeDeclaration>();
        }

        public static EntityDeclaration FindFirstParentEntityDeclaration(this AstNode node)
        {
            return node.FindFirstParentOfType<EntityDeclaration>();
        }

        public static T FindFirstChildOfType<T>(this AstNode node) where T : AstNode
        {
            var children = new Queue<AstNode>(node.Children);

            while (children.Count != 0)
            {
                var currentChild = children.Dequeue();

                if (currentChild is T) return (T)currentChild;

                foreach (var child in currentChild.Children)
                {
                    children.Enqueue(child);
                }
            }

            return null;
        }

        public static bool Is<T>(this AstNode node, Predicate<T> predicate) where T : AstNode
        {
            var castNode = node as T;
            if (castNode == null) return false;
            return predicate(castNode);
        }


        private static string CreateEntityBasedName(AstNode node, string name)
        {
            return node.FindFirstParentEntityDeclaration().GetFullName() + "." + name;
        }
    }
}
