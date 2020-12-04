using Hast.Layer;
using Hast.Synthesis.Abstractions;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class BinaryOperatorExpressionTransformer : IBinaryOperatorExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;

        public BinaryOperatorExpressionTransformer(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer)
        {
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
            ISubTransformerContext context) =>
            TransformBinaryOperatorExpressionInner(
                partiallyTransformedExpression,
                true,
                true,
                true,
                context);

        private IVhdlElement TransformBinaryOperatorExpressionInner(
            IPartiallyTransformedBinaryOperatorExpression partiallyTransformedExpression,
            bool operationResultDataObjectIsVariable,
            bool isFirstOfSimdOperationsOrIsSingleOperation,
            bool isLastOfSimdOperations,
            ISubTransformerContext context)
        {
            var binary = new Binary
            {
                Left = partiallyTransformedExpression.LeftTransformed,
                Right = partiallyTransformedExpression.RightTransformed
            };

            var expression = partiallyTransformedExpression.BinaryOperatorExpression;

            var leftType = expression.Left.GetActualType();
            var rightType = expression.Right.GetActualType();

            // At this point if non-primitive types are checked for equality it could mean that they are custom types
            // either without the equality operator defined or they are custom value types and a ReferenceEquals() is
            // attempted on them which is wrong.
            if (((!leftType.IsPrimitive() || leftType.GetKnownTypeCode() == KnownTypeCode.Object) && leftType.Kind != TypeKind.Enum ||
                (!rightType.IsPrimitive() || rightType.GetKnownTypeCode() == KnownTypeCode.Object) && rightType.Kind != TypeKind.Enum)
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
                    // The % operator in .NET, called modulus in the AST, is in reality a different remainder operator.
                    binary.Operator = BinaryOperator.Remainder;
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
            var resultType = expression.GetResultType();
            var isMultiplication = expression.Operator == BinaryOperatorType.Multiply;

            IType preCastType = null;
            // If the parent is an explicit cast then we need to follow that, otherwise there could be a resize
            // to a smaller type here, then a resize to a bigger type as a result of the cast.
            var hasExplicitCast = firstNonParenthesizedExpressionParent is CastExpression;
            if (hasExplicitCast)
            {
                preCastType = resultType;
                resultType = firstNonParenthesizedExpressionParent.GetActualType();
            }

            var resultVhdlType = _typeConverter.ConvertType(resultType, context.TransformationContext);
            var resultTypeSize = resultVhdlType.GetSize();

            IDataObject operationResultDataObjectReference;
            if (operationResultDataObjectIsVariable)
            {
                operationResultDataObjectReference = stateMachine
                    .CreateVariableWithNextUnusedIndexedName("binaryOperationResult", resultVhdlType)
                    .ToReference();
            }
            else
            {
                operationResultDataObjectReference = stateMachine
                    .CreateSignalWithNextUnusedIndexedName("binaryOperationResult", resultVhdlType)
                    .ToReference();
            }

            IVhdlElement binaryElement = binary;

            var shouldResizeResult =
                (
                    // If the type of the result is the same as the type of the binary expression but the expression is
                    // a multiplication then this means that the result of the operation wouldn't fit into the result
                    // type. This is allowed in .NET (an explicit cast is needed in C# but that will be removed by the
                    // compiler) but will fail in VHDL with something like "[Synth 8-690] width mismatch in assignment;
                    // target has 16 bits, source has 32 bits." In this cases we need to add a type conversion. Also
                    // see the block below.
                    // E.g. ushort = ushort * ushort is valid in IL but in VHDL it must have a length truncation:
                    // unsigned(15 downto 0) = resize(unsigned(15 downto 0) * unsigned(15 downto 0), 16)
                    isMultiplication &&
                    (
                        resultType == expression.GetActualType() ||
                        resultType == leftType && resultType == rightType
                    )
                );

            DataType leftVhdlType = null;
            var leftTypeSize = 0;
            if (leftType != null) // The type reference will be null if e.g. the expression is a PrimitiveExpression.
            {
                leftVhdlType = _typeConverter.ConvertType(leftType, context.TransformationContext);
                leftTypeSize = leftVhdlType.GetSize();
            }
            DataType rightVhdlType = null;
            var rightTypeSize = 0;
            if (rightType != null && rightType.Kind != TypeKind.Null)
            {
                rightVhdlType = _typeConverter.ConvertType(rightType, context.TransformationContext);
                rightTypeSize = rightVhdlType.GetSize();
            }

            if (leftType != null && rightType != null)
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
                        !(resultType == leftType && resultType == rightType)
                    )
                    ||
                    (
                        // If the operand and result sizes don't match.
                        resultTypeSize != 0 && (resultTypeSize != leftTypeSize || resultTypeSize != rightTypeSize)
                    );
            }

            var maxOperandSize = Math.Max(leftTypeSize, rightTypeSize);
            if (maxOperandSize == 0) maxOperandSize = resultTypeSize;

            var deviceDriver = context.TransformationContext.DeviceDriver;
            decimal clockCyclesNeededForOperation;
            var clockCyclesNeededForSignedOperation = deviceDriver
                .GetClockCyclesNeededForBinaryOperation(expression, maxOperandSize, true);
            var clockCyclesNeededForUnsignedOperation = deviceDriver
                .GetClockCyclesNeededForBinaryOperation(expression, maxOperandSize, false);
            if (leftVhdlType != null && rightVhdlType != null && leftVhdlType.Name == rightVhdlType.Name)
            {
                clockCyclesNeededForOperation = leftVhdlType.Name == "signed" ?
                    clockCyclesNeededForSignedOperation :
                    clockCyclesNeededForUnsignedOperation;
            }
            else
            {
                // If the operands have different signs then let's take the slower version just to be safe.
                clockCyclesNeededForOperation = Math.Max(clockCyclesNeededForSignedOperation, clockCyclesNeededForUnsignedOperation);
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
                // Furthermore both shifts will also do a bitwise AND with just 1s on the count, see:
                // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/right-shift-operator
                // How the vacated bits are filled on shifting in either direction is the same (see:
                // https://www.csee.umbc.edu/portal/help/VHDL/numeric_std.vhdl).

                var countSize = leftTypeSize <= 32 ? 5 : 6;
                IVhdlElement resize = ResizeHelper.SmartResize(binary.Right, countSize);

                if (expression.Operator == BinaryOperatorType.ShiftRight)
                {
                    // Since we're already resizing the additional "& 11111" (or "& 111111") might not be needed.
                    // However it's just an identity operation due to the count parameter having the same size. Also,
                    // while this was only added to right shifts .NET actually does the same for left shifts too.
                    // However, it seems to work. Needs further testing to see if it can be removed (it was added in
                    // 21ae34098e48 without anything else being changed and it did fix an issue).
                    resize = new Binary
                    {
                        Left = resize,
                        Operator = BinaryOperator.And,
                        Right =
                            string.Join("", Enumerable.Repeat(1, countSize))
                            .ToVhdlValue(new StdLogicVector { Size = countSize })
                    };

                    var bitwiseAndBinary = new BinaryOperatorExpression(
                        expression.Left.Clone(),
                        BinaryOperatorType.BitwiseAnd,
                        expression.Right.Clone());

                    clockCyclesNeededForOperation += Math.Max(
                        deviceDriver.GetClockCyclesNeededForBinaryOperation(bitwiseAndBinary, maxOperandSize, true),
                        deviceDriver.GetClockCyclesNeededForBinaryOperation(
                            bitwiseAndBinary.Clone<BinaryOperatorExpression>(), maxOperandSize, false));
                }

                binaryElement = new Invocation
                {
                    Target = (expression.Operator == BinaryOperatorType.ShiftLeft ? "shift_left" : "shift_right").ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                    {
                        binary.Left,
                        // The result will be like to_integer(unsigned(SmartResize(..))). The cast to unsigned is
                        // necessary because in .NET the input of the shift is always treated as unsigned. Right shifts
                        // will also have a bitwise AND inside unsigned().
                        Invocation.ToInteger(new Invocation("unsigned", resize))
                    }
                };
            }

            // Shifts also need type conversion if the right operator doesn't have the same type as the left one.
            if (hasExplicitCast || isShift)
            {
                var fromType = isShift && !hasExplicitCast ?
                    leftVhdlType :
                    _typeConverter.ConvertType(preCastType, context.TransformationContext);

                var typeConversionResult = _typeConversionTransformer.ImplementTypeConversion(
                    fromType,
                    resultVhdlType,
                    binaryElement);

                binaryElement = typeConversionResult.ConvertedFromExpression;

                // Most of the time due to the cast no resize is necessary, but sometimes it is.
                shouldResizeResult = shouldResizeResult && !typeConversionResult.IsResized;
            }

            if (shouldResizeResult)
            {
                binaryElement = new Invocation
                {
                    Target = ResizeHelper.SmartResizeName.ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                    {
                        binaryElement,
                        resultTypeSize.ToVhdlValue(KnownDataTypes.UnrangedInt)
                    }
                };
            }

            var operationResultAssignment = new Assignment
            {
                AssignTo = operationResultDataObjectReference,
                Expression = binaryElement
            };

            var operationIsMultiCycle = clockCyclesNeededForOperation > 1;

            if (operationIsMultiCycle &&
                context.TransformationContext.DeviceDriver.DeviceManifest.UsesVivadoInToolChain())
            {
                // We need to add an attribute like the one below so Vivado won't merge this variable/signal with
                // others, thus allowing us to create XDC timing constraints for it.
                // attribute dont_touch of \PrimeCalculator::ArePrimeNumbers(SimpleMemory).0.binaryOperationResult.4\ : variable is "true";

                var attributes = operationResultDataObjectIsVariable ?
                    stateMachine.LocalAttributeSpecifications :
                    stateMachine.GlobalAttributeSpecifications;
                attributes.Add(new AttributeSpecification
                {
                    Attribute = KnownDataTypes.DontTouchAttribute,
                    Expression = new Value { DataType = KnownDataTypes.UnrangedString, Content = "true" },
                    ItemClass = operationResultDataObjectReference.DataObjectKind.ToString(),
                    Of = operationResultDataObjectReference
                });
            }

            // If the current state already takes more than one clock cycle we add a new state and follow up there.
            if (isFirstOfSimdOperationsOrIsSingleOperation && !operationIsMultiCycle)
            {
                stateMachine.AddNewStateAndChangeCurrentBlockIfOverOneClockCycle(context, clockCyclesNeededForOperation);
            }

            // If the operation in itself doesn't take more than one clock cycle then we simply add the operation to the
            // current block, which can be in a new state added previously above.
            if (!operationIsMultiCycle)
            {
                currentBlock.Add(operationResultAssignment);
                if (isFirstOfSimdOperationsOrIsSingleOperation)
                {
                    currentBlock.RequiredClockCycles += clockCyclesNeededForOperation;
                }
            }
            // Since the operation in itself takes more than one clock cycle we need to add a new state just to wait.
            // Then we transition from that state forward to a state where the actual algorithm continues.
            else
            {
                var clockCyclesToWait = (int)Math.Ceiling(clockCyclesNeededForOperation);

                // Building the wait state, just when this is the first transform of multiple SIMD operations (or is a
                // single operation).
                if (isFirstOfSimdOperationsOrIsSingleOperation)
                {
                    var waitedCyclesCountVariable = stateMachine.CreateVariableWithNextUnusedIndexedName(
                        "clockCyclesWaitedForBinaryOperationResult",
                        KnownDataTypes.Int32);
                    var waitedCyclesCountInitialValue = "0".ToVhdlValue(waitedCyclesCountVariable.DataType);
                    waitedCyclesCountVariable.InitialValue = waitedCyclesCountInitialValue;
                    var waitedCyclesCountVariableReference = waitedCyclesCountVariable.ToReference();

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
                stateMachine.RecordMultiCycleOperation(operationResultDataObjectReference, clockCyclesToWait);

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
