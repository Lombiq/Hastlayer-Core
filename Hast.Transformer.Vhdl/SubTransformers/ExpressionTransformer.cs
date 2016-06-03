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
using Hast.Transformer.Vhdl.Helpers;
using Hast.Common.Configuration;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;
        private readonly IInvocationExpressionTransformer _invocationExpressionTransformer;
        private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;
        private readonly IDeviceDriver _deviceDriver;
        private readonly IBinaryOperatorExpressionTransformer _binaryOperatorExpressionTransformer;
        private readonly IStateMachineInvocationBuilder _stateMachineInvocationBuilder;

        public ILogger Logger { get; set; }


        public ExpressionTransformer(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer,
            IInvocationExpressionTransformer invocationExpressionTransformer,
            IArrayCreateExpressionTransformer arrayCreateExpressionTransformer,
            IDeviceDriver deviceDriver,
            IBinaryOperatorExpressionTransformer binaryOperatorExpressionTransformer,
            IStateMachineInvocationBuilder stateMachineInvocationBuilder)
        {
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
            _invocationExpressionTransformer = invocationExpressionTransformer;
            _arrayCreateExpressionTransformer = arrayCreateExpressionTransformer;
            _deviceDriver = deviceDriver;
            _binaryOperatorExpressionTransformer = binaryOperatorExpressionTransformer;
            _stateMachineInvocationBuilder = stateMachineInvocationBuilder;

            Logger = NullLogger.Instance;
        }


        public IVhdlElement Transform(Expression expression, ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;


            Func<DataObjectReference, IVhdlElement> implementTypeConversionForBinaryExpressionParent = reference =>
            {
                var binaryExpression = (BinaryOperatorExpression)expression.Parent;
                return _typeConversionTransformer.
                    ImplementTypeConversionForBinaryExpression(
                        binaryExpression,
                        reference,
                        binaryExpression.Left == expression);
            };


            if (expression is AssignmentExpression)
            {
                var assignment = (AssignmentExpression)expression;

                Func<Expression, Expression, IVhdlElement> transformSimpleAssignmentExpression = (left, right) =>
                {
                    var leftTransformed = Transform(left, context);
                    if (leftTransformed == Empty.Instance) return Empty.Instance;
                    var rightTransformed = Transform(right, context);
                    if (rightTransformed == Empty.Instance) return Empty.Instance;

                    return new Assignment
                    {
                        AssignTo = (IDataObject)leftTransformed,
                        Expression = rightTransformed
                    };
                };

                Func<string> getTaskVariableIdentifier = () =>
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
                };

                Func<EntityDeclaration, int> getMaxDegreeOfParallelism = entity =>
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
                    var scope = context.Scope;

                    // Handling TPL-related DisplayClass instantiation (created in place of lambda delegates). These will 
                    // be like following: <>c__DisplayClass9_ = new PrimeCalculator.<>c__DisplayClass9_0();
                    var rightObjectCreateExpression = assignment.Right as ObjectCreateExpression;
                    string rightObjectFullName;
                    if (rightObjectCreateExpression != null &&
                        (rightObjectFullName = rightObjectCreateExpression.Type.GetFullName()).IsDisplayClassName())
                    {
                        context.TransformationContext.TypeDeclarationLookupTable.Lookup(rightObjectCreateExpression.Type);
                        var leftIdentifierExpression = assignment.Left as IdentifierExpression;

                        if (leftIdentifierExpression != null)
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
                    else if (assignment.Right.Is<InvocationExpression>(invocation =>
                        invocation.Target.Is<MemberReferenceExpression>(member =>
                            member.MemberName == "StartNew" &&
                            member.Target.Is<IdentifierExpression>(identifier =>
                                scope.TaskFactoryVariableNames.Contains(identifier.Identifier)))))
                    {
                        var taskStartArguments = ((InvocationExpression)assignment.Right).Arguments;

                        // The first argument is always for the Func that referes to the method on the DisplayClass
                        // generated for the lambda expression originally passed to the Task factory.
                        var funcVariablename = ((IdentifierExpression)taskStartArguments.First()).Identifier;
                        var targetMethod = scope.FuncVariableNameToDisplayClassMethodMappings[funcVariablename];

                        // We only need to care about he invocation here. Since this is a Task start there will be
                        // some form of await later.
                        _stateMachineInvocationBuilder.BuildInvocation(
                            targetMethod,
                            taskStartArguments.Skip(1).Select(argument => Transform(argument, context)),
                            getMaxDegreeOfParallelism(targetMethod),
                            context);

                        scope.TaskVariableNameToDisplayClassMethodMappings[getTaskVariableIdentifier()] =
                            targetMethod;

                        return Empty.Instance;
                    }
                    // Handling shorthand Task starts like:
                    // array[i] = Task.Factory.StartNew<bool>(new Func<object, bool> (this.<ParallelizedArePrimeNumbers2>b__9_0), num3);
                    else if (assignment.Right.Is<InvocationExpression>(invocation =>
                        invocation.Target.Is<MemberReferenceExpression>(memberReference =>
                            memberReference.MemberName == "StartNew" &&
                            memberReference.Target.GetFullName().Contains("System.Threading.Tasks.Task::Factory")) &&
                        invocation.Arguments.First().Is<ObjectCreateExpression>(objectCreate =>
                            objectCreate.Type.GetFullName().Contains("Func"))))
                    {
                        var inovocationExpression = (InvocationExpression)assignment.Right;
                        var arguments = inovocationExpression.Arguments;

                        var targetMethod = TaskParallelizationHelper
                            .GetTargetDisplayClassMemberFromFuncCreation((ObjectCreateExpression)arguments.First())
                            .GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);
                        var targetMaxDegreeOfParallelism = context.TransformationContext
                            .GetTransformerConfiguration()
                            .GetMaxInvocationInstanceCountConfigurationForMember(targetMethod)
                            .MaxDegreeOfParallelism;

                        // We only need to care about he invocation here. Since this is a Task start there will be
                        // some form of await later.
                        _stateMachineInvocationBuilder.BuildInvocation(
                            targetMethod,
                            arguments.Skip(1).Select(argument => Transform(argument, context)),
                            getMaxDegreeOfParallelism(targetMethod),
                            context);

                        scope.TaskVariableNameToDisplayClassMethodMappings[getTaskVariableIdentifier()] = targetMethod;

                        return Empty.Instance;
                    }
                }

                return transformSimpleAssignmentExpression(assignment.Left, assignment.Right);
            }
            else if (expression is IdentifierExpression)
            {
                var identifierExpression = (IdentifierExpression)expression;
                var reference = stateMachine.CreatePrefixedObjectName(identifierExpression.Identifier).ToVhdlVariableReference();

                if (!(identifierExpression.Parent is BinaryOperatorExpression)) return reference;

                return implementTypeConversionForBinaryExpressionParent(reference);

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

                    return valueString.ToVhdlValue(type);
                }
                else if (primitive.Parent is BinaryOperatorExpression)
                {
                    var reference = new DataObjectReference
                    {
                        DataObjectKind = DataObjectKind.Constant,
                        Name = primitive.ToString()
                    };

                    return implementTypeConversionForBinaryExpressionParent(reference);
                }

                throw new InvalidOperationException(
                    "The type of the following primitive expression couldn't be determined: " +
                    expression.ToString());
            }
            else if (expression is BinaryOperatorExpression)
            {
                var binaryExpression = (BinaryOperatorExpression)expression;
                return _binaryOperatorExpressionTransformer.TransformBinaryOperatorExpression(
                    binaryExpression,
                    Transform(binaryExpression.Left, context),
                    Transform(binaryExpression.Right, context),
                    context);
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

                return _invocationExpressionTransformer
                    .TransformInvocationExpression(invocationExpression, transformedParameters, context);
            }
            else if (expression is MemberReferenceExpression)
            {
                var memberReference = (MemberReferenceExpression)expression;
                var memberFullName = memberReference.GetFullName();

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
                    string displayClassName;
                    if (context.Scope.VariableNameToDisplayClassNameMappings.TryGetValue(targetIdentifier, out displayClassName))
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

                // Is this a Task result access like array[k].Result? We know that we've already handled the target so
                // it stores the result objects, so just need to use them directly.
                if (memberReference.Target.GetActualTypeReference().FullName.StartsWith("System.Threading.Tasks.Task") &&
                    memberReference.MemberName == "Result")
                {
                    return Transform(memberReference.Target, context);
                }


                throw new NotSupportedException("Transformation of the member reference expression " + memberReference + " is not supported.");
            }
            // Not needed at the moment since ThisReferenceExpression can only happen if "this" is passed to a method, 
            // which is not supported
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
                        return new Invocation
                        {
                            Target = "not".ToVhdlValue(KnownDataTypes.Identifier),
                            Parameters = new List<IVhdlElement> { transformedExpression }
                        };
                    case UnaryOperatorType.Plus:
                        return new Unary
                        {
                            Operator = UnaryOperator.Identity,
                            Expression = transformedExpression
                        };
                    case UnaryOperatorType.Await:
                        if (unary.Expression is InvocationExpression &&
                            unary.Expression.GetFullName().IsTaskFromResultMethodName())
                        {
                            return transformedExpression;
                        }

                        // Otherwise nothing to do.
                        goto default;
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

                return declaration.GetFullName().ToVhdlValue(KnownDataTypes.Identifier);
            }
            else if (expression is CastExpression)
            {
                var castExpression = (CastExpression)expression;

                var fromType = _typeConverter.ConvertTypeReference(
                    castExpression.Expression.GetActualTypeReference() ?? castExpression.GetActualTypeReference());
                var toType = _typeConverter.ConvertAstType(castExpression.Type);

                var typeConversionResult = _typeConversionTransformer
                    .ImplementTypeConversion(fromType, toType, Transform(castExpression.Expression, context));
                if (typeConversionResult.IsLossy)
                {
                    var toTypeKeyword = ((PrimitiveType)castExpression.Type).Keyword;
                    var fromTypeKeyword = castExpression.GetActualTypeReference().FullName;
                    Logger.Warning("A cast from " + fromTypeKeyword + " to " + toTypeKeyword + " was lossy. If the result can indeed reach values outside the target type's limits then underflow or overflow errors will occur. The affected expression: " + expression.ToString() + " in method " + context.Scope.Method.GetFullName() + ".");
                }

                return typeConversionResult.Expression;
            }
            else if (expression is ArrayCreateExpression)
            {
                return _arrayCreateExpressionTransformer
                    .Transform((ArrayCreateExpression)expression, context.Scope.StateMachine);
            }
            else if (expression is IndexerExpression)
            {
                var indexerExpression = expression as IndexerExpression;

                var targetVariableReference = Transform(indexerExpression.Target, context) as IDataObject;

                if (targetVariableReference == null)
                {
                    throw new InvalidOperationException("The target of the indexer expression " + expression.ToString() + " couldn't be transformed to a data object reference.");
                }

                if (indexerExpression.Arguments.Count != 1)
                {
                    throw new NotSupportedException("Accessing elements of only single-dimensional arrays are supported.");
                }

                var indexExpression = indexerExpression.Arguments.Single();
                return new ArrayElementAccess
                {
                    Array = targetVariableReference,
                    IndexExpression = _typeConversionTransformer
                        .ImplementTypeConversion(
                            _typeConverter.ConvertTypeReference(indexExpression.GetActualTypeReference()),
                            KnownDataTypes.UnrangedInt,
                            Transform(indexExpression, context))
                        .Expression
                };
            }
            else if (expression is ParenthesizedExpression)
            {
                var parenthesizedExpression = (ParenthesizedExpression)expression;

                return new Parenthesized
                {
                    Target = Transform(parenthesizedExpression.Expression, context)
                };
            }
            else
            {
                throw new NotSupportedException(
                    "Expressions of type " +
                    expression.GetType() + " are not supported. The expression was: " +
                    expression.ToString());
            }
        }
    }
}
