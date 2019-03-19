using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Common.Configuration;
using Hast.Synthesis.Services;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Testing;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;
        private readonly IInvocationExpressionTransformer _invocationExpressionTransformer;
        private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;
        private readonly IBinaryOperatorExpressionTransformer _binaryOperatorExpressionTransformer;
        private readonly IStateMachineInvocationBuilder _stateMachineInvocationBuilder;
        private readonly IRecordComposer _recordComposer;
        private readonly IDeclarableTypeCreator _declarableTypeCreator;


        public ExpressionTransformer(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer,
            IInvocationExpressionTransformer invocationExpressionTransformer,
            IArrayCreateExpressionTransformer arrayCreateExpressionTransformer,
            IBinaryOperatorExpressionTransformer binaryOperatorExpressionTransformer,
            IStateMachineInvocationBuilder stateMachineInvocationBuilder,
            IRecordComposer recordComposer,
            IDeclarableTypeCreator declarableTypeCreator)
        {
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
            _invocationExpressionTransformer = invocationExpressionTransformer;
            _arrayCreateExpressionTransformer = arrayCreateExpressionTransformer;
            _binaryOperatorExpressionTransformer = binaryOperatorExpressionTransformer;
            _stateMachineInvocationBuilder = stateMachineInvocationBuilder;
            _recordComposer = recordComposer;
            _declarableTypeCreator = declarableTypeCreator;
        }


        public IVhdlElement Transform(Expression expression, ISubTransformerContext context)
        {
            var scope = context.Scope;
            var stateMachine = scope.StateMachine;


            IVhdlElement implementTypeConversionForBinaryExpressionParent(DataObjectReference reference)
            {
                var binaryExpression = (BinaryOperatorExpression)expression.Parent;
                return _typeConversionTransformer.
                    ImplementTypeConversionForBinaryExpression(
                        binaryExpression,
                        reference,
                        binaryExpression.Left == expression,
                        context);
            }


            if (expression is AssignmentExpression assignment)
            {
                IVhdlElement transformSimpleAssignmentExpression(Expression left, Expression right)
                {
                    if (left.GetActualTypeReference().IsSimpleMemory()) return Empty.Instance;

                    var leftTransformed = Transform(left, context);
                    if (leftTransformed == Empty.Instance) return Empty.Instance;

                    IVhdlElement rightTransformed;
                    if (right is NullReferenceExpression)
                    {
                        ArrayHelper.ThrowArraysCantBeNullIfArray(right);
                        leftTransformed = NullableRecord.CreateIsNullFieldAccess((IDataObject)leftTransformed);
                        rightTransformed = Value.True;
                    }
                    else
                    {
                        rightTransformed = Transform(right, context);
                    }
                    if (rightTransformed == Empty.Instance) return Empty.Instance;

                    // _typeConversionTransformer.ImplementTypeConversionForAssignment() could be used here, but that
                    // also needs the data types of both operands.

                    return new Assignment
                    {
                        AssignTo = (IDataObject)leftTransformed,
                        Expression = rightTransformed
                    };
                }

                string getTaskVariableIdentifier()
                {
                    // Retrieving the variable the Task is saved to. It's either an array or a standard variable.
                    if (assignment.Left is IndexerExpression)
                    {
                        return ((IdentifierExpression)((IndexerExpression)assignment.Left).Target).Identifier;
                    }
                    else
                    {
                        return ((IdentifierExpression)assignment.Left).Identifier;
                    }
                }

                int getMaxDegreeOfParallelism(EntityDeclaration entity) =>
                    context.TransformationContext
                        .GetTransformerConfiguration()
                        .GetMaxInvocationInstanceCountConfigurationForMember(entity)
                        .MaxDegreeOfParallelism;

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
                else
                {
                    InvocationExpression invocationExpression = null;

                    IEnumerable<TransformedInvocationParameter> transformFromSecondArgument() =>
                        invocationExpression.Arguments.Skip(1).Select(argument =>
                            new TransformedInvocationParameter
                            {
                                Reference = Transform(argument, context),
                                DataType = _declarableTypeCreator
                                    .CreateDeclarableType(argument, argument.GetActualTypeReference(), context.TransformationContext)
                            });

                    // Handling TPL-related DisplayClass instantiation (created in place of lambda delegates). These will 
                    // be like following: <>c__DisplayClass9_ = new PrimeCalculator.<>c__DisplayClass9_0();
                    string rightObjectFullName;
                    if (assignment.Right is ObjectCreateExpression rightObjectCreateExpression &&
                        (rightObjectFullName = rightObjectCreateExpression.Type.GetFullName()).IsDisplayClassName())
                    {
                        context.TransformationContext.TypeDeclarationLookupTable.Lookup(rightObjectCreateExpression.Type);

                        if (assignment.Left is IdentifierExpression leftIdentifierExpression)
                        {
                            scope.VariableNameToDisplayClassNameMappings[leftIdentifierExpression.Identifier] = rightObjectFullName;
                        }

                        return Empty.Instance;
                    }
                    // Omitting assignments like arg_97_0 = Task.Factory;
                    else if (assignment.Left.Is<IdentifierExpression>(identifier =>
                        scope.TaskFactoryVariableNames.Contains(identifier.Identifier)))
                    {
                        return Empty.Instance;
                    }
                    // Handling Task start calls like arg_9C_0[arg_9C_1] = arg_97_0.StartNew<bool>(arg_97_1, j);
                    else if (assignment.Right.Is(invocation =>
                        invocation.Target.Is<MemberReferenceExpression>(memberReference =>
                            memberReference.IsTaskStartNew() &&
                            memberReference.Target.Is<IdentifierExpression>(identifier =>
                                scope.TaskFactoryVariableNames.Contains(identifier.Identifier))),
                        out invocationExpression))
                    {
                        // The first argument is always for the Func that refers to the method on the DisplayClass
                        // generated for the lambda expression originally passed to the Task factory.
                        var funcVariablename = ((IdentifierExpression)invocationExpression.Arguments.First()).Identifier;
                        var targetMethod = scope.FuncVariableNameToDisplayClassMethodMappings[funcVariablename];

                        // We only need to care about the invocation here. Since this is a Task start there will be
                        // some form of await later.
                        _stateMachineInvocationBuilder.BuildInvocation(
                            targetMethod,
                            transformFromSecondArgument(),
                            getMaxDegreeOfParallelism(targetMethod),
                            context);

                        scope.TaskVariableNameToDisplayClassMethodMappings[getTaskVariableIdentifier()] =
                            targetMethod;

                        return Empty.Instance;
                    }
                    // Handling shorthand Task starts like:
                    // array[i] = Task.Factory.StartNew<bool>(new Func<object, bool>(this.<ParallelizedArePrimeNumbers2>b__9_0), num3);
                    else if (assignment.Right.Is(invocation =>
                        invocation.Target.Is<MemberReferenceExpression>(memberReference => memberReference.IsTaskStartNew()) &&
                        invocation.Arguments.First().Is<ObjectCreateExpression>(objectCreate =>
                            objectCreate.Type.GetFullName().Contains("Func")),
                        out invocationExpression))
                    {
                        var funcCreateExpression = (ObjectCreateExpression)invocationExpression.Arguments.First();

                        if (funcCreateExpression.Arguments.Single() is PrimitiveExpression)
                        {
                            throw new InvalidOperationException(
                                "The return value of the Task started as " + invocationExpression.ToString() +
                                " was substituted with a constant (" + funcCreateExpression.Arguments.Single().ToString() +
                                "). This means that the body of the Task isn't computing anything. Most possibly this is not what you wanted.");
                        }

                        var targetMethod = (MethodDeclaration)TaskParallelizationHelper
                            .GetTargetDisplayClassMemberFromFuncCreation(funcCreateExpression)
                            .FindMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);
                        var targetMaxDegreeOfParallelism = context.TransformationContext
                            .GetTransformerConfiguration()
                            .GetMaxInvocationInstanceCountConfigurationForMember(targetMethod)
                            .MaxDegreeOfParallelism;

                        // We only need to care about the invocation here. Since this is a Task start there will be
                        // some form of await later.
                        _stateMachineInvocationBuilder.BuildInvocation(
                            targetMethod,
                            transformFromSecondArgument(),
                            getMaxDegreeOfParallelism(targetMethod),
                            context);

                        scope.TaskVariableNameToDisplayClassMethodMappings[getTaskVariableIdentifier()] = targetMethod;

                        return Empty.Instance;
                    }
                }

                return transformSimpleAssignmentExpression(assignment.Left, assignment.Right);
            }
            else if (expression is IdentifierExpression identifierExpression)
            {
                var reference = stateMachine.CreatePrefixedObjectName(identifierExpression.Identifier).ToVhdlVariableReference();

                if (!(identifierExpression.Parent is BinaryOperatorExpression)) return reference;

                return implementTypeConversionForBinaryExpressionParent(reference);

            }
            else if (expression is PrimitiveExpression primitive)
            {
                var typeReference = primitive.GetActualTypeReference();

                var type = _typeConverter.ConvertTypeReference(typeReference, context.TransformationContext);
                var valueString = primitive.Value.ToString();
                // Replacing decimal comma to decimal dot.
                if (type.TypeCategory == DataTypeCategory.Scalar) valueString = valueString.Replace(',', '.');

                // If a constant value of type real doesn't contain a decimal separator then it will be detected as 
                // integer and a type conversion would be needed. Thus we add a .0 to the end to indicate it's a real.
                if (type == KnownDataTypes.Real && !valueString.Contains('.'))
                {
                    valueString += ".0";
                }

                // The to_signed() and to_unsigned() functions expect signed integer arguments (range: -2147483648 
                // to +2147483647). Thus if the literal is larger than an integer we need to use the binary notation 
                // without these functions.
                if (type.Name == KnownDataTypes.Int8.Name || type.Name == KnownDataTypes.UInt8.Name)
                {
                    var binaryLiteral = string.Empty;

                    if (type.Name == KnownDataTypes.Int8.Name)
                    {
                        var value = Convert.ToInt64(valueString);
                        if (value < -2147483648 || value > 2147483647) binaryLiteral = Convert.ToString(value, 2);
                    }
                    else
                    {
                        var value = Convert.ToUInt64(valueString);
                        if (value > 2147483647) binaryLiteral = Convert.ToString((long)value, 2);
                    }

                    if (!string.IsNullOrEmpty(binaryLiteral))
                    {
                        scope.CurrentBlock.Add(new LineComment(
                            "Since the integer literal " + valueString +
                            " was out of the VHDL integer range it was substituted with a binary literal (" +
                            binaryLiteral + ")."));

                        var size = type.GetSize();

                        if (binaryLiteral.Length < size)
                        {
                            binaryLiteral = binaryLiteral.PadLeft(size, '0');
                        }

                        return binaryLiteral.ToVhdlValue(new StdLogicVector { Size = size });
                    }
                }

                return valueString.ToVhdlValue(type);
            }
            else if (expression is BinaryOperatorExpression binaryExpression)
            {
                IVhdlElement leftTransformed;
                IVhdlElement rightTransformed;

                if (binaryExpression.Left is NullReferenceExpression)
                {
                    ArrayHelper.ThrowArraysCantBeNullIfArray(binaryExpression.Right);
                    rightTransformed = NullableRecord.CreateIsNullFieldAccess((IDataObject)Transform(binaryExpression.Right, context));
                    leftTransformed = Value.True;
                }
                else if (binaryExpression.Right is NullReferenceExpression)
                {
                    ArrayHelper.ThrowArraysCantBeNullIfArray(binaryExpression.Left);
                    leftTransformed = NullableRecord.CreateIsNullFieldAccess((IDataObject)Transform(binaryExpression.Left, context));
                    rightTransformed = Value.True;
                }
                else
                {
                    leftTransformed = Transform(binaryExpression.Left, context);
                    rightTransformed = Transform(binaryExpression.Right, context);
                }

                return _binaryOperatorExpressionTransformer.TransformBinaryOperatorExpression(
                    new PartiallyTransformedBinaryOperatorExpression
                    {
                        BinaryOperatorExpression = binaryExpression,
                        LeftTransformed = leftTransformed,
                        RightTransformed = rightTransformed
                    },
                    context);
            }
            else if (expression is InvocationExpression invocationExpression)
            {
                var transformedParameters = new List<ITransformedInvocationParameter>();

                IEnumerable<Expression> arguments = invocationExpression.Arguments;

                // When the SimpleMemory object is passed around it can be omitted since state machines access the memory
                // directly.
                if (context.TransformationContext.UseSimpleMemory())
                {
                    arguments = arguments.Where(argument => !argument.GetActualTypeReference().IsSimpleMemory());
                }

                foreach (var argument in arguments)
                {
                    transformedParameters.Add(new TransformedInvocationParameter
                    {
                        Reference = Transform(argument, context),
                        DataType = _declarableTypeCreator.CreateDeclarableType(argument, argument.GetActualTypeReference(), context.TransformationContext)
                    });
                }

                return _invocationExpressionTransformer
                    .TransformInvocationExpression(invocationExpression, transformedParameters, context);
            }
            else if (expression is MemberReferenceExpression memberReference)
            {

                // Handling array.Length with the VHDL length attribute.
                if (memberReference.IsArrayLengthAccess())
                {
                    // The length will always be a 32b int, but since everything else is signed or unsigned, we need to
                    // convert.
                    return Invocation.ToSigned(new Raw("{0}'length", Transform(memberReference.Target, context)), 32);
                }

                var memberFullName = memberReference.GetMemberFullName();

                // Expressions like return Task.CompletedTask;
                if (memberFullName.IsTaskCompletedTaskPropertyName())
                {
                    return Empty.Instance;
                }

                // Field reference expressions in DisplayClasses are supported.
                if (memberReference.Target is ThisReferenceExpression && memberFullName.IsDisplayClassMemberName())
                {
                    // These fields are global and correspond to the DisplayClass class so they shouldn't be prefixed
                    // with the state machine's name.
                    return memberFullName.ToExtendedVhdlId().ToVhdlVariableReference();
                }

                var targetIdentifier = (memberReference.Target as IdentifierExpression)?.Identifier;
                if (targetIdentifier != null)
                {
                    if (scope.VariableNameToDisplayClassNameMappings.TryGetValue(targetIdentifier, out var displayClassName))
                    {
                        // This is an assignment like: <>c__DisplayClass9_.<>4__this = this; This can be omitted.
                        if (memberReference.MemberName.EndsWith("__this"))
                        {
                            return Empty.Instance;
                        }
                        // Otherwise this is field access on the DisplayClass object (the field was created to pass variables
                        // from the local scope to the method generated from the lambda expression). Can look something like:
                        // <>c__DisplayClass9_.numbers = new uint[35];
                        else
                        {
                            return context.TransformationContext.TypeDeclarationLookupTable
                                .Lookup(displayClassName)
                                .Members
                                .Single(member => member
                                    .Is<FieldDeclaration>(field => field.Variables.Single().Name == memberReference.MemberName))
                                .GetFullName()
                                .ToExtendedVhdlId()
                                .ToVhdlVariableReference();
                        }
                    }
                }

                // Is this reference to an enum's member?
                if (memberReference.Target is TypeReferenceExpression targetTypeReferenceExpression &&
                    context.TransformationContext.TypeDeclarationLookupTable.Lookup(targetTypeReferenceExpression)?.ClassType == ClassType.Enum)
                {
                    return memberFullName.ToExtendedVhdlIdValue();
                }

                // Is this a Task result access like array[k].Result or task.Result?
                var targetTypeReference = memberReference.Target.GetActualTypeReference();
                if (targetTypeReference != null &&
                    targetTypeReference.FullName.StartsWith("System.Threading.Tasks.Task") &&
                    memberReference.MemberName == "Result")
                {
                    // If this is not an array then it doesn't need to be explicitly awaited, just access to its
                    // Result property should await it. So doing it here.
                    if (memberReference.Target is IdentifierExpression && !targetTypeReference.IsArray)
                    {
                        var targetMethod = scope
                            .TaskVariableNameToDisplayClassMethodMappings[((IdentifierExpression)memberReference.Target).Identifier];
                        return _stateMachineInvocationBuilder
                            .BuildSingleInvocationWait(targetMethod, 0, context);
                    }
                    else
                    {
                        // We know that we've already handled the target so it stores the result objects, so just need 
                        // to use them directly.
                        return Transform(memberReference.Target, context);
                    }
                }

                // Otherwise it's reference to an object's member.
                return new RecordFieldAccess
                {
                    Instance = (IDataObject)Transform(memberReference.Target, context),
                    FieldName = memberReference.MemberName.ToExtendedVhdlId()
                };
            }
            else if (expression is UnaryOperatorExpression)
            {
                // Increment/decrement unary operators that are in their own statements are compiled into binary operators 
                // (e.g. i++ will be i = i + 1) so we don't have to care about those.

                // Since unary operations can also take significant time (but they can't be multi-cycle) to complete 
                // they're also assigned to result variables as with binary operator expressions.

                var unary = expression as UnaryOperatorExpression;


                var expressionType = _typeConverter
                    .ConvertTypeReference(
                        unary.Expression is CastExpression ? unary.Expression.GetActualTypeReference(true) : unary.Expression.GetActualTypeReference(),
                        context.TransformationContext);
                var expressionSize = expressionType.GetSize();
                var clockCyclesNeededForOperation = context.TransformationContext.DeviceDriver
                    .GetClockCyclesNeededForUnaryOperation(unary, expressionSize, expressionType.Name == "signed");

                stateMachine.AddNewStateAndChangeCurrentBlockIfOverOneClockCycle(context, clockCyclesNeededForOperation);
                scope.CurrentBlock.RequiredClockCycles += clockCyclesNeededForOperation;

                var transformedExpression = Transform(unary.Expression, context);
                IVhdlElement transformedOperation;

                switch (unary.Operator)
                {
                    case UnaryOperatorType.Minus:
                        // Casting if the result type is not what the parent expects.
                        var parentTypeInformation = unary.Parent.Annotation<TypeInformation>();
                        if (!(unary.FindFirstNonParenthesizedExpressionParent() is CastExpression) &&
                            parentTypeInformation != null && 
                            parentTypeInformation.ExpectedType != parentTypeInformation.InferredType &&
                            parentTypeInformation.ExpectedType != null && parentTypeInformation.InferredType != null)
                        {
                            var fromType = _typeConverter
                                .ConvertTypeReference(parentTypeInformation.ExpectedType, context.TransformationContext);
                            var toType = _typeConverter
                                .ConvertTypeReference(parentTypeInformation.InferredType, context.TransformationContext);

                            if (KnownDataTypes.Integers.Contains(fromType) && KnownDataTypes.Integers.Contains(toType))
                            {
                                transformedOperation = _typeConversionTransformer.ImplementTypeConversion(
                                    fromType,
                                    toType,
                                    new Binary
                                    {
                                        Left = "0".ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        Operator = BinaryOperator.Subtract,
                                        Right = transformedExpression
                                    })
                                    .ConvertedFromExpression;
                            }
                            else
                            {
                                transformedOperation = new Unary
                                {
                                    Operator = UnaryOperator.Negation,
                                    Expression = transformedExpression
                                };
                            }
                        }

                        else
                        {
                            transformedOperation = new Unary
                            {
                                Operator = UnaryOperator.Negation,
                                Expression = transformedExpression
                            };
                        }

                        break;
                    case UnaryOperatorType.Not:
                    case UnaryOperatorType.BitNot:
                        // In VHDL there is no boolean negation operator, just the not() function. This will bitwise
                        // negate the value, so for bools it will work as the .NET NOT operator, for other types as a 
                        // bitwise NOT.
                        transformedOperation = new Invocation("not", transformedExpression);

                        break;
                    case UnaryOperatorType.Plus:
                        transformedOperation = new Unary
                        {
                            Operator = UnaryOperator.Identity,
                            Expression = transformedExpression
                        };

                        break;
                    case UnaryOperatorType.Await:
                        if (unary.Expression is InvocationExpression &&
                            unary.Expression.GetFullName().IsTaskFromResultMethodName())
                        {
                            transformedOperation = transformedExpression;
                            break;
                        }

                        // Otherwise nothing to do.
                        goto default;
                    default:
                        throw new NotSupportedException(
                            "Transformation of the unary operation " + unary.Operator + " is not supported."
                            .AddParentEntityName(unary));
                }

                var operationResultDataObjectReference = stateMachine
                        .CreateVariableWithNextUnusedIndexedName("unaryOperationResult", expressionType)
                        .ToReference();
                scope.CurrentBlock.Add(new Assignment
                {
                    AssignTo = operationResultDataObjectReference,
                    Expression = transformedOperation
                });

                return operationResultDataObjectReference;
            }
            else if (expression is TypeReferenceExpression)
            {
                var type = ((TypeReferenceExpression)expression).Type;
                var declaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(type);

                if (declaration == null) ExceptionHelper.ThrowDeclarationNotFoundException(((SimpleType)type).Identifier, expression);

                return declaration.GetFullName().ToVhdlValue(KnownDataTypes.Identifier);
            }
            else if (expression is CastExpression castExpression)
            {
                var innerExpressionResult = Transform(castExpression.Expression, context);

                // To avoid double-casting of binary expression results BinaryOperatorExpressionTransformer also checks
                // if the parent is a cast. So then no need to cast again.
                if (castExpression.Expression is BinaryOperatorExpression ||
                    castExpression.Expression.Is<ParenthesizedExpression>(parenthesized => parenthesized.Expression is BinaryOperatorExpression))
                {
                    return innerExpressionResult;
                }

                var fromTypeReference = castExpression.Expression.GetActualTypeReference() ?? castExpression.GetActualTypeReference();
                var fromType = _declarableTypeCreator
                    .CreateDeclarableType(castExpression.Expression, fromTypeReference, context.TransformationContext);
                var toType = _declarableTypeCreator
                    .CreateDeclarableType(castExpression, castExpression.Type, context.TransformationContext);

                // If the inner expression produced a data object then let's check the size of that: if it's the same
                // size as the target type of the cast then no need to cast again.
                var resultDataObject = innerExpressionResult as IDataObject;
                var resultParentesized = innerExpressionResult as Parenthesized;
                if (resultDataObject == null && resultParentesized != null)
                {
                    resultDataObject = resultParentesized.Target as IDataObject;
                }

                var typeConversionResult = _typeConversionTransformer
                    .ImplementTypeConversion(fromType, toType, innerExpressionResult);
                if (typeConversionResult.IsLossy)
                {
                    scope.Warnings.AddWarning(
                        "LossyCast",
                        "A cast from " + fromType.ToVhdl() + " to " + toType.ToVhdl() +
                        " was lossy. If the result can indeed reach values outside the target type's limits then underflow or overflow errors will occur. The affected expression: " +
                        expression.ToString() + " in method " + scope.Method.GetFullName() + ".");
                }

                return typeConversionResult.ConvertedFromExpression;
            }
            else if (expression is ArrayCreateExpression)
            {
                return _arrayCreateExpressionTransformer.Transform((ArrayCreateExpression)expression, context);
            }
            else if (expression is IndexerExpression)
            {
                var indexerExpression = expression as IndexerExpression;

                var targetVariableReference = Transform(indexerExpression.Target, context) as IDataObject;

                if (targetVariableReference == null)
                {
                    throw new InvalidOperationException(
                        "The target of the indexer expression " + expression.ToString() +
                        " couldn't be transformed to a data object reference.".AddParentEntityName(expression));
                }

                if (indexerExpression.Arguments.Count != 1)
                {
                    throw new NotSupportedException(
                        "Accessing elements of only single-dimensional arrays are supported.".AddParentEntityName(expression));
                }

                var indexExpression = indexerExpression.Arguments.Single();
                return new ArrayElementAccess
                {
                    ArrayReference = targetVariableReference,
                    IndexExpression = _typeConversionTransformer
                        .ImplementTypeConversion(
                            _typeConverter.ConvertTypeReference(indexExpression.GetActualTypeReference(), context.TransformationContext),
                            KnownDataTypes.UnrangedInt,
                            Transform(indexExpression, context))
                        .ConvertedFromExpression
                };
            }
            else if (expression is ParenthesizedExpression parenthesizedExpression)
            {
                return new Parenthesized
                {
                    Target = Transform(parenthesizedExpression.Expression, context)
                };
            }
            else if (expression is ObjectCreateExpression objectCreateExpression)
            {
                var initiailizationResult = InitializeRecord(expression, objectCreateExpression.Type, context);

                // Running the constructor, which needs to be done before initializers.
                var constructorFullName = objectCreateExpression.GetConstructorFullName();
                if (!string.IsNullOrEmpty(constructorFullName))
                {
                    if (context.TransformationContext.TypeDeclarationLookupTable
                        .Lookup(objectCreateExpression.Type)
                        .Members
                        .SingleOrDefault(member => member.GetFullName() == constructorFullName) is MethodDeclaration constructor)
                    {
                        scope.CurrentBlock.Add(new LineComment("Invoking the target's constructor."));

                        // The easiest is to fake an invocation.
                        var constructorInvocation = new InvocationExpression(
                            new MemberReferenceExpression(
                                new TypeReferenceExpression(objectCreateExpression.Type.Clone()),
                                constructor.Name),
                            // Passing ctor parameters, and an object reference as the first one (since all methods were
                            // converted to static with the first parameter being @this).
                            new[] { initiailizationResult.RecordInstanceIdentifier.Clone() }
                                .Union(objectCreateExpression.Arguments.Select(argument => argument.Clone())));

                        expression.CopyAnnotationsTo(constructorInvocation);

                        // Creating a clone of the expression's sub-tree where object creation is replaced to make the 
                        // fake InvocationExpression realistic. A clone is needed not to cause concurrency issues if the
                        // same expression is processed on multiple threads for multiple hardware copies.
                        var expressionName = expression.GetFullName();

                        var subTreeClone = expression.FindFirstParentEntityDeclaration().Clone();
                        var objectCreateExpressionClone = subTreeClone
                            .FindFirstChildOfType<ObjectCreateExpression>(cloneExpression => cloneExpression.GetFullName() == expressionName);
                        objectCreateExpressionClone.ReplaceWith(constructorInvocation);

                        Transform(constructorInvocation, context);
                    }
                }

                // There is no need for object creation per se, nothing should be on the right side of an assignment.
                return Empty.Instance;
            }
            else if (expression is DefaultValueExpression defaultValueExpression)
            {
                // The only case when a default() will remain in the syntax tree is for composed types. For primitives
                // a constant will be substituted. E.g. instead of default(int) a 0 will be in the AST.
                var initiailizationResult = InitializeRecord(expression, defaultValueExpression.Type, context);

                ArrayHelper.ThrowArraysCantBeNullIfArray(defaultValueExpression);

                context.Scope.CurrentBlock.Add(new Assignment
                {
                    AssignTo = NullableRecord.CreateIsNullFieldAccess(initiailizationResult.RecordInstanceReference),
                    Expression = Value.True
                });

                // There is no need for struct instantiation per se if the value was originally assigned to a
                // variable/field/property, nothing should be on the right side of an assignment.
                return Empty.Instance;
            }
            else if (expression is DirectionExpression directionExpression)
            {
                // DirectionExpressions like ref and out modifiers on method invocation arguments don't need to be 
                // handled specially: these are just out-flowing parameters.
                return Transform(directionExpression.Expression, context);
            }
            else
            {
                throw new NotSupportedException(
                    "Expressions of type " +
                    expression.GetType() + " are not supported. The expression was: " +
                    expression.ToString().AddParentEntityName(expression));
            }
        }


        private RecordInitializationResult InitializeRecord(Expression expression, AstType recordAstType, ISubTransformerContext context)
        {
            // Objects are mimicked with records and those don't need instantiation. However it's useful to
            // initialize all record fields to their default or initial values (otherwise if e.g. a class is
            // instantiated in a loop in the second run the old values could be accessed in VHDL).

            var typeDeclaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(recordAstType);

            if (typeDeclaration == null) ExceptionHelper.ThrowDeclarationNotFoundException(recordAstType.GetFullName(), expression);

            var record = _recordComposer.CreateRecordFromType(typeDeclaration, context.TransformationContext);

            if (record.Fields.Any())
            {
                context.Scope.CurrentBlock.Add(new LineComment("Initializing record fields to their defaults."));
            }

            var result = new RecordInitializationResult { Record = record };

            var parentAssignment = expression
                .FindFirstParentOfType<AssignmentExpression>(assignment => assignment.Right == expression);

            // This will only work if the newly created object is assigned to a variable or something else. It won't
            // work if the newly created object is directly passed to a method for example. However
            // DirectlyAccessedNewObjectVariablesCreator takes care of that.
            var recordInstanceAssignmentTarget = parentAssignment.Left;
            result.RecordInstanceIdentifier =
                recordInstanceAssignmentTarget is IdentifierExpression ||
                recordInstanceAssignmentTarget is IndexerExpression ||
                recordInstanceAssignmentTarget is MemberReferenceExpression ?
                    recordInstanceAssignmentTarget :
                    recordInstanceAssignmentTarget.FindFirstParentOfType<IdentifierExpression>();
            result.RecordInstanceReference = (IDataObject)Transform(result.RecordInstanceIdentifier, context);

            foreach (var field in record.Fields)
            {
                var initializationValue = field.DataType.DefaultValue;

                if (typeDeclaration.Members
                    .SingleOrDefault(member =>
                        member.Is<FieldDeclaration>(f =>
                            f.Variables.Single().Name == field.Name.TrimExtendedVhdlIdDelimiters())) is FieldDeclaration fieldDeclaration)
                {
                    var fieldInitializer = fieldDeclaration.Variables.Single().Initializer;
                    if (fieldInitializer != Expression.Null)
                    {
                        initializationValue = Transform(fieldInitializer, context) as Value;
                    }
                }

                if (initializationValue != null)
                {
                    context.Scope.CurrentBlock.Add(new Assignment
                    {
                        AssignTo = new RecordFieldAccess
                        {
                            Instance = result.RecordInstanceReference,
                            FieldName = field.Name
                        },
                        Expression = initializationValue
                    });
                }
            }

            return result;
        }


        private class RecordInitializationResult
        {
            public NullableRecord Record { get; set; }
            public IDataObject RecordInstanceReference { get; set; }
            public Expression RecordInstanceIdentifier { get; set; }
        }
    }
}
