﻿using System;
using System.Linq;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Logging;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class TypeConversionTransformer : ITypeConversionTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public TypeConversionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            DataObjectReference variableReference,
            bool isLeft,
            ISubTransformerContext context)
        {
            // If the type of an operand can't be determined the best guess is the expression's type.
            var expressionTypeReference = binaryOperatorExpression.GetActualTypeReference();
            var expressionType = expressionTypeReference != null ? 
                _typeConverter.ConvertTypeReference(expressionTypeReference, context.TransformationContext) : 
                null;

            var leftTypeReference = binaryOperatorExpression.Left.GetActualTypeReference();
            var rightTypeReference = binaryOperatorExpression.Right.GetActualTypeReference();

            // If this some null check then no need for any type conversion.
            if (binaryOperatorExpression.Left is NullReferenceExpression || binaryOperatorExpression.Right is NullReferenceExpression)
            {
                return variableReference;
            }

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

            var leftType = leftTypeReference != null ? 
                _typeConverter.ConvertTypeReference(leftTypeReference, context.TransformationContext) : 
                expressionType;

            var rightType = rightTypeReference != null ? 
                _typeConverter.ConvertTypeReference(rightTypeReference, context.TransformationContext) : 
                expressionType;

            if (leftType == null || rightType == null)
            {
                throw new InvalidOperationException(
                    "The type of the operands of the following expression couldn't be determined: " +
                    binaryOperatorExpression.ToString().AddParentEntityName(binaryOperatorExpression));
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
                context.Scope.Warnings.AddWarning(
                    "LossyBinaryExpressionCast",
                    "Converting from " + fromType.Name +
                    " to " + toType.Name +
                    " to fix a binary expression. Although valid in .NET this could cause information loss due to rounding. " +
                    "The affected expression: " + binaryOperatorExpression.ToString() +
                    " in member " + binaryOperatorExpression.FindFirstParentOfType<EntityDeclaration>().GetFullName() + ".");
            }
            return typeConversionResult.ConvertedFromExpression;
        }

        public IAssignmentTypeConversionResult ImplementTypeConversionForAssignment(
            DataType fromType,
            DataType toType,
            IVhdlElement fromExpression,
            IDataObject toDataObject)
        {
            var subResult = ImplementTypeConversion(fromType, toType, fromExpression);

            var result = new AssignmentTypeConversionResult
            {
                ConvertedFromExpression = subResult.ConvertedFromExpression,
                ConvertedToDataObject = toDataObject,
                IsLossy = subResult.IsLossy,
                IsResized = subResult.IsResized
            };

            // If both types are arrays then if the array size is different slicing is needed.
            var fromArray = fromType as UnconstrainedArrayInstantiation;
            var toArray = toType as UnconstrainedArrayInstantiation;
            if (fromArray != null && toArray != null && fromArray.RangeTo < toArray.RangeTo)
            {
                result.ConvertedToDataObject = new ArraySlice
                {
                    ArrayReference = toDataObject,
                    IndexFrom = 0,
                    IndexTo = fromArray.RangeTo
                };
                result.IsResized = true;
            }

            return result;
        }

        public ITypeConversionResult ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement fromExpression)
        {
            var result = new TypeConversionResult { ConvertedFromExpression = fromExpression };

            if (fromType == toType)
            {
                // If both types are arrays then if the array size is different slicing is needed.
                var fromArray = fromType as UnconstrainedArrayInstantiation;
                var toArray = toType as UnconstrainedArrayInstantiation;
                if (fromArray != null && toArray != null && fromArray.RangeTo > toArray.RangeTo)
                {
                    result.ConvertedFromExpression = new ArraySlice
                    {
                        ArrayReference = (IDataObject)fromExpression,
                        IndexFrom = 0,
                        IndexTo = toArray.RangeTo
                    };
                    result.IsResized = true;
                }

                return result;
            }

            Func<DataType, int> getSize = dataType => ((SizedDataType)dataType).Size;

            var fromSize = fromType.GetSize();
            var toSize = toType.GetSize();

            var castInvocation = new Invocation();
            castInvocation.Parameters.Add(fromExpression);

            Action<int> convertInvocationToResizeAndAddSizeParameter = size =>
            {
                // Resize is supposed to work with little endian numbers: "When truncating, the sign bit is retained
                // along with the rightmost part." for signed numbers and "When truncating, the leftmost bits are 
                // dropped." for unsigned ones. See: http://www.csee.umbc.edu/portal/help/VHDL/numeric_std.vhdl
                castInvocation.Target = "resize".ToVhdlIdValue();
                castInvocation.Parameters
                    .Add(size.ToVhdlValue(KnownDataTypes.UnrangedInt));
                result.IsResized = true;
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
                if (fromSize == toSize) return result;

                // Casting to a smaller type, so we need to cut off bits. Casting to a bigger type is not lossy but
                // still needs resize.
                if (fromSize > toSize)
                {
                    result.IsLossy = true;
                }

                convertInvocationToResizeAndAddSizeParameter(toSize);
            }
            else if (KnownDataTypes.Integers.Contains(fromType) && toType == KnownDataTypes.Real)
            {
                castInvocation.Target = "real".ToVhdlIdValue();
            }
            else if (fromType == KnownDataTypes.Real &&KnownDataTypes.Integers.Contains(toType))
            {
                castInvocation.Target = "integer".ToVhdlIdValue();
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

                result.IsLossy = toSize > 32;
            }
            if (toType == KnownDataTypes.StdLogicVector32)
            {
                castInvocation.Target = "std_logic_vector".ToVhdlIdValue();
                result.IsLossy = fromSize > 32;
            }


            if (castInvocation.Target == null)
            {
                throw new NotSupportedException(
                    "Casting from " + fromType.Name + " to " + toType.Name + 
                    " is not supported. Transformed expression to be cast: " + fromExpression.ToVhdl());
            }


            result.ConvertedFromExpression = castInvocation;
            return result;
        }


        private class TypeConversionResult : ITypeConversionResult
        {
            public IVhdlElement ConvertedFromExpression { get; set; }
            public bool IsLossy { get; set; }
            public bool IsResized { get; set; }
        }

        private class AssignmentTypeConversionResult : TypeConversionResult, IAssignmentTypeConversionResult
        {
            public IDataObject ConvertedToDataObject { get; set; }
        }
    }
}
