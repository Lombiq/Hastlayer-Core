using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class MemberReferenceExpressionExtensions
    {
        /// <summary>
        /// Find the referenced member's declaration.
        /// </summary>
        /// <param name="typeDeclarationLookupTable">
        /// The <see cref="ITypeDeclarationLookupTable"/> instance corresponding to the current scope.
        /// </param>
        /// <param name="findLeftmostMemberIfRecursive">
        /// If the member reference references another member (like <c>this.Property1.Property2.Property3</c>) then if 
        /// set to <c>true</c> the member corresponding to the leftmost member (<c>this.Property1</c> in this case) will
        /// be looked up.
        /// </param>
        public static EntityDeclaration FindMemberDeclaration(
            this MemberReferenceExpression memberReferenceExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable,
            bool findLeftmostMemberIfRecursive = false)
        {
            TypeDeclaration type;

            if (memberReferenceExpression.Target is MemberReferenceExpression)
            {
                if (findLeftmostMemberIfRecursive)
                {
                    return ((MemberReferenceExpression)memberReferenceExpression.Target).FindMemberDeclaration(typeDeclarationLookupTable, true);
                }
                else
                {
                    type = typeDeclarationLookupTable.Lookup(memberReferenceExpression.Target.GetActualTypeFullName());
                }
            }
            else
            {
                type = memberReferenceExpression.FindTargetTypeDeclaration(typeDeclarationLookupTable);
            }

            if (type == null) return null;

            var fullName = memberReferenceExpression.GetReferencedMemberFullName();
            if (string.IsNullOrEmpty(fullName)) return null;
            return type
                .Members
                .SingleOrDefault(member => member.GetFullName() == fullName);
        }

        public static TypeDeclaration FindTargetTypeDeclaration(
            this MemberReferenceExpression memberReferenceExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var target = memberReferenceExpression.Target;

            if (target is TypeReferenceExpression)
            {
                // The member is in a different class.
                return typeDeclarationLookupTable.Lookup((TypeReferenceExpression)target);
            }
            else if (target is BaseReferenceExpression)
            {
                // The member is in the base class (because of single class inheritance in C#, there can be only one base class).
                return memberReferenceExpression.FindFirstParentTypeDeclaration().BaseTypes
                    .Select(type => typeDeclarationLookupTable.Lookup(type))
                    .SingleOrDefault(typeDeclaration => typeDeclaration != null && typeDeclaration.ClassType == ClassType.Class);
            }
            else if (target is IdentifierExpression || target is IndexerExpression)
            {
                var type = target.GetActualType();
                return type == null ? null : typeDeclarationLookupTable.Lookup(type.GetFullName());
            }
            else if (target is MemberReferenceExpression)
            {
                return ((MemberReferenceExpression)target).FindTargetTypeDeclaration(typeDeclarationLookupTable);
            }
            else if (target is ObjectCreateExpression)
            {
                // The member is referenced in an object initializer.
                return typeDeclarationLookupTable.Lookup(((ObjectCreateExpression)target).Type);
            }
            else if (target is InvocationExpression)
            {
                var memberResolveResult = memberReferenceExpression.GetResolveResult<MemberResolveResult>();
                if (memberResolveResult != null)
                {
                    return typeDeclarationLookupTable.Lookup(memberResolveResult.Member.DeclaringType.GetFullName());
                }
            }

            // The member is within this class.
            return memberReferenceExpression.FindFirstParentTypeDeclaration();
        }

        public static string GetMemberFullName(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.GetReferencedMemberFullName();

        /// <summary>
        /// Determines if the member reference is an access to an array's Length property.
        /// </summary>
        public static bool IsArrayLengthAccess(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.MemberName == "Length" && memberReferenceExpression.Target.GetActualType().IsArray();

        public static bool IsTaskStartNew(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.MemberName == "StartNew" &&
            memberReferenceExpression.Target.GetActualTypeFullName() == typeof(System.Threading.Tasks.TaskFactory).FullName;

        public static bool IsMethodReference(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.GetResolveResult<MemberResolveResult>().Member is IMethod;

        public static bool IsFieldReference(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.GetResolveResult<MemberResolveResult>().Member is IField;
    }
}
