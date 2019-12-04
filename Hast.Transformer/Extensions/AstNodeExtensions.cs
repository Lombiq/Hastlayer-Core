using System;
using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class AstNodeExtensions
    {
        /// <summary>
        /// Retrieves the node's full name, including (if applicable) information about return type, type parameters, 
        /// arguments...
        /// </summary>
        public static string GetFullName(this AstNode node)
        {
            if (node is MemberReferenceExpression memberReferenceExpression)
            {
                return memberReferenceExpression.Target.GetFullName() + "." + memberReferenceExpression.MemberName;
            }

            if (node is ObjectCreateExpression)
            {
                return node.CreateNameForUnnamedNode();
            }

            if (node is InvocationExpression)
            {
                return node.CreateNameForUnnamedNode();
            }

            var referencedMemberFullName = node.GetReferencedMemberFullName();
            if (!string.IsNullOrEmpty(referencedMemberFullName)) return referencedMemberFullName;

            var parameterReference = node.Annotation<ParameterReference>();
            if (parameterReference != null) return CreateParentEntityBasedName(node, parameterReference.Name);

            if (node is PrimitiveType) return ((PrimitiveType)node).Keyword;

            if (node is ComposedType composedType)
            {
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

            if (node is IndexerExpression)
            {
                return ((IndexerExpression)node).Target.GetFullName();
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
                if (node.Parent is FieldDeclaration)
                {
                    return node.FindFirstParentEntityDeclaration().GetFullName();
                }

                return CreateParentEntityBasedName(node, ((VariableInitializer)node).Name);
            }

            if (node is AssignmentExpression assignment)
            {
                return node.CreateNameForUnnamedNode() + assignment.Left.GetFullName() + assignment.Right.GetFullName();
            }

            if (node == Expression.Null || node == Statement.Null)
            {
                return string.Empty;
            }

            return node.CreateNameForUnnamedNode();
        }

        public static string GetILRangeName(this AstNode node)
        {
            // The node or parents should contain one or more ILRange objects which maybe correspond to the node's
            // location in the original IL.
            var ilRange = node.Annotation<List<ILRange>>();
            if (ilRange == null)
            {
                ilRange = node
                    .FindFirstParentOfType<AstNode>(parent => parent.Annotations.Any(annotation => annotation is List<ILRange>))
                    ?.Annotation<List<ILRange>>();
            }
            return ilRange != null ? ilRange.First().ToString() : string.Empty;
        }

        /// <summary>
        /// Retrieves the node's full name (see <see cref="GetFullName(AstNode)"/>) and if it's a property, produces
        /// a unified name for all of its forms (like backing field, compiler-generated get/set methods).
        /// </summary>
        public static string GetFullNameWithUnifiedPropertyName(this AstNode node)
        {
            var name = node.GetFullName();

            // If this is a compiler-generated property getter or setter method then get the real property name.
            var methodDefinition = node.Annotation<MethodDefinition>();
            if (methodDefinition != null && (methodDefinition.IsGetter || methodDefinition.IsSetter))
            {
                name = methodDefinition.IsGetter ?
                    name.Replace("::get_", "::") :
                    name.Replace("::set_", "::");
            }
            else if (name.IsBackingFieldName())
            {
                name = name.ConvertFullBackingFieldNameToPropertyName();
            }

            return name;
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

            var childIsNested = false;
            while (declaringType != null)
            {
                // The delimiter between the name of an inner class and its parent needs to be a plus.
                var delimiter = childIsNested ? "+" : ".";

                if (declaringType.DeclaringType == null)
                {
                    name = declaringType.FullName + delimiter + name;
                }
                else
                {
                    name = declaringType.Name + delimiter + name;
                }

                childIsNested = declaringType.IsNested;
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

        public static Statement FindFirstParentStatement(this AstNode node) =>
            node.FindFirstParentOfType<Statement>();

        public static BlockStatement FindFirstParentBlockStatement(this AstNode node) =>
            node.FindFirstParentOfType<BlockStatement>();

        public static T FindFirstParentOfType<T>(this AstNode node) where T : AstNode =>
            node.FindFirstParentOfType<T>(n => true);

        public static T FindFirstParentOfType<T>(this AstNode node, Predicate<T> predicate) where T : AstNode =>
            node.FindFirstParentOfType(predicate, out int height);

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

                if (currentChild is T castCurrentChild && predicate(castCurrentChild)) return castCurrentChild;

                foreach (var child in currentChild.Children)
                {
                    children.Enqueue(child);
                }
            }

            return null;
        }

        public static bool Is<T>(this AstNode node, Predicate<T> predicate) where T : AstNode =>
            node.Is(predicate, out T castNode);

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

        public static T WithAnnotation<T>(this T node, object annotation) where T : AstNode
        {
            node.AddAnnotation(annotation);
            return node;
        }

        /// <summary>
        /// Replaces all annotations with the type of the given new annotation with the supplied instance of the new
        /// annotation.
        /// </summary>
        public static TNode ReplaceAnnotations<TNode, TAnnotation>(this TNode node, TAnnotation annotation)
            where TNode : AstNode
            where TAnnotation : class
        {
            node.RemoveAnnotations<TAnnotation>();
            node.AddAnnotation(annotation);

            return node;
        }


        internal static string GetReferencedMemberFullName(this AstNode node)
        {
            var memberDefinition = node.Annotation<IMemberDefinition>();
            if (memberDefinition != null) return memberDefinition.FullName;

            var memberReference = node.Annotation<MemberReference>();
            if (memberReference != null) return memberReference.FullName;

            return null;
        }


        private static string CreateNameForUnnamedNode(this AstNode node) =>
            // The node doesn't really have a name so give it one that is suitably unique.
            CreateParentEntityBasedName(node, node.ToString() + node.GetILRangeName());

        private static string CreateParentEntityBasedName(AstNode node, string name) =>
            node.FindFirstParentEntityDeclaration().GetFullName() + "." + name;
    }
}
