using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MemberReferenceExpressionExtensions
    {
        public static EntityDeclaration GetMemberDeclaration(this MemberReferenceExpression memberReferenceExpression, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var type = memberReferenceExpression.GetTargetType(typeDeclarationLookupTable);

            var parent = memberReferenceExpression.Parent;
            MemberReference memberReference = null;
            while (memberReference == null && parent != null)
            {
                memberReference = parent.Annotation<MemberReference>();
                parent = parent.Parent;
            }

            return type.Members.Where(member => member.Annotation<MemberReference>().FullName == memberReference.FullName).SingleOrDefault();
        }

        public static TypeDeclaration GetTargetType(this MemberReferenceExpression memberReferenceExpression, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            if (memberReferenceExpression.Target is TypeReferenceExpression)
            {
                // The member is in a different class.
                return typeDeclarationLookupTable.Lookup((TypeReferenceExpression)memberReferenceExpression.Target);
            }
            else if (memberReferenceExpression.Target is BaseReferenceExpression)
            {
                // The member is in the base class (because of single class inheritance in C#, there can be only one base class).
                return memberReferenceExpression.GetParentType().BaseTypes
                    .Select(type => typeDeclarationLookupTable.Lookup(type))
                    .Single(typeDeclaration => typeDeclaration.ClassType == ClassType.Class);
            }
            else
            {
                // The member is within this class.
                return memberReferenceExpression.GetParentType();
            }
        }
    }
}
