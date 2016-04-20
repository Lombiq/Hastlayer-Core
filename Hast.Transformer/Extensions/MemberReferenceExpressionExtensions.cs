using System.Linq;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MemberReferenceExpressionExtensions
    {
        public static EntityDeclaration GetMemberDeclaration(
            this MemberReferenceExpression memberReferenceExpression, 
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var type = memberReferenceExpression.GetTargetTypeDeclaration(typeDeclarationLookupTable);

            if (type == null) return null;

            var parent = memberReferenceExpression.Parent;
            MemberReference memberReference = null;
            while (memberReference == null && parent != null)
            {
                memberReference = parent.Annotation<MemberReference>();
                parent = parent.Parent;
            }

            return type.Members.Where(member => member.Annotation<MemberReference>().FullName == memberReference.FullName).SingleOrDefault();
        }

        public static TypeDeclaration GetTargetTypeDeclaration(
            this MemberReferenceExpression memberReferenceExpression, 
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            if (memberReferenceExpression.Target is TypeReferenceExpression)
            {
                // The member is in a different class.
                return typeDeclarationLookupTable.Lookup((TypeReferenceExpression)memberReferenceExpression.Target);
            }
            else if (memberReferenceExpression.Target is BaseReferenceExpression)
            {
                // The member is in the base class (because of single class inheritance in C#, there can be only one base class).
                return memberReferenceExpression.FindParentTypeDeclaration().BaseTypes
                    .Select(type => typeDeclarationLookupTable.Lookup(type))
                    .SingleOrDefault(typeDeclaration => typeDeclaration != null && typeDeclaration.ClassType == ClassType.Class);
            }
            else if (memberReferenceExpression.Target is IdentifierExpression)
            {
                return typeDeclarationLookupTable.Lookup(memberReferenceExpression.Target.GetActualTypeReference().FullName);
            }
            else
            {
                // The member is within this class.
                return memberReferenceExpression.FindParentTypeDeclaration();
            }
        }
    }
}
