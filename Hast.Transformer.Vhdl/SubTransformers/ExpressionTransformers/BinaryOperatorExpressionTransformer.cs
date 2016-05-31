using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class BinaryOperatorExpressionTransformer : IBinaryOperatorExpressionTransformer
    {
        private readonly IDeviceDriver _deviceDriver;
        private readonly ITypeConverter _typeConverter;


        public BinaryOperatorExpressionTransformer(IDeviceDriver deviceDriver, ITypeConverter typeConverter)
        {
            _deviceDriver = deviceDriver;
            _typeConverter = typeConverter;
        }


        public IVhdlElement TransformBinaryOperatorExpression(
            BinaryOperatorExpression expression,
            IVhdlElement leftTransformed,
            IVhdlElement rightTransformed,
            ISubTransformerContext context)
        {
            var binary = new Binary { Left = leftTransformed, Right = rightTransformed };

            // Would need to decide between + and & or sll/srl and sra/sla
            // See: http://www.csee.umbc.edu/portal/help/VHDL/operator.html
            // The commented out cases with unsupported operators are here so adding them later is easier.
            switch (expression.Operator)
            {
                case BinaryOperatorType.Add:
                    binary.Operator = BinaryOperator.Add;
                    break;
                //case BinaryOperatorType.Any:
                //    break;
                //case BinaryOperatorType.BitwiseAnd:
                //    break;
                //case BinaryOperatorType.BitwiseOr:
                //    break;
                case BinaryOperatorType.ConditionalAnd:
                    binary.Operator = BinaryOperator.ConditionalAnd;
                    break;
                case BinaryOperatorType.ConditionalOr:
                    binary.Operator = BinaryOperator.ConditionalOr;
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
                case BinaryOperatorType.ShiftLeft:
                    binary.Operator = BinaryOperator.ShiftLeft;
                    break;
                case BinaryOperatorType.ShiftRight:
                    binary.Operator = BinaryOperator.ShiftRight;
                    break;
                case BinaryOperatorType.Subtract:
                    binary.Operator = BinaryOperator.Subtract;
                    break;
                default:
                    throw new NotImplementedException("Support for the binary operator " + expression.Operator + " is not implemented.");
            }


            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

            var clockCyclesNeededForOperation = _deviceDriver.GetClockCyclesNeededForBinaryOperation(expression);
            var operationIsMultiCycle = clockCyclesNeededForOperation > 1;

            var resultTypeReference = expression.GetActualTypeReference(true);
            if (resultTypeReference == null)
            {
                resultTypeReference = expression.FindFirstNonParenthesizedExpressionParent().GetActualTypeReference();
            }
            var resultType = _typeConverter.ConvertTypeReference(resultTypeReference);
            var resultTypeSize = resultType is SizedDataType ? ((SizedDataType)resultType).Size : 0;
            var operationResultVariableReference = stateMachine
                .CreateVariableWithNextUnusedIndexedName("binaryOperationResult", resultType)
                .ToReference();

            IVhdlElement binaryElement = binary;

            var leftTypeReference = expression.Left.GetActualTypeReference();
            var rightTypeReference = expression.Right.GetActualTypeReference();
            var isMultiplication = expression.Operator == BinaryOperatorType.Multiply;
            var shouldResize =
                (
                    // If the type of the result is the same as the type of the binary expression but the expression is a
                    // multiplication then this means that the result of the operation wouldn't fit into the result type.
                    // This is allowed in .NET (an explicit cast is needed in C# but that will be removed by the compiler)
                    // but will fail in VHDL with something like "[Synth 8-690] width mismatch in assignment; target has 
                    // 16 bits, source has 32 bits." In this cases we need to add a type conversion. Also see the block
                    // below
                    // E.g. ushort = ushort * ushort is valid in IL but in VHDL it must have a length truncation:
                    // unsigned(15 downto 0) = resize(unsigned(15 downto 0) * unsigned(15 downto 0), 16)
                    isMultiplication &&
                    (
                        resultTypeReference == expression.GetActualTypeReference() ||
                        resultTypeReference == leftTypeReference && resultTypeReference == rightTypeReference
                    )
                );

            if (leftTypeReference != null && rightTypeReference != null)
            {
                var leftType = _typeConverter.ConvertTypeReference(leftTypeReference);
                var leftTypeSize = leftType is SizedDataType ? ((SizedDataType)leftType).Size : 0;
                var rightType = _typeConverter.ConvertTypeReference(rightTypeReference);
                var rightTypeSize = rightType is SizedDataType ? ((SizedDataType)rightType).Size : 0;

                shouldResize = shouldResize ||
                    (
                        // If the operands and the result has the same size then the result won't fit.
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

            if (shouldResize)
            {
                binaryElement = new Invocation
                {
                    Target = "resize".ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                    {
                        { binary },
                        { ((SizedDataType)resultType).Size.ToVhdlValue(KnownDataTypes.UnrangedInt) }
                    }
                };
            }

            var operationResultAssignment = new Assignment
            {
                AssignTo = operationResultVariableReference,
                Expression = binaryElement
            };

            // Since the current state takes more than one clock cycle we add a new state and follow up there.
            if (!operationIsMultiCycle && currentBlock.RequiredClockCycles + clockCyclesNeededForOperation > 1)
            {
                var nextStateBlock = new InlineBlock(new LineComment(
                    "This state was added because the previous state would go over one clock cycle with any more operations."));
                var nextStateIndex = stateMachine.AddState(nextStateBlock);
                currentBlock.Add(stateMachine.CreateStateChange(nextStateIndex));
                currentBlock.ChangeBlockToDifferentState(nextStateBlock, nextStateIndex);
            }

            // If the operation in itself doesn't take more than one clock cycle then we simply add the operation to the
            // current block, which can be in a new state added previously above.
            if (!operationIsMultiCycle)
            {
                currentBlock.Add(operationResultAssignment);
                currentBlock.RequiredClockCycles += clockCyclesNeededForOperation;
            }
            // Since the operation in itself takes more than one clock cycle we need to add a new state just to wait.
            // Then we transition from that state forward to a state where the actual algorithm continues.
            else
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
                        operationResultVariableReference.ToVhdl(vhdlGenerationOptions) +
                        " (have to wait " + clockCyclesToWait + " clock cycles in this state)."),
                    new LineComment(
                    "The assignment needs to be kept up for multi-cycle operations for the result to actually appear in the target."),
                    operationResultAssignment);

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

                var waitForResultStateIndex = stateMachine.AddState(waitForResultBlock);
                stateMachine.States[waitForResultStateIndex].RequiredClockCycles = clockCyclesToWait;
                currentBlock.Add(stateMachine.CreateStateChange(waitForResultStateIndex));

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
                currentBlock.ChangeBlockToDifferentState(afterResultReceivedBlock, afterResultReceivedStateIndex);
            }

            return operationResultVariableReference;
        }
    }
}
