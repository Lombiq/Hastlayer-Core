using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
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

                // Simple "variable = condition ? value1 : value2" expressions are easily handled but ones not in this
                // form need some further work.
                if (assignment == null ||
                    !(assignment.Left is IdentifierExpression) ||
                    !(assignment.Parent is ExpressionStatement))
                {
                    var variableName = "conditational" + conditionalExpression.ToString().GetHashCode().ToString();
                    var variableTypeReference = conditionalExpression.GetActualTypeReference();

                    // First creating a variable for the result.
                    var variableDeclaration = 
                        new VariableDeclarationStatement(AstBuilder.ConvertType(variableTypeReference), variableName);
                    AstInsertionHelper.InsertStatementBefore(
                        conditionalExpression.FindFirstParentOfType<Statement>(),
                        variableDeclaration);

                    // Then moving the conditational expression so its result is assigned to the variable.
                    var variableIdentifier = new IdentifierExpression(variableName);
                    variableIdentifier.AddAnnotation(new ILVariable { Name = variableName, Type = variableTypeReference });
                    var newConditionalExpression = (ConditionalExpression)conditionalExpression.Clone();
                    assignment = new AssignmentExpression(variableIdentifier, newConditionalExpression);
                    assignment.AddAnnotation(conditionalExpression.Annotation<TypeInformation>());
                    AstInsertionHelper.InsertStatementAfter(variableDeclaration, new ExpressionStatement(assignment));

                    // And finally swapping out the original expression with the variable reference.
                    conditionalExpression.ReplaceWith(variableIdentifier.Clone());
                    conditionalExpression = newConditionalExpression;
                }

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
