using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
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
            if (node is TypeDeclaration || node is AstType)
            {
                return node.GetActualTypeFullName();
            }

            if (node is EntityDeclaration entityDeclaration)
            {
                return node.GetResolveResult<MemberResolveResult>()?.GetFullName() ??
                    CreateParentEntityBasedName(node, entityDeclaration.Name);

            }

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

            var iLVariableResolveResult = node.GetResolveResult<ILVariableResolveResult>();
            if (iLVariableResolveResult != null) return CreateParentEntityBasedName(node, iLVariableResolveResult.Variable.Name);

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

            if (node is SimpleType)
            {
                return node.GetActualTypeFullName();
            }

            return node.CreateNameForUnnamedNode();
        }

        public static string GetILRangeName(this AstNode node)
        {
            var ilInstruction = node.Annotation<ILInstruction>();
            return ilInstruction != null ? ilInstruction.ILRanges.First().ToString() : string.Empty;
        }

        /// <summary>
        /// Retrieves the simple dot-delimited name of a type, including the parent types' and the wrapping namespace's 
        /// name.
        /// </summary>
        public static string GetSimpleName(this AstNode node) =>
            // Unlike formerly with Cecil, the FullName property is in a simple format.
            node.GetResolveResult<MemberResolveResult>()?.Member.ReflectionName ?? node.GetActualType().FullName;

        public static T GetResolveResult<T>(this AstNode node) where T : ResolveResult =>
            node.GetResolveResult() as T;

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
            node.FindFirstParentOfType(predicate, out _);

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

        public static T As<T>(this AstNode node) where T : AstNode => node as T;

        public static AstNode FindFirstNonParenthesizedExpressionParent(this AstNode node)
        {
            var parent = node.Parent;

            while (parent is ParenthesizedExpression && parent != null)
            {
                parent = parent.Parent;
            }

            return parent;
        }

        public static IType GetActualType(this AstNode node)
        {
            if (node is ParenthesizedExpression parenthesizedExpression)
            {
                return parenthesizedExpression.Expression.GetActualType();
            }

            if (node is IndexerExpression indexerExpression)
            {
                return indexerExpression.Target.GetActualType().GetElementType();
            }

            if (node is AssignmentExpression assignmentExpression)
            {
                return assignmentExpression.Left.GetActualType();
            }

            if (node is UnaryOperatorExpression unaryOperatorExpression)
            {
                return unaryOperatorExpression.Expression.GetActualType();
            }

            var type = node.GetResolveResult().Type;
            return type == SpecialType.UnknownType ? null : type;
        }

        public static string GetActualTypeFullName(this AstNode node) => node.GetActualType().GetFullName();

        public static ResolveResult CreateResolveResultFromActualType(this AstNode node) =>
            node.GetActualType().ToResolveResult();


        internal static string GetReferencedMemberFullName(this AstNode node)
        {
            var memberResolveResult = node.GetResolveResult<MemberResolveResult>();

            if (node is MemberReferenceExpression)
            {
                var property = memberResolveResult?.Member as IProperty;

                // For certain members only their parent invocation will contain usable ResolveResult (see:
                // https://github.com/icsharpcode/ILSpy/issues/1407). For properties this is the case every time since
                // from the IMember of the property we can't see whether the getter or setter is invoked, only from the
                // parent invocation (this is only true for custom properties, not for auto-properties).
                if (node.Parent is InvocationExpression &&
                    (memberResolveResult == null || 
                        ((MemberReferenceExpression)node).IsPropertyReference() && 
                            !property.Getter.IsCompilerGenerated() &&
                            !property.Setter.IsCompilerGenerated()))
                {
                    return node.Parent.GetResolveResult<InvocationResolveResult>()?.GetFullName();
                }

                // This will only be the case for Task.Factory.StartNew calls made from lambdas, e.g. the <Run>b__0
                // method here:
                // Task.Factory.StartNew (<>c__DisplayClass3_.<>9__0 ?? (<>c__DisplayClass3_.<>9__0 = <>c__DisplayClass3_.<Run>b__0), num);
                MethodGroupResolveResult methodGroupResolveResult;
                if ((methodGroupResolveResult = node.GetResolveResult<MethodGroupResolveResult>()) != null)
                {
                    return methodGroupResolveResult.Methods.Single().GetFullName();
                }
            }

            return memberResolveResult?.GetFullName();
        }

        private static string CreateNameForUnnamedNode(this AstNode node) =>
            // The node doesn't really have a name so give it one that is suitably unique.
            CreateParentEntityBasedName(node, node.ToString() + node.GetILRangeName());

        private static string CreateParentEntityBasedName(AstNode node, string name) =>
            node.FindFirstParentEntityDeclaration().GetFullName() + "." + name;
    }
}
