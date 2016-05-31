using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Logging;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class TypeConversionTransformer : ITypeConversionTransformer
    {
        private readonly ITypeConverter _typeConverter;

        public ILogger Logger { get; set; }


        public TypeConversionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;

            Logger = NullLogger.Instance;
        }


        public IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            DataObjectReference variableReference,
            bool isLeft)
        {
            // If the type of an operand can't be determined the best guess is the expression's type.
            var expressionTypeReference = binaryOperatorExpression.GetActualTypeReference();
            var expressionType = expressionTypeReference != null ? _typeConverter.ConvertTypeReference(expressionTypeReference) : null;

            var leftTypeReference = binaryOperatorExpression.Left.GetActualTypeReference();
            var rightTypeReference = binaryOperatorExpression.Right.GetActualTypeReference();

            // We won't get a type reference if the expression is a PrimitiveExpression (a constant). In this case we'll
            // assume that the type of the two sides is the same.
            if (binaryOperatorExpression.Left is PrimitiveExpression || binaryOperatorExpression.Right is PrimitiveExpression)
            {
                if (leftTypeReference == null && binaryOperatorExpression.Left is PrimitiveExpression)
                {
                    leftTypeReference = rightTypeReference;
                }
                else
                {
                    rightTypeReference = leftTypeReference;
                }
            }
            // If both of them are PrimitiveExpressions that's something strange (like writing e.g. "if (1 == 3) { ....").
            // Let's assume that then the correct type is that of the expression's.
            else if (binaryOperatorExpression.Left is PrimitiveExpression && binaryOperatorExpression.Right is PrimitiveExpression)
            {
                leftTypeReference = expressionTypeReference;
            }

            var leftType = leftTypeReference != null ? _typeConverter.ConvertTypeReference(leftTypeReference) : expressionType;

            var rightType = rightTypeReference != null ? _typeConverter.ConvertTypeReference(rightTypeReference) : expressionType;

            if (leftType == null || rightType == null)
            {
                throw new InvalidOperationException(
                    "The type of the operands of the following expression could't be determined: " +
                    binaryOperatorExpression.ToString());
            }

            if (leftType == rightType) return variableReference;

            bool convertToLeftType;
            // Is the result type of the expression equal to one of the operands? Then convert the other operand.
            if (expressionTypeReference == leftTypeReference || expressionTypeReference == rightTypeReference)
            {
                convertToLeftType = expressionTypeReference == leftTypeReference;
            }
            // If the result type of the expression is something else (e.g. if the operation is inequality then for two
            // integer operands the result type will be boolean) then convert in a way that's lossless.
            else
            {
                convertToLeftType = ImplementTypeConversion(leftType, rightType, Empty.Instance).IsLossy;
            }

            var fromType = convertToLeftType ? rightType : leftType;
            var toType = convertToLeftType ? leftType : rightType;

            if (isLeft && toType == leftType || !isLeft && toType == rightType)
            {
                return variableReference;
            }

            var typeConversionResult = ImplementTypeConversion(fromType, toType, variableReference);
            if (typeConversionResult.IsLossy)
            {
                Logger.Warning(
                    "Converting from " + fromType.Name +
                    " to " + toType.Name +
                    " to fix a binary expression. Although valid in .NET this could cause information loss due to rounding. " +
                    "The affected expression: " + binaryOperatorExpression.ToString() +
                    " in member " + binaryOperatorExpression.FindFirstParentOfType<EntityDeclaration>().GetFullName() + ".");
            }
            return typeConversionResult.Expression;
        }

        public ITypeConversionResult ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement expression)
        {
            var result = new TypeConversionResult { Expression = expression };

            if (fromType == toType)
            {
                return result;
            }

            Func<DataType, int> getSize = dataType => ((SizedDataType)dataType).Size;

            var fromSize = fromType is SizedDataType ? getSize(fromType) : 0;
            var toSize = toType is SizedDataType ? getSize(toType) : 0;

            var castInvocation = new Invocation();
            castInvocation.Parameters.Add(expression);

            Action<int> convertInvocationToResizeAndAddSizeParameter = size =>
            {
                // Resize is supposed to work with little endian numbers: "When truncating, the sign bit is retained
                // along with the rightmost part." for signed numbers and "When truncating, the leftmost bits are 
                // dropped." for unsigned ones. See: http://www.csee.umbc.edu/portal/help/VHDL/numeric_std.vhdl
                castInvocation.Target = "resize".ToVhdlIdValue();
                castInvocation.Parameters
                    .Add(size.ToVhdlValue(KnownDataTypes.UnrangedInt));
            };

            Action resizeToToSizeIfNeeded = () =>
            {
                // The from type should be resized to fit into the to type.
                if (fromSize != toSize)
                {
                    var resizeInvocation = new Invocation();
                    resizeInvocation.Parameters.Add(castInvocation);
                    castInvocation = resizeInvocation;
                    convertInvocationToResizeAndAddSizeParameter(toSize);
                }
            };

            // Trying supported cast scenarios:

            if ((KnownDataTypes.SignedIntegers.Contains(fromType) && KnownDataTypes.SignedIntegers.Contains(toType)) ||
                KnownDataTypes.UnsignedIntegers.Contains(fromType) && KnownDataTypes.UnsignedIntegers.Contains(toType))
            {
                // Casting to a smaller type, so we need to cut off bits.
                if (getSize(fromType) > toSize)
                {
                    result.IsLossy = true;
                    convertInvocationToResizeAndAddSizeParameter(toSize);
                }
                else return result;
            }
            else if (KnownDataTypes.Integers.Contains(fromType) && toType == KnownDataTypes.Real)
            {
                castInvocation.Target = "real".ToVhdlIdValue();
            }
            else if (KnownDataTypes.UnsignedIntegers.Contains(fromType) && KnownDataTypes.SignedIntegers.Contains(toType))
            {
                // If the full scale of the uint wouldn't fit.
                result.IsLossy = fromSize > toSize / 2;

                castInvocation.Target = "signed".ToVhdlIdValue();
                resizeToToSizeIfNeeded();
            }
            else if (KnownDataTypes.SignedIntegers.Contains(fromType) && KnownDataTypes.UnsignedIntegers.Contains(toType))
            {
                result.IsLossy = true;
                castInvocation.Target = "unsigned".ToVhdlIdValue();
                resizeToToSizeIfNeeded();
            }
            else if (KnownDataTypes.Integers.Contains(fromType) && toType == KnownDataTypes.UnrangedInt)
            {
                result.IsLossy = true;
                castInvocation.Target = "to_integer".ToVhdlIdValue();
            }

            if (fromType == KnownDataTypes.StdLogicVector32)
            {
                if (KnownDataTypes.SignedIntegers.Contains(toType))
                {
                    castInvocation.Target = "signed".ToVhdlIdValue();
                }
                else if (KnownDataTypes.UnsignedIntegers.Contains(toType))
                {
                    castInvocation.Target = "unsigned".ToVhdlIdValue();
                }
            }
            if (toType == KnownDataTypes.StdLogicVector32)
            {
                castInvocation.Target = "std_logic_vector".ToVhdlIdValue();
            }


            if (castInvocation.Target == null)
            {
                throw new NotSupportedException("Casting from " + fromType.Name + " to " + toType.Name + " is not supported.");
            }


            result.Expression = castInvocation;
            return result;
        }


        private class TypeConversionResult : ITypeConversionResult
        {
            public IVhdlElement Expression { get; set; }
            public bool IsLossy { get; set; }
        }
    }
}
