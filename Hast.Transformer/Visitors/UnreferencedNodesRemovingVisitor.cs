using System.Linq;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    internal class UnreferencedNodesRemovingVisitor : DepthFirstAstVisitor
    {
        public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
        {
            base.VisitCustomEventDeclaration(eventDeclaration);
            RemoveIfUnreferenced(eventDeclaration);
        }

        public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
        {
            base.VisitEventDeclaration(eventDeclaration);
            RemoveIfUnreferenced(eventDeclaration);
        }

        public override void VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration)
        {
            base.VisitDelegateDeclaration(delegateDeclaration);
            RemoveIfUnreferenced(delegateDeclaration);
        }

        public override void VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration)
        {
            base.VisitExternAliasDeclaration(externAliasDeclaration);
            RemoveIfUnreferenced(externAliasDeclaration);
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            base.VisitTypeDeclaration(typeDeclaration);

            var unreferencedMembers = typeDeclaration.Members.Where(member => !member.IsReferenced());

            if (typeDeclaration.Members.Count == unreferencedMembers.Count())
            {
                typeDeclaration.Remove();
            }

            foreach (var member in unreferencedMembers)
            {
                member.Remove();
            }
        }


        private static void RemoveIfUnreferenced(AstNode node)
        {
            if (!node.IsReferenced()) node.Remove();
        }
    }
}
