using System;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services
{
    public class OperatorAssignmentsToSimpleAssignmentsConverter : IOperatorAssignmentsToSimpleAssignmentsConverter
    {
        public void ConvertOperatorAssignmentExpressionsToSimpleAssignments(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new OperatorAssignmentExpressionsToSimpleAssignmentsConvertingVisitor());
        }


        private class OperatorAssignmentExpressionsToSimpleAssignmentsConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                if (assignmentExpression.Operator == AssignmentOperatorType.Assign) return;

                BinaryOperatorType binaryOperator;

                switch (assignmentExpression.Operator)
                {
                    case AssignmentOperatorType.Add:
                        binaryOperator = BinaryOperatorType.Add;
                        break;
                    case AssignmentOperatorType.Subtract:
                        binaryOperator = BinaryOperatorType.Add;
                        break;
                    case AssignmentOperatorType.Multiply:
                        binaryOperator = BinaryOperatorType.Multiply;
                        break;
                    case AssignmentOperatorType.Divide:
                        binaryOperator = BinaryOperatorType.Divide;
                        break;
                    case AssignmentOperatorType.Modulus:
                        binaryOperator = BinaryOperatorType.Modulus;
                        break;
                    case AssignmentOperatorType.ShiftLeft:
                        binaryOperator = BinaryOperatorType.ShiftLeft;
                        break;
                    case AssignmentOperatorType.ShiftRight:
                        binaryOperator = BinaryOperatorType.ShiftRight;
                        break;
                    case AssignmentOperatorType.BitwiseAnd:
                        binaryOperator = BinaryOperatorType.BitwiseAnd;
                        break;
                    case AssignmentOperatorType.BitwiseOr:
                        binaryOperator = BinaryOperatorType.BitwiseOr;
                        break;
                    case AssignmentOperatorType.ExclusiveOr:
                        binaryOperator = BinaryOperatorType.ExclusiveOr;
                        break;
                    case AssignmentOperatorType.Any:
                        binaryOperator = BinaryOperatorType.Any;
                        break;
                    default:
                        throw new NotSupportedException(
                            "Converting assignments with the operator " + assignmentExpression.Operator.ToString() +
                            " is not supported. The assignment expression was: " + assignmentExpression.ToString()
                            .AddParentEntityName(assignmentExpression));
                }

                var binary = new BinaryOperatorExpression(
                    assignmentExpression.Left.Clone(),
                    binaryOperator,
                    assignmentExpression.Right.Clone());

                binary.AddAnnotation(assignmentExpression.GetActualTypeReference(true));

                assignmentExpression.Operator = AssignmentOperatorType.Assign;
                assignmentExpression.Right.ReplaceWith(binary);
            }
        }
    }
}
