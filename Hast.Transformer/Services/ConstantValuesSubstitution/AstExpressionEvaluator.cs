using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;

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

            return binaryOperatorExpression.Operator switch
            {
                BinaryOperatorType.BitwiseAnd => leftValue & rightValue,
                BinaryOperatorType.BitwiseOr => leftValue | rightValue,
                BinaryOperatorType.ConditionalAnd => leftValue && rightValue,
                BinaryOperatorType.ConditionalOr => leftValue || rightValue,
                BinaryOperatorType.ExclusiveOr => leftValue ^ rightValue,
                BinaryOperatorType.GreaterThan => leftValue > rightValue,
                BinaryOperatorType.GreaterThanOrEqual => leftValue >= rightValue,
                BinaryOperatorType.Equality => leftValue.Equals(rightValue),
                BinaryOperatorType.InEquality => !leftValue.Equals(rightValue),
                BinaryOperatorType.LessThan => leftValue < rightValue,
                BinaryOperatorType.LessThanOrEqual => leftValue <= rightValue,
                BinaryOperatorType.Add => leftValue + rightValue,
                BinaryOperatorType.Subtract => leftValue - rightValue,
                BinaryOperatorType.Multiply => leftValue * rightValue,
                BinaryOperatorType.Divide => leftValue / rightValue,
                BinaryOperatorType.Modulus => leftValue % rightValue,
                BinaryOperatorType.ShiftLeft => leftValue << rightValue,
                BinaryOperatorType.ShiftRight => leftValue >> rightValue,
                BinaryOperatorType.NullCoalescing => leftValue ?? rightValue,
                _ => throw new NotSupportedException(
                    "Evaluating binary operator expressions with the operator " + binaryOperatorExpression.Operator +
                    " is not supported. Affected expression: " + binaryOperatorExpression),
            };
        }

        public dynamic EvaluateCastExpression(CastExpression castExpression)
        {
            if (!(castExpression.Expression is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Evaluating only cast expressions that target a primitive expression are supported. The targeted expression was: " +
                    castExpression.Expression.ToString() + ".");
            }

            var toType = castExpression.GetActualType();
            dynamic value = ((PrimitiveExpression)castExpression.Expression).Value;

            switch (toType.GetFullName())
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
                        "Evaluating casting to " + toType.GetFullName() + " is not supported. Affected expression: " +
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
                ////case UnaryOperatorType.Any:
                ////    break;
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
                default:
                    throw new NotSupportedException(
                        "Evaluating unary operator expressions with the operator " + unaryOperatorExpression.Operator +
                        " is not supported. Affected expression: " + unaryOperatorExpression.ToString()
                        .AddParentEntityName(unaryOperatorExpression));
            }
        }
    }
}
