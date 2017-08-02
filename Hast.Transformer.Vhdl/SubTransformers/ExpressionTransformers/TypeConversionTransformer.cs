﻿using System;
using System.Collections.Generic;
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
            var result = new TypeConversionResult();

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
                else
                {
                    result.ConvertedFromExpression = fromExpression;
                }

                return result;
            }

            Func<DataType, int> getSize = dataType => ((SizedDataType)dataType).Size;

            var fromSize = fromType.GetSize();
            var toSize = toType.GetSize();



            Func<string, IVhdlElement, Invocation> createCastInvocation = (target, expression) =>
                new Invocation
                {
                    Target = target.ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement> { { expression } }
                };

            Func<string, Invocation> createCastInvocationForFromExpression = target => 
                createCastInvocation(target, fromExpression);

            Func<IVhdlElement, IVhdlElement> createResizeExpression = parameter =>
            {
                result.IsResized = true;

                // There needs to be some decision logic on size in SmartResize() because sometimes the sizes in VHDL
                // won't be the same as in .NET due to type handling.
                return Invocation.SmartResize(parameter, toSize);
            };


            // Trying supported cast scenarios:

            if ((KnownDataTypes.SignedIntegers.Contains(fromType) && KnownDataTypes.SignedIntegers.Contains(toType)) ||
                KnownDataTypes.UnsignedIntegers.Contains(fromType) && KnownDataTypes.UnsignedIntegers.Contains(toType))
            {
                if (fromSize == toSize)
                {
                    result.ConvertedFromExpression = fromExpression;
                    return result;
                }

                // Casting to a smaller type, so we need to cut off bits. Casting to a bigger type is not lossy but
                // still needs resize.
                if (fromSize > toSize)
                {
                    result.IsLossy = true;
                }

                result.ConvertedFromExpression = createResizeExpression(fromExpression);
            }
            else if (KnownDataTypes.Integers.Contains(fromType) && toType == KnownDataTypes.Real)
            {
                result.ConvertedFromExpression = createCastInvocationForFromExpression("real");
            }
            else if (fromType == KnownDataTypes.Real && KnownDataTypes.Integers.Contains(toType))
            {
                result.ConvertedFromExpression = createCastInvocationForFromExpression("integer");
            }
            else if (KnownDataTypes.UnsignedIntegers.Contains(fromType) && KnownDataTypes.SignedIntegers.Contains(toType))
            {
                // If the full scale of the uint wouldn't fit.
                result.IsLossy = fromSize > toSize / 2;

                var expression = fromExpression;

                // Resizing needs to happen before signed() otherwise casting an unsigned to signed can result in data 
                // loss due to the range change. 
                if (fromSize != toSize)
                {
                    expression = createResizeExpression(fromExpression);
                }

                result.ConvertedFromExpression = createCastInvocation("signed", expression);
            }
            else if (KnownDataTypes.SignedIntegers.Contains(fromType) && KnownDataTypes.UnsignedIntegers.Contains(toType))
            {
                result.IsLossy = true;

                if (fromSize >= toSize)
                {
                    result.ConvertedFromExpression = createCastInvocationForFromExpression("unsigned");

                    if (fromSize != toSize)
                    {
                        result.ConvertedFromExpression = createResizeExpression(result.ConvertedFromExpression);
                    }
                }
                else
                {
                    var expandInvocation = createCastInvocationForFromExpression("ToUnsignedAndExpand");
                    expandInvocation.Parameters.Add(toSize.ToVhdlValue(KnownDataTypes.UnrangedInt));
                    result.ConvertedFromExpression = expandInvocation;
                }
            }
            else if (KnownDataTypes.Integers.Contains(fromType) && toType == KnownDataTypes.UnrangedInt)
            {
                result.IsLossy = true;
                result.ConvertedFromExpression = createCastInvocationForFromExpression("to_integer");
            }

            if (fromType == KnownDataTypes.StdLogicVector32)
            {
                if (KnownDataTypes.SignedIntegers.Contains(toType))
                {
                    result.ConvertedFromExpression = createCastInvocationForFromExpression("signed");
                }
                else if (KnownDataTypes.UnsignedIntegers.Contains(toType))
                {
                    result.ConvertedFromExpression = createCastInvocationForFromExpression("unsigned");
                }

                result.IsLossy = toSize > 32;
            }
            if (toType == KnownDataTypes.StdLogicVector32)
            {
                result.ConvertedFromExpression = createCastInvocationForFromExpression("std_logic_vector");
                result.IsLossy = fromSize > 32;
            }


            if (result.ConvertedFromExpression == null)
            {
                throw new NotSupportedException(
                    "Casting from " + fromType.Name + " to " + toType.Name +
                    " is not supported. Transformed expression to be cast: " + fromExpression.ToVhdl());
            }


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
