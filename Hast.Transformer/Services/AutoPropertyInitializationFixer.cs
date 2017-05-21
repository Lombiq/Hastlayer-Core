using System.Text.RegularExpressions;
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

                // The backing field's name will be something like "<Number>k__BackingField". It will contain the name
                // of the property.
                memberReferenceExpression.MemberName = 
                    Regex.Replace(memberReferenceExpression.MemberName, "<(.*)>.*BackingField", match => match.Groups[1].Value);
            }
        }
    }
}
