using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class RemainderOperatorExpressionsExpander : IRemainderOperatorExpressionsExpander
    {
        public void ExpandRemainderOperatorExpressions(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new RemainderOperatorExpressionsExpanderVisitor());
        }


        private class RemainderOperatorExpressionsExpanderVisitor : DepthFirstAstVisitor
        {
            public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
            {
                base.VisitBinaryOperatorExpression(binaryOperatorExpression);

                if (binaryOperatorExpression.Operator != BinaryOperatorType.Modulus) return;

                // Changing a % b to a – a / b * b.
                // At this point the operands should have the same type, so it's safe just clone around.

                if (binaryOperatorExpression.GetActualTypeReference() == null)
                {
                    binaryOperatorExpression
                        .AddAnnotation(binaryOperatorExpression.Left.GetTypeInformationOrCreateFromActualTypeReference());
                }

                // First assigning the operands to new variables so if method calls, casts or anything are in there
                // those are not duplicated.

                void createVariableForOperand(Expression operand)
                {
                    // Don't create a variable if it's not necessary.
                    // Primitive values should be left out because operations with primitive operands can be faster on
                    // hardware.
                    if (operand is PrimitiveExpression || operand is IdentifierExpression)
                    {
                        return;
                    }

                    var variableIdentifier = VariableHelper.DeclareAndReferenceVariable(
                        // Need to add ILRange because there can be multiple remainder operations for the same variable
                        // so somehow we need to distinguish between them.
                        "remainderOperand" + operand.GetILRangeName(),
                        operand,
                        TypeHelper.CreateAstType(operand.GetActualTypeReference()));

                    var assignment = new AssignmentExpression(variableIdentifier, operand.Clone())
                        .WithAnnotation(operand.GetTypeInformationOrCreateFromActualTypeReference());

                    AstInsertionHelper.InsertStatementBefore(
                        binaryOperatorExpression.FindFirstParentStatement(),
                        new ExpressionStatement(assignment));

                    operand.ReplaceWith(variableIdentifier.Clone());
                }

                createVariableForOperand(binaryOperatorExpression.Left);
                createVariableForOperand(binaryOperatorExpression.Right);

                // Building the chained operation from the inside out.

                // a / b
                var dividingExpression = (BinaryOperatorExpression)binaryOperatorExpression.Clone();
                dividingExpression.Operator = BinaryOperatorType.Divide;

                // a / b * b
                var multiplyingExpression = (BinaryOperatorExpression)binaryOperatorExpression.Clone();
                multiplyingExpression.Operator = BinaryOperatorType.Multiply;
                multiplyingExpression.Left = dividingExpression;

                // a – a / b * b
                binaryOperatorExpression.Operator = BinaryOperatorType.Subtract;
                binaryOperatorExpression.Right = multiplyingExpression;
            }
        }
    }
}