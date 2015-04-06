using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    internal class ReferencedNodesFlaggingVisitor : DepthFirstAstVisitor
    {
        private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;


        public ReferencedNodesFlaggingVisitor(ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            _typeDeclarationLookupTable = typeDeclarationLookupTable;
        }


        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            base.VisitMemberReferenceExpression(memberReferenceExpression);


            if (memberReferenceExpression.Target is TypeReferenceExpression)
            {
                var typeReferenceExpression = (TypeReferenceExpression)memberReferenceExpression.Target;
                if (typeReferenceExpression.Type is SimpleType && ((SimpleType)typeReferenceExpression.Type).Identifier == "MethodImplOptions")
                {
                    // This can happen when a method is extern (see: https://msdn.microsoft.com/en-us/library/e59b22c5.aspx), thus has no body
                    // but has the MethodImpl attribute (e.g. Math.Abs(double value). Nothing to do.
                    return;
                }
            }


            var member = memberReferenceExpression.GetMemberDeclaration(_typeDeclarationLookupTable);

            if (member == null || member.WasVisited()) return;

            // Using the reference expression as the "from", since e.g. two calls to the same method should be counted twice, even if from the
            // same method.
            member.AddReference(memberReferenceExpression);

            member.GetParentType().AddReference(memberReferenceExpression);

            member.SetVisited();

            // Since when e.g. another method is referenced that is above the level of this expression in the syntaxt tree,
            // thus it won't be visited unless we start a visitor there too.
            member.AcceptVisitor(this);
        }
    }
}
