using System;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    public class AstExpressionEvaluator : IAstExpressionEvaluator
    {
        public dynamic EvaluateBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            if (!(binaryOperatorExpression.Left is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Evaluating only binary operator expressions where both operands are primitive expressions are supported. The left expression was: " +
                    binaryOperatorExpression.Left.ToString() + ".");
            }

            if (!(binaryOperatorExpression.Right is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Evaluating only binary operator expressions where both operands are primitive expressions are supported. The right expression was: " +
                    binaryOperatorExpression.Right.ToString() + ".");
            }

            dynamic leftValue = ((PrimitiveExpression)binaryOperatorExpression.Left).Value;
            dynamic rightValue = ((PrimitiveExpression)binaryOperatorExpression.Right).Value;

            switch (binaryOperatorExpression.Operator)
            {
                //case BinaryOperatorType.Any:
                //    break;
                case BinaryOperatorType.BitwiseAnd:
                    return leftValue & rightValue;
                case BinaryOperatorType.BitwiseOr:
                    return leftValue | rightValue;
                case BinaryOperatorType.ConditionalAnd:
                    return leftValue && rightValue;
                case BinaryOperatorType.ConditionalOr:
                    return leftValue || rightValue;
                case BinaryOperatorType.ExclusiveOr:
                    return leftValue ^ rightValue;
                case BinaryOperatorType.GreaterThan:
                    return leftValue > rightValue;
                case BinaryOperatorType.GreaterThanOrEqual:
                    return leftValue >= rightValue;
                case BinaryOperatorType.Equality:
                    return leftValue.Equals(rightValue);
                case BinaryOperatorType.InEquality:
                    return !leftValue.Equals(rightValue);
                case BinaryOperatorType.LessThan:
                    return leftValue < rightValue;
                case BinaryOperatorType.LessThanOrEqual:
                    return leftValue <= rightValue;
                case BinaryOperatorType.Add:
                    return leftValue + rightValue;
                case BinaryOperatorType.Subtract:
                    return leftValue - rightValue;
                case BinaryOperatorType.Multiply:
                    return leftValue * rightValue;
                case BinaryOperatorType.Divide:
                    return leftValue / rightValue;
                case BinaryOperatorType.Modulus:
                    return leftValue % rightValue;
                case BinaryOperatorType.ShiftLeft:
                    return leftValue << rightValue;
                case BinaryOperatorType.ShiftRight:
                    return leftValue >> rightValue;
                case BinaryOperatorType.NullCoalescing:
                    return leftValue ?? rightValue;
                default:
                    throw new NotImplementedException(
                        "Evaluating binary operator expressions with the operator " + binaryOperatorExpression.Operator + 
                        " is not supported. Affected expression: " + binaryOperatorExpression.ToString());
            }
        }

        public dynamic EvaluateCastExpression(CastExpression castExpression)
        {
            if (!(castExpression.Expression is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Evaluating only cast expressions that target a primitive expression are supported. The targeted expression was: " + 
                    castExpression.Expression.ToString() + ".");
            }

            var toType = castExpression.GetActualTypeReference(true);
            dynamic value = ((PrimitiveExpression)castExpression.Expression).Value;

            switch (toType.FullName)
            {
                case "System.Boolean":
                    return (bool)value;
                case "System.Byte":
                    return (byte)value;
                case "System.Char":
                    return (char)value;
                case "System.Decimal":
                    return (decimal)value;
                case "System.Double":
                    return (double)value;
                case "System.Int16":
                    return (short)value;
                case "System.Int32":
                    return (int)value;
                case "System.Int64":
                    return (long)value;
                case "System.Object":
                    return value;
                case "System.SByte":
                    return (sbyte)value;
                case "System.String":
                    return (string)value;
                case "System.UInt16":
                    return (ushort)value;
                case "System.UInt32":
                    return (uint)value;
                case "System.UInt64":
                    return (ulong)value;
                default:
                    throw new NotSupportedException(
                        "Evaluating casting to " + toType.FullName + " is not supported. Affected expression: " + 
                        castExpression.ToString().AddParentEntityName(castExpression));
            }
        }

        public dynamic EvaluateUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            if (!(unaryOperatorExpression.Expression is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Evaluating only unary expressions that target a primitive expression are supported. The targeted expression was: " +
                    unaryOperatorExpression.Expression.ToString() + ".");
            }

            dynamic value = ((PrimitiveExpression)unaryOperatorExpression.Expression).Value;

            switch (unaryOperatorExpression.Operator)
            {
                //case UnaryOperatorType.Any:
                //    break;
                case UnaryOperatorType.Not:
                    return !value;
                case UnaryOperatorType.BitNot:
                    return ~value;
                case UnaryOperatorType.Minus:
                    return -value;
                case UnaryOperatorType.Plus:
                    return +value;
                case UnaryOperatorType.Increment:
                    return ++value;
                case UnaryOperatorType.Decrement:
                    return --value;
                case UnaryOperatorType.PostIncrement:
                    return value++;
                case UnaryOperatorType.PostDecrement:
                    return value--;
                //case UnaryOperatorType.Dereference:
                //    break;
                //case UnaryOperatorType.AddressOf:
                //    break;
                //case UnaryOperatorType.Await:
                //    break;
                default:
                    throw new NotSupportedException(
                        "Evaluating unary operator expressions with the operator " + unaryOperatorExpression.Operator + 
                        " is not supported. Affected expression: " + unaryOperatorExpression.ToString()
                        .AddParentEntityName(unaryOperatorExpression));
            }
        }
    }
}
