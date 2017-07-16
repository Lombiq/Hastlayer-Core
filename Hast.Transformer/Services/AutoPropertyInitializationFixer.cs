using System;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

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

                if (memberReferenceExpression.Annotation<FieldDefinition>() == null) return;

                memberReferenceExpression.MemberName = memberReferenceExpression.MemberName.ConvertSimpleBackingFieldNameToPropertyName();
            }
        }
    }
}
