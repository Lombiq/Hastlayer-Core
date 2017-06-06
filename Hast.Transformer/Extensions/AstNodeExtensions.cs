using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.ILAst;
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
            if (parameterReference != null) return CreateParentEntityBasedName(node, parameterReference.Name);

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
                return CreateParentEntityBasedName(node, ((IdentifierExpression)node).Identifier);
            }

            if (node is BinaryOperatorExpression)
            {
                return CreateParentEntityBasedName(node, node.ToString());
            }

            if (node is IndexerExpression)
            {
                return ((IndexerExpression)node).Target.GetFullName();
            }

            // This is a member on a type that's not transformed, like System.Array.
            if (node is MemberReferenceExpression)
            {
                var memberReferenceExpression = (MemberReferenceExpression)node;
                return memberReferenceExpression.Target.GetFullName() + "." + memberReferenceExpression.MemberName;
            }

            if (node is Identifier)
            {
                return ((Identifier)node).Name;
            }

            if (node is Attribute)
            {
                return ((Attribute)node).Type.GetFullName();
            }

            if (node is TypeReferenceExpression)
            {
                return ((TypeReferenceExpression)node).Type.GetFullName();
            }

            var ilVariable = node.Annotation<ILVariable>();
            if (ilVariable != null)
            {
                return CreateParentEntityBasedName(node, ilVariable.Name);
            }

            if (node is PrimitiveExpression)
            {
                return ((PrimitiveExpression)node).Value.ToString();
            }

            if (node is VariableInitializer)
            {
                return node.FindFirstParentEntityDeclaration().GetFullName();
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

        public static bool IsIn<T>(this AstNode node) where T : AstNode =>
            node.FindFirstParentOfType<T>() != null;

        public static TypeDeclaration FindFirstParentTypeDeclaration(this AstNode node) =>
            node.FindFirstParentOfType<TypeDeclaration>();

        public static EntityDeclaration FindFirstParentEntityDeclaration(this AstNode node) =>
            node.FindFirstParentOfType<EntityDeclaration>();

        public static BlockStatement FindFirstParentBlockStatement(this AstNode node) =>
            node.FindFirstParentOfType<BlockStatement>();

        public static T FindFirstParentOfType<T>(this AstNode node) where T : AstNode =>
            node.FindFirstParentOfType<T>(n => true);

        public static T FindFirstParentOfType<T>(this AstNode node, Predicate<T> predicate) where T : AstNode
        {
            int height;
            return node.FindFirstParentOfType<T>(predicate, out height);
        }

        public static T FindFirstParentOfType<T>(this AstNode node, Predicate<T> predicate, out int height) where T : AstNode
        {
            height = 1;
            node = node.Parent;

            while (node != null && !(node is T && predicate((T)node)))
            {
                node = node.Parent;
                height++;
            }

            return (T)node;
        }

        public static T FindFirstChildOfType<T>(this AstNode node) where T : AstNode =>
            node.FindFirstChildOfType<T>(n => true);

        public static T FindFirstChildOfType<T>(this AstNode node, Predicate<T> predicate) where T : AstNode
        {
            var children = new Queue<AstNode>(node.Children);

            while (children.Count != 0)
            {
                var currentChild = children.Dequeue();

                var castCurrentChild = currentChild as T;
                if (castCurrentChild != null && predicate(castCurrentChild)) return castCurrentChild;

                foreach (var child in currentChild.Children)
                {
                    children.Enqueue(child);
                }
            }

            return null;
        }

        public static bool Is<T>(this AstNode node, Predicate<T> predicate) where T : AstNode
        {
            T castNode;
            return node.Is(predicate, out castNode);
        }

        public static bool Is<T>(this AstNode node, out T castNode) where T : AstNode =>
            node.Is(n => true, out castNode);

        public static bool Is<T>(this AstNode node, Predicate<T> predicate, out T castNode) where T : AstNode
        {
            castNode = node as T;
            if (castNode == null) return false;
            return predicate(castNode);
        }

        public static AstNode FindFirstNonParenthesizedExpressionParent(this AstNode node)
        {
            var parent = node.Parent;

            while (parent is ParenthesizedExpression && parent != null)
            {
                parent = parent.Parent;
            }

            return parent;
        }


        private static string CreateParentEntityBasedName(AstNode node, string name) =>
            node.FindFirstParentEntityDeclaration().GetFullName() + "." + name;
    }
}
