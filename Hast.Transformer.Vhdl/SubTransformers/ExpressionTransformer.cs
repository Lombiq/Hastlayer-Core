using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Synthesis;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using Orchard.Logging;
using Hast.Transformer.Vhdl.ArchitectureComponents;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;
        private readonly IInvocationExpressionTransformer _invocationExpressionTransformer;
        private readonly IDeviceDriver _deviceDriver;

        public ILogger Logger { get; set; }


        public ExpressionTransformer(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer,
            IInvocationExpressionTransformer invocationExpressionTransformer,
            IDeviceDriver deviceDriver)
        {
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
            _invocationExpressionTransformer = invocationExpressionTransformer;
            _deviceDriver = deviceDriver;

            Logger = NullLogger.Instance;
        }


        public IVhdlElement Transform(Expression expression, ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;

            if (expression is AssignmentExpression)
            {
                var assignment = (AssignmentExpression)expression;

                Func<Expression, Expression, Assignment> transformSimpleAssignmentExpression = (left, right) =>
                    new Assignment
                    {
                        AssignTo = (IDataObject)Transform(left, context),
                        Expression = Transform(right, context)
                    };

                // If the right side of an assignment is also an assignment that means that it's a single-line assignment
                // to multiple variables, so e.g. int a, b, c = 2; We flatten out such expression to individual simple
                // assignments.
                if (assignment.Right is AssignmentExpression)
                {
                    // Finding the rightmost expression that is the actual value assignment.
                    var currentRight = assignment.Right;
                    while (currentRight is AssignmentExpression)
                    {
                        currentRight = ((AssignmentExpression)currentRight).Right;
                    }

                    var actualAssignment = currentRight;

                    var assignmentsBlock = new InlineBlock();

                    assignmentsBlock.Add(transformSimpleAssignmentExpression(assignment.Left, actualAssignment));

                    currentRight = assignment.Right;
                    while (currentRight is AssignmentExpression)
                    {
                        var currentAssignment = ((AssignmentExpression)currentRight);

                        assignmentsBlock.Add(transformSimpleAssignmentExpression(currentAssignment.Left, actualAssignment));
                        currentRight = currentAssignment.Right;
                    }

                    return assignmentsBlock;
                }

                return transformSimpleAssignmentExpression(assignment.Left, assignment.Right);
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = (IdentifierExpression)expression;
                var reference = stateMachine.CreatePrefixedObjectName(identifier.Identifier).ToVhdlVariableReference();

                if (!(identifier.Parent is BinaryOperatorExpression)) return reference;

                return _typeConversionTransformer
                    .ImplementTypeConversionForBinaryExpression((BinaryOperatorExpression)identifier.Parent, reference, context);

            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = (PrimitiveExpression)expression;

                var typeReference = expression.GetActualTypeReference();
                if (typeReference != null)
                {
                    var type = _typeConverter.ConvertTypeReference(typeReference);
                    var valueString = primitive.Value.ToString();
                    // Replacing decimal comma to decimal dot.
                    if (type.TypeCategory == DataTypeCategory.Numeric) valueString = valueString.Replace(',', '.');

                    // If a constant value of type real doesn't contain a decimal separator then it will be detected as 
                    // integer and a type conversion would be needed. Thus we add a .0 to the end to indicate it's a real.
                    if (type == KnownDataTypes.Real && !valueString.Contains('.'))
                    {
                        valueString += ".0";
                    }

                    return new Value { DataType = type, Content = valueString };
                }
                else if (primitive.Parent is BinaryOperatorExpression)
                {
                    var reference = new DataObjectReference
                    {
                        DataObjectKind = DataObjectKind.Constant,
                        Name = primitive.ToString()
                    };

                    return _typeConversionTransformer.
                        ImplementTypeConversionForBinaryExpression((BinaryOperatorExpression)primitive.Parent, reference, context);
                }

                throw new InvalidOperationException(
                    "The type of the following primitive expression couldn't be determined: " +
                    expression.ToString());
            }
            else if (expression is BinaryOperatorExpression)
            {
                return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context);
            }
            else if (expression is InvocationExpression)
            {
                var invocationExpression = (InvocationExpression)expression;
                var transformedParameters = new List<IVhdlElement>();

                IEnumerable<Expression> arguments = invocationExpression.Arguments;

                // When the SimpleMemory object is passed around it can be omitted since state machines access the memory
                // directly.
                if (context.TransformationContext.UseSimpleMemory())
                {
                    arguments = arguments.Where(argument =>
                        {
                            var actualTypeReference = argument.GetActualTypeReference();
                            return actualTypeReference == null || !actualTypeReference.FullName.EndsWith("SimpleMemory");
                        });
                }

                foreach (var argument in arguments)
                {
                    transformedParameters.Add(Transform(argument, context));
                }

                return _invocationExpressionTransformer.TransformInvocationExpression(invocationExpression, context, transformedParameters);
            }
            // These are not needed at the moment. MemberReferenceExpression is handled in TransformInvocationExpression 
            // and a ThisReferenceExpression can only happen if "this" is passed to a method, which is not supported.
            //else if (expression is MemberReferenceExpression)
            //{
            //    var memberReference = (MemberReferenceExpression)expression;
            //    return Transform(memberReference.Target, context) + "." + memberReference.MemberName;
            //}
            //else if (expression is ThisReferenceExpression)
            //{
            //    var thisRef = expression as ThisReferenceExpression;
            //    return context.Scope.Method.Parent.GetFullName();
            //}
            else if (expression is UnaryOperatorExpression)
            {
                var unary = expression as UnaryOperatorExpression;

                // The increment/decrement unary operators are compiled into binary operators (e.g. i++ will be
                // i = i + 1) so we don't have to care about those.

                var transformedExpression = Transform(unary.Expression, context);

                switch (unary.Operator)
                {
                    case UnaryOperatorType.Minus:
                        return new Unary
                        {
                            Operator = UnaryOperator.Negation,
                            Expression = transformedExpression
                        };
                    case UnaryOperatorType.Not:
                        // In VHDL there is no boolean negation operator, just the not() function.
                        return new Invokation
                        {
                            Target = new Value { DataType = KnownDataTypes.Identifier, Content = "not" },
                            Parameters = new List<IVhdlElement> { transformedExpression }
                        };
                    case UnaryOperatorType.Plus:
                        return new Unary
                        {
                            Operator = UnaryOperator.Identity,
                            Expression = transformedExpression
                        };
                    default:
                        throw new NotSupportedException("Transformation of the unary operation " + unary.Operator + " is not supported.");
                }
            }
            else if (expression is TypeReferenceExpression)
            {
                var type = ((TypeReferenceExpression)expression).Type;
                var declaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(type);

                if (declaration == null)
                {
                    throw new InvalidOperationException("No matching type for \"" + ((SimpleType)type).Identifier + "\" found in the syntax tree. This can mean that the type's assembly was not added to the syntax tree.");
                }

                return new Value { DataType = KnownDataTypes.Identifier, Content = declaration.GetFullName() };
            }
            else if (expression is CastExpression)
            {
                return TransformCastExpression((CastExpression)expression, context);
            }
            else throw new NotSupportedException("Expressions of type " + expression.GetType() + " are not supported.");
        }


        private IVhdlElement TransformBinaryOperatorExpression(BinaryOperatorExpression expression, ISubTransformerContext context)
        {
            var binary = new Binary
            {
                Left = Transform(expression.Left, context),
                Right = Transform(expression.Right, context)
            };

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

            var resultType = expression.GetActualTypeReference(true);
            if (resultType == null) resultType = expression.Parent.GetActualTypeReference();
            var operationResultVariableReference = stateMachine
                .CreateVariableWithNextUnusedIndexedName(
                    "binaryOperationResult",
                    _typeConverter.ConvertTypeReference(resultType))
                .ToReference();

            var operationResultAssignment = new Assignment { AssignTo = operationResultVariableReference, Expression = binary };

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
                var waitedCyclesCountInitialValue = new Value { Content = "0", DataType = waitedCyclesCountVariable.DataType };
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
                        Right = new Value
                        {
                            Content = clockCyclesToWait.ToString(),
                            DataType = waitedCyclesCountVariable.DataType
                        }
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
                        Right = new Value { Content = "1", DataType = waitedCyclesCountVariable.DataType }
                    }
                };
                currentBlock.ChangeBlockToDifferentState(afterResultReceivedBlock, afterResultReceivedStateIndex);
            }

            return operationResultVariableReference;
        }

        private IVhdlElement TransformCastExpression(CastExpression expression, ISubTransformerContext context)
        {
            // This is a temporary workaround to get around cases where operations (e.g. multiplication) with 32b numbers
            // resulting in a 64b number would cause a cast to a 64b number type (what we don't support yet). 
            // See: https://lombiq.atlassian.net/browse/HAST-20
            var toTypeKeyword = ((PrimitiveType)expression.Type).Keyword;
            var fromTypeKeyword = expression.GetActualTypeReference().FullName;
            if (toTypeKeyword == "long" ||
                toTypeKeyword == "ulong" ||
                fromTypeKeyword == "System.Int64" ||
                fromTypeKeyword == "System.UInt64")
            {
                Logger.Warning("A cast from " + fromTypeKeyword + " to " + toTypeKeyword + " was omitted because non-32b numbers are not yet supported. If the result can indeed reach values above the 32b limit then overflow errors will occur. The affected expression: " + expression.ToString() + " in method " + context.Scope.Method.GetFullName() + ".");
                return Transform(expression.Expression, context);
            }

            var fromType = _typeConverter.ConvertTypeReference(expression.GetActualTypeReference());
            var toType = _typeConverter.ConvertAstType(expression.Type);

            return _typeConversionTransformer.ImplementTypeConversion(fromType, toType, Transform(expression.Expression, context));
        }
    }
}
