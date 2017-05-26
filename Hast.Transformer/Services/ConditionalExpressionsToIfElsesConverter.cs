using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services
{
    public class ConditionalExpressionsToIfElsesConverter : IConditionalExpressionsToIfElsesConverter
    {
        public void ConvertConditionalExpressionsToIfElses(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new ConditionalExpressionsConvertingVisitor());
        }


        private class ConditionalExpressionsConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                base.VisitConditionalExpression(conditionalExpression);

                var assignment = conditionalExpression.Parent as AssignmentExpression;

                // Only simple "variable = condition ? value1 : value2" expressions are supported now.
                if (assignment == null || 
                    !(assignment.Left is IdentifierExpression) ||
                    !(assignment.Parent is ExpressionStatement))
                    return;

                var trueAssignment = (AssignmentExpression)assignment.Clone();
                trueAssignment.Right = conditionalExpression.TrueExpression.Clone();
                var falseAssignment = (AssignmentExpression)assignment.Clone();
                falseAssignment.Right = conditionalExpression.FalseExpression.Clone();

                conditionalExpression.Parent.Parent
                    .ReplaceWith(new IfElseStatement(conditionalExpression.Condition.Clone(), trueAssignment, falseAssignment));
            }
        }
    }
}
