using Hast.Common.Configuration;
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
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
                /*
                Task.Factory.StartNew(lambda => here) calls are compiled into one of the two version:
                * If the lambda is a closure on local variables then a DisplayClass with fields for all local variables
                  and a method(e.g.ParallelAlgorithm, MonteCarloPiEstimator).Sometimes such a DisplayClass will be
                  generated even if no local variables are accessed(e.g. (ParallelizedCalculateIntegerSumUpToNumbers()).
                * If no local variables are access then a method with the[CompilerGenerated] attribute within the same
                  class (e.g.ImageContrastModifier, Fix64Calculator.PrimeCalculator.ParallelizedArePrimeNumbers(),
                  KpzKernelsParallelizedInterface, Posit32Calculator.ParallelizedCalculateIntegerSumUpToNumbers()).

                Each of these start with:
                    Task<OutputType>[] array;
                    array = new Task<OutputType>[numberOfTasks];

                In the first case ("array" is a Task<T> array) the DisplayClass will be instantiated, its fields populated and then the Task started:
                    <>c__DisplayClass4_0<> c__DisplayClass4_;
                    <>c__DisplayClass4_ = new <>c__DisplayClass4_0();
                    <>c__DisplayClass4_.localVariable1 = ...;
                    <>c__DisplayClass4_.localVariable2 = ...;
                    array[i] = Task.Factory.StartNew(<>c__DisplayClass4_.<>9__0 ?? (<>c__DisplayClass4_.<>9__0 = <>c__DisplayClass4_.<NameOfTaskStartingMethod>b__0), inputArgument);

                In the second case there will be just a Task start invocation:
                    array[i] = Task.Factory.StartNew((Func<object, OutputType>)this.<NameOfTaskStartingMethod>b__6_0, (object) inputArgument);

                Since both cases are almost all assignments we're handling them mostly here.
                Both of these are then awaited as:
                    Task.WhenAll(array).Wait();
                */

                IVhdlElement transformSimpleAssignmentExpression(Expression left, Expression right)
                {
                    var leftType = left.GetActualType();
                    if (leftType.IsSimpleMemory()) return Empty.Instance;

                    var leftTransformed = Transform(left, context);
                    if (leftTransformed == Empty.Instance) return Empty.Instance;

                    IVhdlElement rightTransformed;
                    if (right is NullReferenceExpression)
                    {
                        ArrayHelper.ThrowArraysCantBeNullIfArray(assignment);
                        leftTransformed = NullableRecord.CreateIsNullFieldAccess((IDataObject)leftTransformed);
                        rightTransformed = Value.True;
                    }
                    else
                    {
                        rightTransformed = Transform(right, context);
                    }

                    var leftDataObject = (IDataObject)leftTransformed;

                    if (left is IdentifierExpression &&
                        stateMachine.LocalAliases.Any(alias => alias.Name == leftDataObject.Name))
                    {
                        // The left variable was previously swapped for an alias to allow reference-like behavior so
                        // changes made to record fields are propagated. However such aliases can't be assigned to as
                        // that would also overwrite the original variable.
                        throw new NotSupportedException(
                            "The assignment " + expression +
                            " is not supported. You can't at the moment assign to a variable that you previously assigned to using a reference type-holding variable."
                            .AddParentEntityName(assignment));
                    }

                    if (rightTransformed == Empty.Instance) return Empty.Instance;

                    var rightType = right.GetActualType();

                    if (assignment.IsPotentialAliasAssignment())
                    {
                        // This is an assignment which is possibly just an alias. Since there are no references in VHDL
                        // the best option is to use aliases (instead of assigning back variables after every change).
                        // This is not perfect though since if the now alias variable is assigned to then that won't
                        // work, see the exception above.
                        // This might not be needed because of UnneededReferenceVariablesRemover. 

                        // Switching the left variable out with an alias so it'll have reference-like behavior.

                        var leftVariable = stateMachine.LocalVariables.Single(variable => variable.Name == leftDataObject.Name);
                        stateMachine.LocalVariables.Remove(leftVariable);
                        var aliasedObjectReference =
                            right.Is<MemberReferenceExpression>(reference => reference.IsFieldReference()) ?
                                (IDataObject)rightTransformed :
                                stateMachine.LocalVariables
                                    .Single(variable => variable.Name == ((IDataObject)rightTransformed).Name)
                                    .ToReference();

                        stateMachine.LocalAliases.Add(
                            new Alias
                            {
                                Name = leftDataObject.Name,
                                AliasedObject = aliasedObjectReference,
                                DataType = leftVariable.DataType
                            });
                    }

                    if (!(right is NullReferenceExpression))
                    {
                        var typeConversionResult = _typeConversionTransformer.ImplementTypeConversionForAssignment(
                            _typeConverter.ConvertType(rightType, context.TransformationContext),
                            _typeConverter.ConvertType(leftType, context.TransformationContext),
                            rightTransformed,
                            leftDataObject);
                        leftDataObject = typeConversionResult.ConvertedToDataObject;
                        rightTransformed = typeConversionResult.ConvertedFromExpression;
                    }

                    return new Assignment
                    {
                        AssignTo = leftDataObject,
                        Expression = rightTransformed
                    };
                }

                string getTaskVariableIdentifier() =>
                    // Retrieving the variable the Task is saved to. It's either an array or a standard variable.
                    assignment.Left is IndexerExpression indexerExpression
                        ? ((IdentifierExpression)indexerExpression.Target).Identifier
                        : ((IdentifierExpression)assignment.Left).Identifier;

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
                    while (currentRight is AssignmentExpression assignmentExpression)
                    {
                        currentRight = assignmentExpression.Right;
                    }

                    var actualAssignment = currentRight;

                    var assignmentsBlock = new InlineBlock();

                    assignmentsBlock.Add(transformSimpleAssignmentExpression(assignment.Left, actualAssignment));

                    currentRight = assignment.Right;
                    while (currentRight is AssignmentExpression assignmentExpression)
                    {
                        var currentAssignment = assignmentExpression;

                        assignmentsBlock.Add(transformSimpleAssignmentExpression(currentAssignment.Left, actualAssignment));
                        currentRight = currentAssignment.Right;
                    }

                    return assignmentsBlock;
                }
                else
                {
                    // Handling TPL-related DisplayClass instantiation (created in place of lambda delegates). These will 
                    // be like following: <>c__DisplayClass4_ = new <>c__DisplayClass4_0();
                    string rightObjectFullName;
                    if (assignment.Right is ObjectCreateExpression rightObjectCreateExpression &&
                        (rightObjectFullName = rightObjectCreateExpression.Type.GetFullName()).IsDisplayOrClosureClassName())
                    {
                        context.TransformationContext.TypeDeclarationLookupTable.Lookup(rightObjectCreateExpression.Type);

                        if (assignment.Left is IdentifierExpression leftIdentifierExpression)
                        {
                            scope.VariableNameToDisplayClassNameMappings[leftIdentifierExpression.Identifier] = rightObjectFullName;
                        }

                        return Empty.Instance;
                    }

                    // Handling Task starts like:
                    // array[i] = Task.Factory.StartNew(<>c__DisplayClass4_.<>9__0 ?? (<>c__DisplayClass4_.<>9__0 = <>c__DisplayClass4_.<NameOfTaskStartingMethod>b__0), inputArgument);
                    // array[i] = Task.Factory.StartNew((Func<object, OutputType>)this.<NameOfTaskStartingMethod>b__6_0, (object) inputArgument);
                    else if (assignment.Right.Is<InvocationExpression>(
                        invocation => invocation.IsTaskStart(),
                        out var invocationExpression))
                    {
                        var firstArgument = invocationExpression.Arguments.First();
                        MethodDeclaration targetMethod;

                        // Is this the first type of Task starts?
                        if (firstArgument is BinaryOperatorExpression binaryOperatorExpression)
                        {
                            targetMethod = binaryOperatorExpression
                                .Right
                                .As<ParenthesizedExpression>()
                                .Expression
                                .As<AssignmentExpression>()
                                .Right
                                .As<MemberReferenceExpression>()
                                .FindMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable)
                                .As<MethodDeclaration>();
                        }

                        // Or the second one?
                        else
                        {
                            targetMethod = firstArgument
                                .As<CastExpression>()
                                .Expression
                                .As<MemberReferenceExpression>()
                                .FindMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable)
                                .As<MethodDeclaration>();
                        }

                        var targetMaxDegreeOfParallelism = context.TransformationContext
                            .GetTransformerConfiguration()
                            .GetMaxInvocationInstanceCountConfigurationForMember(targetMethod)
                            .MaxDegreeOfParallelism;

                        // We only need to care about the invocation here. Since this is a Task start there will be
                        // some form of await later.
                        _stateMachineInvocationBuilder.BuildInvocation(
                            targetMethod,
                            invocationExpression.Arguments.Skip(1).Select(argument =>
                                new TransformedInvocationParameter
                                {
                                    Reference = Transform(argument, context),
                                    DataType = _declarableTypeCreator
                                        .CreateDeclarableType(argument, argument.GetActualType(), context.TransformationContext)
                                }),
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

                return !(identifierExpression.Parent is BinaryOperatorExpression)
                    ? reference
                    : implementTypeConversionForBinaryExpressionParent(reference);
            }
            else if (expression is PrimitiveExpression primitive)
            {
                var type = primitive.GetActualType();

                var vhdlType = _typeConverter.ConvertType(type, context.TransformationContext);
                var valueString = primitive.Value.ToString();
                // Replacing decimal comma to decimal dot.
                if (vhdlType.TypeCategory == DataTypeCategory.Scalar) valueString = valueString.Replace(',', '.');

                // If a constant value of type real doesn't contain a decimal separator then it will be detected as 
                // integer and a type conversion would be needed. Thus we add a .0 to the end to indicate it's a real.
                if (vhdlType == KnownDataTypes.Real && !valueString.Contains('.', StringComparison.Ordinal))
                {
                    valueString += ".0";
                }

                // The to_signed() and to_unsigned() functions expect signed integer arguments (range: -2147483648 
                // to +2147483647). Thus if the literal is larger than an integer we need to use the binary notation 
                // without these functions.
                if (vhdlType.Name == KnownDataTypes.Int8.Name || vhdlType.Name == KnownDataTypes.UInt8.Name)
                {
                    var binaryLiteral = string.Empty;

                    if (vhdlType.Name == KnownDataTypes.Int8.Name)
                    {
                        var value = Convert.ToInt64(valueString, CultureInfo.InvariantCulture);
                        if (value < -2147483648 || value > 2147483647) binaryLiteral = Convert.ToString(value, 2);
                    }
                    else
                    {
                        var value = Convert.ToUInt64(valueString, CultureInfo.InvariantCulture);
                        if (value > 2147483647) binaryLiteral = Convert.ToString((long)value, 2);
                    }

                    if (!string.IsNullOrEmpty(binaryLiteral))
                    {
                        scope.CurrentBlock.Add(new LineComment(
                            "Since the integer literal " + valueString +
                            " was out of the VHDL integer range it was substituted with a binary literal (" +
                            binaryLiteral + ")."));

                        var size = vhdlType.GetSize();

                        if (binaryLiteral.Length < size)
                        {
                            binaryLiteral = binaryLiteral.PadLeft(size, '0');
                        }

                        return binaryLiteral.ToVhdlValue(new StdLogicVector { Size = size });
                    }
                }

                return valueString.ToVhdlValue(vhdlType);
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
                if (invocationExpression.IsTaskStart()) return Empty.Instance;

                var transformedParameters = new List<ITransformedInvocationParameter>();

                IEnumerable<Expression> arguments = invocationExpression.Arguments;

                // When the SimpleMemory object is passed around it can be omitted since state machines access the
                // memory directly.
                if (context.TransformationContext.UseSimpleMemory())
                {
                    arguments = arguments.Where(argument => !argument.GetActualType().IsSimpleMemory());
                }

                foreach (var argument in arguments)
                {
                    transformedParameters.Add(new TransformedInvocationParameter
                    {
                        Reference = Transform(argument, context),
                        DataType = _declarableTypeCreator.CreateDeclarableType(argument, argument.GetActualType(), context.TransformationContext)
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
                if (memberReference.Target is ThisReferenceExpression && memberFullName.IsDisplayOrClosureClassMemberName())
                {
                    // These fields are global and correspond to the DisplayClass class so they shouldn't be prefixed
                    // with the state machine's name.
                    return memberFullName.ToExtendedVhdlId().ToVhdlVariableReference();
                }

                var targetIdentifier = (memberReference.Target as IdentifierExpression)?.Identifier;
                if (targetIdentifier != null &&
                    scope.VariableNameToDisplayClassNameMappings.TryGetValue(targetIdentifier, out var displayClassName))
                {
                    // This is field access on the DisplayClass object (the field was created to pass variables from
                    // the local scope to the method generated from the lambda expression). Can look something like:
                    // <>c__DisplayClass9_.numbers = new uint[35];
                    return context.TransformationContext.TypeDeclarationLookupTable
                        .Lookup(displayClassName)
                        .Members
                        .Single(member => member
                            .Is<FieldDeclaration>(field => field.Variables.Single().Name == memberReference.MemberName))
                        .GetFullName()
                        .ToExtendedVhdlId()
                        .ToVhdlVariableReference();
                }

                // Is this a reference to an enum's member?
                if (memberReference.Target is TypeReferenceExpression targetTypeReferenceExpression &&
                    context.TransformationContext.TypeDeclarationLookupTable.Lookup(targetTypeReferenceExpression)?.ClassType == ClassType.Enum)
                {
                    return memberFullName.ToExtendedVhdlIdValue();
                }

                // Is this a Task result access like array[k].Result or task.Result?
                var targetType = memberReference.Target.GetActualType();
                if (targetType != null &&
                    targetType.GetFullName().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal) &&
                    memberReference.MemberName == "Result")
                {
                    // If this is not an array then it doesn't need to be explicitly awaited, just access to its
                    // Result property should await it. So doing it here.
                    if (memberReference.Target is IdentifierExpression targetIdentifierExpression && !targetType.IsArray())
                    {
                        var targetMethod = scope
                            .TaskVariableNameToDisplayClassMethodMappings[targetIdentifierExpression.Identifier];
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
            else if (expression is UnaryOperatorExpression unary)
            {
                // Increment/decrement unary operators that are in their own statements are compiled into binary operators 
                // (e.g. i++ will be i = i + 1) so we don't have to care about those.

                // Since unary operations can also take significant time (but they can't be multi-cycle) to complete 
                // they're also assigned to result variables as with binary operator expressions.

                var expressionType = _typeConverter
                    .ConvertType(unary.Expression.GetActualType(), context.TransformationContext);
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
                        transformedOperation = new Unary
                        {
                            Operator = UnaryOperator.Negation,
                            Expression = transformedExpression
                        };

                        break;
                    case UnaryOperatorType.Not:
                    case UnaryOperatorType.BitNot:
                        // In VHDL there is no boolean negation operator, just the not() function. This will bitwise
                        // negate the value, so for bools it will work as the .NET NOT operator, for other types as a 
                        // bitwise NOT.
                        transformedOperation = new Invocation("not", transformedExpression);

                        break;
                    case UnaryOperatorType.Plus:
                        // Unary plus is a noop.
                        transformedOperation = transformedExpression;

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
            else if (expression is TypeReferenceExpression typeReferenceExpression)
            {
                var type = typeReferenceExpression.Type;
                var declaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(type);

                if (declaration == null) ExceptionHelper.ThrowDeclarationNotFoundException(type.GetFullName(), expression);

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

                var fromType = castExpression.Expression.GetActualType();
                var fromVhdlType = _declarableTypeCreator
                    .CreateDeclarableType(castExpression.Expression, fromType, context.TransformationContext);
                var toVhdlType = _declarableTypeCreator
                    .CreateDeclarableType(castExpression, castExpression.Type, context.TransformationContext);

                // If the inner expression produced a data object then let's check the size of that: if it's the same
                // size as the target type of the cast then no need to cast again.
                var resultDataObject = innerExpressionResult as IDataObject;
                if (resultDataObject == null && innerExpressionResult is Parenthesized resultParentesized)
                {
                    resultDataObject = resultParentesized.Target as IDataObject;
                }

                var typeConversionResult = _typeConversionTransformer
                    .ImplementTypeConversion(fromVhdlType, toVhdlType, innerExpressionResult);
                if (typeConversionResult.IsLossy)
                {
                    scope.Warnings.AddWarning(
                        "LossyCast",
                        "A cast from " + fromVhdlType.ToVhdl() + " to " + toVhdlType.ToVhdl() +
                        " was lossy. If the result can indeed reach values outside the target type's limits then underflow or overflow errors will occur. The affected expression: " +
                        expression.ToString() + " in method " + scope.Method.GetFullName() + ".");
                }

                return typeConversionResult.ConvertedFromExpression;
            }
            else if (expression is ArrayCreateExpression arrayCreateExpression)
            {
                return _arrayCreateExpressionTransformer.Transform(arrayCreateExpression, context);
            }
            else if (expression is IndexerExpression indexerExpression)
            {
                if (!(Transform(indexerExpression.Target, context) is IDataObject targetVariableReference))
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
                            _typeConverter.ConvertType(indexExpression.GetActualType(), context.TransformationContext),
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
                if (!string.IsNullOrEmpty(constructorFullName) &&
                    context.TransformationContext.TypeDeclarationLookupTable
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

                    // Creating a clone of the expression's sub-tree where object creation is replaced to make the fake
                    // InvocationExpression realistic. A clone is needed not to cause concurrency issues if the same
                    // expression is processed on multiple threads for multiple hardware copies.
                    var expressionName = expression.GetFullName();

                    var subTreeClone = expression.FindFirstParentEntityDeclaration().Clone();
                    var objectCreateExpressionClone = subTreeClone
                        .FindFirstChildOfType<ObjectCreateExpression>(cloneExpression => cloneExpression.GetFullName() == expressionName);
                    objectCreateExpressionClone.ReplaceWith(constructorInvocation);

                    Transform(constructorInvocation, context);
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
            // Objects are mimicked with records and those don't need instantiation. However it's useful to initialize
            // all record fields to their default or initial values (otherwise if e.g. a class is instantiated in a
            // loop in the second run the old values could be accessed in VHDL).

            var typeDeclaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(recordAstType);

            if (typeDeclaration == null) ExceptionHelper.ThrowDeclarationNotFoundException(recordAstType.GetFullName(), expression);

            var record = _recordComposer.CreateRecordFromType(typeDeclaration, context.TransformationContext);

            var result = new RecordInitializationResult { Record = record };

            if (record.Fields.Any())
            {
                context.Scope.CurrentBlock.Add(new LineComment("Initializing record fields to their defaults."));

                var parentAssignment = expression
                    .FindFirstParentOfType<AssignmentExpression>(assignment => assignment.Right == expression);

                // This will only work if the newly created object is assigned to a variable or something else. It
                // won't work if the newly created object is directly passed to a method for example. However
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

                    // Initializing fields with their explicit defaults.
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

                    // Initializing properties with their explicit defaults.
                    else if (typeDeclaration.Members
                        .SingleOrDefault(member =>
                            member.Is<PropertyDeclaration>(p =>
                                p.Name == field.Name.TrimExtendedVhdlIdDelimiters())) is PropertyDeclaration propertyDeclaration)
                    {
                        if (propertyDeclaration.Initializer != Expression.Null)
                        {
                            initializationValue = Transform(propertyDeclaration.Initializer, context) as Value;
                        }
                    }

                    if (initializationValue != null)
                    {
                        IVhdlElement initializerExpression = initializationValue;

                        // In C# the default value can be e.g. an integer literal for an ushort field. So we need to
                        // take care of that.
                        if (field.DataType != initializationValue.DataType)
                        {
                            initializerExpression = _typeConversionTransformer.ImplementTypeConversion(
                                initializationValue.DataType,
                                field.DataType,
                                initializerExpression).ConvertedFromExpression;
                        }

                        context.Scope.CurrentBlock.Add(new Assignment
                        {
                            AssignTo = new RecordFieldAccess
                            {
                                Instance = result.RecordInstanceReference,
                                FieldName = field.Name
                            },
                            Expression = initializerExpression
                        });
                    }
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
