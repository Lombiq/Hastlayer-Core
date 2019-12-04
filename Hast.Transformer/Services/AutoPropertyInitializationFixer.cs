using ICSharpCode.Decompiler.CSharp.Syntax;
using System;

namespace Hast.Transformer.Services
{
    public class AutoPropertyInitializationFixer : IAutoPropertyInitializationFixer
    {
        public void FixAutoPropertyInitializations(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new ConstructorFindingVisitor());
        }


        private class ConstructorFindingVisitor : DepthFirstAstVisitor
        {
            public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
            {
                base.VisitConstructorDeclaration(constructorDeclaration);

                constructorDeclaration.AcceptVisitor(new AutoPropertyInitializationFixingVisitor());
            }
        }

        private class AutoPropertyInitializationFixingVisitor : DepthFirstAstVisitor
        {
            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                if (!memberReferenceExpression.IsFieldReference()) return;

                memberReferenceExpression.MemberName = memberReferenceExpression.MemberName.ConvertSimpleBackingFieldNameToPropertyName();
            }
        }
    }
}
