using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class UnaryIncrementsDecrementsConverter : IUnaryIncrementsDecrementsConverter
    {
        public void ConvertUnaryIncrementsDecrements(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new UnaryIncrementsDecrementsConvertingVisitor());
        }


        private class UnaryIncrementsDecrementsConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
            {
                base.VisitUnaryOperatorExpression(unaryOperatorExpression);

                var incrementDecrementOperators = new[]
                {
                    UnaryOperatorType.Decrement,
                    UnaryOperatorType.Increment,
                    UnaryOperatorType.PostDecrement,
                    UnaryOperatorType.PostIncrement
                };

                var operatorType = unaryOperatorExpression.Operator;

                if (!incrementDecrementOperators.Contains(operatorType)) return;

                var binaryOperator =
                    operatorType == UnaryOperatorType.Increment || operatorType == UnaryOperatorType.PostIncrement ?
                    BinaryOperatorType.Add :
                    BinaryOperatorType.Subtract;

                var typeInformation = unaryOperatorExpression.GetTypeInformationOrCreateFromActualTypeReference();

                var binaryExpression = new BinaryOperatorExpression(
                    unaryOperatorExpression.Expression.Clone(),
                    binaryOperator,
                    new PrimitiveExpression(1).WithAnnotation(typeInformation))
                    .WithAnnotation(typeInformation);


                var assignment = new AssignmentExpression(unaryOperatorExpression.Expression.Clone(), binaryExpression)
                    .WithAnnotation(typeInformation);

                var statement = new ExpressionStatement(assignment);
                var parentStatement = unaryOperatorExpression.FindFirstParentStatement();

                // This substitution will only work correctly if there's no other reference to the value holder in this 
                // statement, since a new statement is inserted skipping all other expressions in this statement.
                // Otherwise those will also mistakenly use the modified value. So checking for other expressions here.

                var expressionName = unaryOperatorExpression.Expression.GetFullName();
                var otherReference = parentStatement.FindFirstChildOfType<Expression>(expression =>
                    expression != unaryOperatorExpression.Expression && expression.GetFullName() == expressionName);

                if (otherReference != null)
                {
                    throw new NotSupportedException(
                        "There are multiple references to " + unaryOperatorExpression.Expression.ToString() +
                        " in the statement " + parentStatement.ToString() + " while it is also subject of the unary expression " +
                        unaryOperatorExpression.ToString() +
                        ". This prevents the unary expression from being converted to a standard binary expression and thus allow Hastlayer to transform it." +
                        " Split the statement so one statement only includes a single reference to " +
                        unaryOperatorExpression.Expression.ToString() + ".".AddParentEntityName(unaryOperatorExpression));
                }

                if (operatorType == UnaryOperatorType.Increment || operatorType == UnaryOperatorType.Decrement)
                {
                    AstInsertionHelper.InsertStatementBefore(parentStatement, statement);
                }
                else
                {
                    AstInsertionHelper.InsertStatementAfter(parentStatement, statement);
                }

                unaryOperatorExpression.ReplaceWith(unaryOperatorExpression.Expression.Clone());
            }
        }
    }
}
