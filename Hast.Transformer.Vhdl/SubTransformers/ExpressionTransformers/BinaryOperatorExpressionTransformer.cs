using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Synthesis.Services;
using Hast.Transformer.Helpers;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class BinaryOperatorExpressionTransformer : IBinaryOperatorExpressionTransformer
    {
        private readonly IDeviceDriverSelector _deviceDriverSelector;
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;


        public BinaryOperatorExpressionTransformer(
            IDeviceDriverSelector deviceDriverSelector,
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer)
        {
            _deviceDriverSelector = deviceDriverSelector;
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
        }


        public IEnumerable<IVhdlElement> TransformParallelBinaryOperatorExpressions(
              IEnumerable<IPartiallyTransformedBinaryOperatorExpression> partiallyTransformedExpressions,
              ISubTransformerContext context)
        {
            var resultReferences = new List<IVhdlElement>();

            var partiallyTransformedExpressionsList = partiallyTransformedExpressions.ToList();

            resultReferences.Add(TransformBinaryOperatorExpressionInner(
                partiallyTransformedExpressionsList[0],
                false,
                true,
                false,
                context));

            for (int i = 1; i < partiallyTransformedExpressionsList.Count - 1; i++)
            {
                resultReferences.Add(TransformBinaryOperatorExpressionInner(
                    partiallyTransformedExpressionsList[i],
                    false,
                    false,
                    false,
                    context));
            }

            resultReferences.Add(TransformBinaryOperatorExpressionInner(
                partiallyTransformedExpressionsList[partiallyTransformedExpressionsList.Count - 1],
                false,
                false,
                true,
                context));

            return resultReferences;
        }

        public IVhdlElement TransformBinaryOperatorExpression(
            IPartiallyTransformedBinaryOperatorExpression partiallyTransformedExpression,
            ISubTransformerContext context)
        {
            return TransformBinaryOperatorExpressionInner(
                partiallyTransformedExpression,
                true,
                true,
                true,
                context);
        }


        private IVhdlElement TransformBinaryOperatorExpressionInner(
            IPartiallyTransformedBinaryOperatorExpression partiallyTransformedExpression,
            bool operationResultDataObjectIsVariable,
            bool isFirstOfSimdOperations,
            bool isLastOfSimdOperations,
            ISubTransformerContext context)
        {
            var binary = new Binary
            {
                Left = partiallyTransformedExpression.LeftTransformed,
                Right = partiallyTransformedExpression.RightTransformed
            };

            var expression = partiallyTransformedExpression.BinaryOperatorExpression;

            var leftTypeReference = expression.Left.GetActualTypeReference();
            var rightTypeReference = expression.Right.GetActualTypeReference();

            // At this point if non-primitive types are checked for equality it could mean that they are custom 
            // types either without the equality operator defined or they are custom value types and a
            // ReferenceEquals() is attempted on them which is wrong.
            if ((leftTypeReference != null &&
                        !leftTypeReference.IsPrimitive &&
                        (leftTypeReference as TypeDefinition)?.IsEnum != true ||
                    rightTypeReference != null &&
                        !rightTypeReference.IsPrimitive &&
                        (rightTypeReference as TypeDefinition)?.IsEnum != true)
                &&
                !(expression.Left is NullReferenceExpression || expression.Right is NullReferenceExpression))
            {
                throw new InvalidOperationException(
                    "Unsupported operator in the following binary operator expression: " + expression.ToString() +
                    ". This could mean that you attempted to use an operator on custom types either without the operator being defined for the type or they are custom value types and you mistakenly tried to use ReferenceEquals() on them."
                    .AddParentEntityName(expression));
            }

            // The commented out cases with unsupported operators are here so adding them later is easier.
            switch (expression.Operator)
            {
                case BinaryOperatorType.Add:
                    binary.Operator = BinaryOperator.Add;
                    break;
                //case BinaryOperatorType.Any:
                //    break;
                case BinaryOperatorType.BitwiseAnd:
                case BinaryOperatorType.ConditionalAnd:
                    binary.Operator = BinaryOperator.And;
                    break;
                case BinaryOperatorType.BitwiseOr:
                case BinaryOperatorType.ConditionalOr:
                    binary.Operator = BinaryOperator.Or;
                    break;
                case BinaryOperatorType.Divide:
                    binary.Operator = BinaryOperator.Divide;
                    break;
                case BinaryOperatorType.Equality:
                    binary.Operator = BinaryOperator.Equality;
                    break;
                case BinaryOperatorType.ExclusiveOr:
                    binary.Operator = BinaryOperator.ExclusiveOr;
                    break;
                case BinaryOperatorType.GreaterThan:
                    binary.Operator = BinaryOperator.GreaterThan;
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    binary.Operator = BinaryOperator.GreaterThanOrEqual;
                    break;
                case BinaryOperatorType.InEquality:
                    binary.Operator = BinaryOperator.InEquality;
                    break;
                case BinaryOperatorType.LessThan:
                    binary.Operator = BinaryOperator.LessThan;
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    binary.Operator = BinaryOperator.LessThanOrEqual;
                    break;
                case BinaryOperatorType.Modulus:
                    binary.Operator = BinaryOperator.Modulus;
                    break;
                case BinaryOperatorType.Multiply:
                    binary.Operator = BinaryOperator.Multiply;
                    break;
                //case BinaryOperatorType.NullCoalescing:
                //    break;
                // Left and right shift for numerical types is a function call in VHDL, so handled separately. See
                // below. The sll/srl or sra/sla operators shouldn't be used, see: 
                // https://www.nandland.com/vhdl/examples/example-shifts.html and https://stackoverflow.com/questions/9018087/shift-a-std-logic-vector-of-n-bit-to-right-or-left
                case BinaryOperatorType.ShiftLeft:
                case BinaryOperatorType.ShiftRight:
                    break;
                case BinaryOperatorType.Subtract:
                    binary.Operator = BinaryOperator.Subtract;
                    break;
                default:
                    throw new NotImplementedException("Binary operator " + expression.Operator + " is not supported.");
            }


            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

            var firstNonParenthesizedExpressionParent = expression.FindFirstNonParenthesizedExpressionParent();
            var resultTypeReference = expression.GetResultTypeReference();
            var isMultiplication = expression.Operator == BinaryOperatorType.Multiply;

            // Is the result type smaller than it should be? It seems that (u)short * (u)short which results in an int 
            // in .NET can be (u)short in the AST, if it's adjacent to another binary operation (maybe just division).
            // The same is with (s)byte. This is wrong and we need to work around that.
            Predicate<TypeReference> isAnyShort = typeReference =>
                typeReference.FullName == typeof(ushort).FullName || typeReference.FullName == typeof(short).FullName;
            Predicate<TypeReference> isAnyByte = typeReference =>
                typeReference.FullName == typeof(byte).FullName || typeReference.FullName == typeof(sbyte).FullName;
            var resultNeedsForcedIntCast =
                isMultiplication &&
                (isAnyShort(resultTypeReference) && (isAnyShort(leftTypeReference) || isAnyShort(rightTypeReference))) ||
                (isAnyByte(resultTypeReference) && (isAnyByte(leftTypeReference) || isAnyByte(rightTypeReference)));
            var forcedResultIntCastIsSigned = false;
            if (resultNeedsForcedIntCast)
            {
                forcedResultIntCastIsSigned =
                    new[] { typeof(short).FullName, typeof(sbyte).FullName }.Contains(resultTypeReference.FullName);
                var intTypeInformation = forcedResultIntCastIsSigned ?
                    TypeHelper.CreateInt32TypeInformation() :
                    TypeHelper.CreateUInt32TypeInformation();

                // Not nice, but the node needs to be modified too so anything else will see the same type as well.
                expression.RemoveAnnotations<TypeInformation>();
                expression.AddAnnotation(intTypeInformation);

                resultTypeReference = intTypeInformation.ExpectedType;
            }

            TypeReference preCastTypeReference = null;
            // If the parent is an explicit cast then we need to follow that, otherwise there could be a resize
            // to a smaller type here, then a resize to a bigger type as a result of the cast.
            if (firstNonParenthesizedExpressionParent is CastExpression)
            {
                preCastTypeReference = resultTypeReference;
                resultTypeReference = firstNonParenthesizedExpressionParent.GetActualTypeReference(true);
            }

            var resultType = _typeConverter.ConvertTypeReference(resultTypeReference, context.TransformationContext);
            var resultTypeSize = resultType.GetSize();



            IDataObject operationResultDataObjectReference;
            if (operationResultDataObjectIsVariable)
            {
                operationResultDataObjectReference = stateMachine
                    .CreateVariableWithNextUnusedIndexedName("binaryOperationResult", resultType)
                    .ToReference();
            }
            else
            {
                operationResultDataObjectReference = stateMachine
                    .CreateSignalWithNextUnusedIndexedName("binaryOperationResult", resultType)
                    .ToReference();
            }

            IVhdlElement binaryElement = binary;

            var shouldResizeResult =
                (
                    // If the type of the result is the same as the type of the binary expression but the expression is a
                    // multiplication then this means that the result of the operation wouldn't fit into the result type.
                    // This is allowed in .NET (an explicit cast is needed in C# but that will be removed by the compiler)
                    // but will fail in VHDL with something like "[Synth 8-690] width mismatch in assignment; target has 
                    // 16 bits, source has 32 bits." In this cases we need to add a type conversion. Also see the block
                    // below.
                    // E.g. ushort = ushort * ushort is valid in IL but in VHDL it must have a length truncation:
                    // unsigned(15 downto 0) = resize(unsigned(15 downto 0) * unsigned(15 downto 0), 16)
                    isMultiplication &&
                    (
                        resultTypeReference == expression.GetActualTypeReference() ||
                        resultTypeReference == leftTypeReference && resultTypeReference == rightTypeReference
                    )
                );

            DataType leftType = null;
            var leftTypeSize = 0;
            if (leftTypeReference != null) // The type reference will be null if e.g. the expression is a PrimitiveExpression.
            {
                leftType = _typeConverter.ConvertTypeReference(leftTypeReference, context.TransformationContext);
                leftTypeSize = leftType.GetSize();
            }
            DataType rightType = null;
            var rightTypeSize = 0;
            if (rightTypeReference != null)
            {
                rightType = _typeConverter.ConvertTypeReference(rightTypeReference, context.TransformationContext);
                rightTypeSize = rightType.GetSize();
            }

            if (leftTypeReference != null && rightTypeReference != null)
            {
                shouldResizeResult = shouldResizeResult ||
                    (
                        // If the operands and the result have the same size then the result won't fit.
                        isMultiplication &&
                        resultTypeSize != 0 &&
                        resultTypeSize == leftTypeSize &&
                        resultTypeSize == rightTypeSize
                    )
                    ||
                    (
                        // If the operation is an addition and the types of the result and the operands differ then we 
                        // also have to resize.
                        expression.Operator == BinaryOperatorType.Add &&
                        !(resultTypeReference == leftTypeReference && resultTypeReference == rightTypeReference)
                    )
                    ||
                    (
                        // If the operand and result sizes don't match.
                        resultTypeSize != 0 && (resultTypeSize != leftTypeSize || resultTypeSize != rightTypeSize)
                    );
            }

            var isShift = false;
            if (expression.Operator == BinaryOperatorType.ShiftLeft || expression.Operator == BinaryOperatorType.ShiftRight)
            {
                isShift = true;

                // Contrary to what happens in VHDL binary shifting in .NET will only use the lower 5 bits (for 32b
                // operands) or 6 bits (for 64b operands) of the shift count. So e.g. 1 << 33 won't produce 0 (by
                // shifting out to the void) but 2, since only a shift by 1 happens (as 33 is 100001 in binary).
                // See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/left-shift-operator
                // So we need to truncate.
                binaryElement = new Invocation
                {
                    Target = (expression.Operator == BinaryOperatorType.ShiftLeft ? "shift_left" : "shift_right").ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                    {
                        binary.Left,
                        Invocation.ToInteger(Invocation.SmartResize(binary.Right, leftTypeSize <= 32 ? 5 : 6))
                    }
                };
            }

            // Shifts also need type conversion if the right operator doesn't have the same type as the left one.
            if (firstNonParenthesizedExpressionParent is CastExpression || isShift)
            {
                var fromType = isShift && !(firstNonParenthesizedExpressionParent is CastExpression) ?
                    leftType :
                    _typeConverter.ConvertTypeReference(preCastTypeReference, context.TransformationContext);

                var typeConversionResult = _typeConversionTransformer.ImplementTypeConversion(
                    fromType,
                    resultType,
                    binaryElement);

                binaryElement = typeConversionResult.ConvertedFromExpression;

                // Most of the time due to the cast no resize is necessary, but sometimes it is.
                shouldResizeResult = shouldResizeResult && !typeConversionResult.IsResized;
                resultNeedsForcedIntCast = false;
            }

            if (shouldResizeResult)
            {
                binaryElement = new Invocation
                {
                    Target = "resize".ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                        {
                            { binaryElement },
                            { resultTypeSize.ToVhdlValue(KnownDataTypes.UnrangedInt) }
                        }
                };
            }

            if (resultNeedsForcedIntCast)
            {
                // This needs to be after resize() because otherwise casting an unsigned to signed can result in data
                // loss due to the range change. 
                binaryElement = new Invocation
                {
                    Target = (forcedResultIntCastIsSigned ? "signed" : "unsigned").ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement> { { binaryElement } }
                };
            }

            var operationResultAssignment = new Assignment
            {
                AssignTo = operationResultDataObjectReference,
                Expression = binaryElement
            };

            var maxOperandSize = Math.Max(leftTypeSize, rightTypeSize);
            if (maxOperandSize == 0) maxOperandSize = resultTypeSize;

            var deviceDriver = _deviceDriverSelector.GetDriver(context);
            decimal clockCyclesNeededForOperation;
            var clockCyclesNeededForSignedOperation = deviceDriver
                .GetClockCyclesNeededForBinaryOperation(expression, maxOperandSize, true);
            var clockCyclesNeededForUnsignedOperation = deviceDriver
                .GetClockCyclesNeededForBinaryOperation(expression, maxOperandSize, false);
            if (leftType != null && rightType != null && leftType.Name == rightType.Name)
            {
                clockCyclesNeededForOperation = leftType.Name == "signed" ?
                    clockCyclesNeededForSignedOperation :
                    clockCyclesNeededForUnsignedOperation;
            }
            else
            {
                // If the operands have different signs then let's take the slower version just to be safe.
                clockCyclesNeededForOperation = Math.Max(clockCyclesNeededForSignedOperation, clockCyclesNeededForUnsignedOperation);
            }

            var operationIsMultiCycle = clockCyclesNeededForOperation > 1;

            // If the current state takes more than one clock cycle we add a new state and follow up there.
            if (isFirstOfSimdOperations && !operationIsMultiCycle)
            {
                stateMachine.AddNewStateAndChangeCurrentBlockIfOverOneClockCycle(context, clockCyclesNeededForOperation);
            }

            // If the operation in itself doesn't take more than one clock cycle then we simply add the operation to the
            // current block, which can be in a new state added previously above.
            if (!operationIsMultiCycle)
            {
                currentBlock.Add(operationResultAssignment);
                if (isFirstOfSimdOperations)
                {
                    currentBlock.RequiredClockCycles += clockCyclesNeededForOperation;
                }
            }
            // Since the operation in itself takes more than one clock cycle we need to add a new state just to wait.
            // Then we transition from that state forward to a state where the actual algorithm continues.
            else
            {
                // Building the wait state, just when this is the first transform of multiple SIMD operations (or is a
                // single operation).
                if (isFirstOfSimdOperations)
                {
                    var waitedCyclesCountVariable = stateMachine.CreateVariableWithNextUnusedIndexedName(
                        "clockCyclesWaitedForBinaryOperationResult",
                        KnownDataTypes.Int32);
                    var waitedCyclesCountInitialValue = "0".ToVhdlValue(waitedCyclesCountVariable.DataType);
                    waitedCyclesCountVariable.InitialValue = waitedCyclesCountInitialValue;
                    var waitedCyclesCountVariableReference = waitedCyclesCountVariable.ToReference();

                    var clockCyclesToWait = (int)Math.Ceiling(clockCyclesNeededForOperation);

                    var waitForResultBlock = new InlineBlock(
                        new GeneratedComment(vhdlGenerationOptions =>
                            "Waiting for the result to appear in " +
                            operationResultDataObjectReference.ToVhdl(vhdlGenerationOptions) +
                            " (have to wait " + clockCyclesToWait + " clock cycles in this state)."),
                        new LineComment(
                        "The assignment needs to be kept up for multi-cycle operations for the result to actually appear in the target."));

                    var waitForResultIf = new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = waitedCyclesCountVariableReference,
                            Operator = BinaryOperator.GreaterThanOrEqual,
                            Right = clockCyclesToWait.ToVhdlValue(waitedCyclesCountVariable.DataType)
                        }
                    };
                    waitForResultBlock.Add(waitForResultIf);

                    var waitForResultStateIndex = stateMachine.AddNewStateAndChangeCurrentBlock(context, waitForResultBlock);
                    stateMachine.States[waitForResultStateIndex].RequiredClockCycles = clockCyclesToWait;

                    var afterResultReceivedBlock = new InlineBlock();
                    var afterResultReceivedStateIndex = stateMachine.AddState(afterResultReceivedBlock);
                    waitForResultIf.True = new InlineBlock(
                        stateMachine.CreateStateChange(afterResultReceivedStateIndex),
                        new Assignment { AssignTo = waitedCyclesCountVariableReference, Expression = waitedCyclesCountInitialValue });
                    waitForResultIf.Else = new Assignment
                    {
                        AssignTo = waitedCyclesCountVariableReference,
                        Expression = new Binary
                        {
                            Left = waitedCyclesCountVariableReference,
                            Operator = BinaryOperator.Add,
                            Right = "1".ToVhdlValue(waitedCyclesCountVariable.DataType)
                        }
                    };
                }


                currentBlock.Add(operationResultAssignment);


                // Changing the current block to the one in the state after the wait state, just when this is the last
                // transform of multiple SIMD operations (or is a single operation).
                if (isLastOfSimdOperations)
                {
                    // It should be the last state added above.
                    currentBlock.ChangeBlockToDifferentState(stateMachine.States.Last().Body, stateMachine.States.Count - 1);
                }
            }

            return operationResultDataObjectReference;
        }
    }
}
