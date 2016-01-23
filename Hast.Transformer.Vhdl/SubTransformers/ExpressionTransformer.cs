﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Synthesis;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using Orchard.Logging;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;
        private readonly IDeviceDriver _deviceDriver;

        public ILogger Logger { get; set; }


        public ExpressionTransformer(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer,
            IDeviceDriver deviceDriver)
        {
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
            _deviceDriver = deviceDriver;

            Logger = NullLogger.Instance;
        }


        public IVhdlElement Transform(Expression expression, ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;

            if (expression is AssignmentExpression)
            {
                var assignment = (AssignmentExpression)expression;
                return new Assignment
                {
                    AssignTo = (IDataObject)Transform(assignment.Left, context),
                    Expression = Transform(assignment.Right, context)
                };
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = (IdentifierExpression)expression;
                var reference = stateMachine.CreatePrefixedVariableName(identifier.Identifier).ToVhdlVariableReference();

                if (!(identifier.Parent is BinaryOperatorExpression)) return reference;

                return _typeConversionTransformer
                    .ImplementTypeConversionForBinaryExpression((BinaryOperatorExpression)identifier.Parent, reference, context);

            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = (PrimitiveExpression)expression;

                var typeReference = expression.GetActualType();
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
            else if (expression is BinaryOperatorExpression) return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context);
            else if (expression is InvocationExpression) return TransformInvocationExpression((InvocationExpression)expression, context);
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
                return new Invokation
                {
                    Target = new Value { DataType = KnownDataTypes.Identifier, Content = "not" },
                    Parameters = new List<IVhdlElement> { Transform(unary.Expression, context) }
                };
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

            //  Would need to decide between + and & or sll/srl and sra/sla
            // See: http://www.csee.umbc.edu/portal/help/VHDL/operator.html
            switch (expression.Operator)
            {
                case BinaryOperatorType.Add:
                    binary.Operator = "+";
                    break;
                case BinaryOperatorType.Any:
                    break;
                case BinaryOperatorType.BitwiseAnd:
                    break;
                case BinaryOperatorType.BitwiseOr:
                    break;
                case BinaryOperatorType.ConditionalAnd:
                    break;
                case BinaryOperatorType.ConditionalOr:
                    break;
                case BinaryOperatorType.Divide:
                    binary.Operator = "/";
                    break;
                case BinaryOperatorType.Equality:
                    binary.Operator = "=";
                    break;
                case BinaryOperatorType.ExclusiveOr:
                    binary.Operator = "XOR";
                    break;
                case BinaryOperatorType.GreaterThan:
                    binary.Operator = ">";
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    binary.Operator = ">=";
                    break;
                case BinaryOperatorType.InEquality:
                    binary.Operator = "/=";
                    break;
                case BinaryOperatorType.LessThan:
                    binary.Operator = "<";
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    binary.Operator = "<=";
                    break;
                case BinaryOperatorType.Modulus:
                    binary.Operator = "mod";
                    break;
                case BinaryOperatorType.Multiply:
                    binary.Operator = "*";
                    break;
                case BinaryOperatorType.NullCoalescing:
                    break;
                case BinaryOperatorType.ShiftLeft:
                    binary.Operator = "sll";
                    break;
                case BinaryOperatorType.ShiftRight:
                    binary.Operator = "srl";
                    break;
                case BinaryOperatorType.Subtract:
                    binary.Operator = "-";
                    break;
            }

            return binary;
        }

        private IVhdlElement TransformInvocationExpression(InvocationExpression expression, ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

            var targetMemberReference = expression.Target as MemberReferenceExpression;
            var transformedParameters = new List<IVhdlElement>();
            foreach (var argument in expression.Arguments)
            {
                transformedParameters.Add(Transform(argument, context));
            }

            // This is a SimpleMemory access.
            if (context.TransformationContext.UseSimpleMemory() &&
                targetMemberReference != null &&
                targetMemberReference.Target is IdentifierExpression &&
                ((IdentifierExpression)targetMemberReference.Target).Identifier == context.Scope.Method.GetSimpleMemoryParameterName())
            {
                var memberName = targetMemberReference.MemberName;

                var isWrite = memberName.StartsWith("Write");
                var invokationParameters = transformedParameters;
                invokationParameters.AddRange(new[]
                {
                    new DataObjectReference
                    {
                        DataObjectKind = DataObjectKind.Signal, 
                        Name = isWrite ? SimpleMemoryNames.DataOutLocal : SimpleMemoryNames.DataInLocal
                    },
                    new DataObjectReference
                    {
                        DataObjectKind = DataObjectKind.Signal, 
                        Name = isWrite ? SimpleMemoryNames.WriteAddressLocal : SimpleMemoryNames.ReadAddressLocal
                    }
                });

                var target = "SimpleMemory" + targetMemberReference.MemberName;
                var memoryOperationInvokation = new Invokation
                {
                    Target = new Value { Content = target },
                    Parameters = invokationParameters
                };

                // The memory operation should be initialized in this state, then finished in another one.
                var memoryOperationFinishedBlock = new InlineBlock();

                currentBlock.Add(stateMachine.CreateStateChange(stateMachine.AddState(memoryOperationFinishedBlock)));

                if (isWrite)
                {
                    // TODO: WriteEnable and CellIndex should be set in currentBlock and be re-set in
                    // memoryOperationFinishedBlock.
                    memoryOperationFinishedBlock.Add(new VhdlBuilder.Representation.Declaration.Comment("Write finish"));

                    currentBlock.Add(memoryOperationInvokation.Terminate());
                    currentBlock.ChangeBlock(memoryOperationFinishedBlock);

                    return Empty.Instance;
                }
                else
                {
                    // TODO: ReadEnable should be set in currentBlock and re-set in memoryOperationFinishedBlock.
                    currentBlock.Add(new VhdlBuilder.Representation.Declaration.Comment("ReadEnable"));

                    currentBlock.ChangeBlock(memoryOperationFinishedBlock);

                    // If this is a memory read then comes the juggling with funneling the out parameter of the memory 
                    // write procedure to the original location.
                    var returnReference = CreateProcedureReturnReference(
                        target,
                        _typeConverter.ConvertTypeReference(expression.GetReturnType()),
                        memoryOperationInvokation,
                        context);

                    return returnReference;
                }
            }


            var targetMethodName = expression.GetFullName();
            var targetStateMachineName = targetMethodName;
            var targetStateMachineVhdlId = targetStateMachineName.ToExtendedVhdlId();

            var targetDeclaration = targetMemberReference.GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);

            if (targetDeclaration == null || !(targetDeclaration is MethodDeclaration))
            {
                throw new InvalidOperationException("The invoked method " + targetMethodName + " can't be found.");
            }

            context.TransformationContext.MemberCallChainTable.AddTarget(context.Scope.StateMachine.Name, targetStateMachineVhdlId);


            // Since .NET methods can be recursive but a hardware state machine can only have one "instance" we need to
            // have multiple state machines with the same logic. This way even with recursive calls there will always be
            // an idle, usable state machine (these state machines are distinguished by an index).

            var stateMachineRunningIndexVariableName = GetNextUnusedTemporaryVariableName(targetStateMachineName, "runningIndex", context);
            var stateMachineRunningIndexVariable = new Variable
            {
                Name = stateMachineRunningIndexVariableName,
                DataType = new RangedDataType
                {
                    TypeCategory = DataTypeCategory.Numeric,
                    Name = "integer",
                    RangeMin = 0,
                    RangeMax = 32767
                }
            };
            stateMachine.LocalVariables.Add(stateMachineRunningIndexVariable);

            // Logic for determining which state machine is idle and thus can be invoked. We probe every state machine.
            // If we can't find any idle one that is an issue, we should probably have a fail safe for that somehow.
            var maxCallStackDepth = context.TransformationContext.GetTransformerConfiguration().MaxCallStackDepth;
            var stateMachineSelectingConditionsBlock = new InlineBlock();
            currentBlock.Add(stateMachineSelectingConditionsBlock);
            for (int i = 0; i < maxCallStackDepth; i++)
            {
                var indexedStateMachineName = MethodStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i);
                var startVariableReference = MethodStateMachineNameFactory
                    .CreateStartVariableName(indexedStateMachineName)
                    .ToVhdlVariableReference();

                var trueBlock = new InlineBlock();

                trueBlock.Add(new Assignment
                {
                    AssignTo = startVariableReference,
                    Expression = Value.True
                }.Terminate());

                trueBlock.Add(new Assignment
                {
                    AssignTo = stateMachineRunningIndexVariable.ToReference(),
                    Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() }
                }.Terminate());

                var methodParametersEnumerator = ((MethodDeclaration)targetDeclaration).Parameters.GetEnumerator();
                methodParametersEnumerator.MoveNext();
                foreach (var parameter in transformedParameters)
                {
                    trueBlock.Add(new Assignment
                    {
                        AssignTo =
                            MethodStateMachineNameFactory.CreatePrefixedVariableName(indexedStateMachineName, methodParametersEnumerator.Current.Name)
                            .ToVhdlVariableReference(),
                        Expression = parameter
                    }.Terminate());
                    methodParametersEnumerator.MoveNext();
                }

                var elseBlock = new InlineBlock();

                stateMachineSelectingConditionsBlock.Add(new IfElse
                {
                    Condition = new Binary
                    {
                        Left = startVariableReference,
                        Operator = "=",
                        Right = Value.False
                    },
                    True = trueBlock,
                    Else = elseBlock
                });

                stateMachineSelectingConditionsBlock = elseBlock;
            }

            stateMachineSelectingConditionsBlock.Add(new VhdlBuilder.Representation.Declaration.Comment(
                "No idle state machine could be found. This is an error."));

            // Common variable to signal that the invoked state machine finished.
            var stateMachineFinishedVariableName = GetNextUnusedTemporaryVariableName(targetStateMachineName, "finished", context);
            var stateMachineFinishedVariableReference = stateMachineFinishedVariableName.ToVhdlVariableReference();
            stateMachine.LocalVariables.Add(new Variable
            {
                Name = stateMachineFinishedVariableName,
                DataType = KnownDataTypes.Boolean
            });

            var isInvokedStateMachineFinishedIfElseTrue = new InlineBlock(
                new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.False }.Terminate());

            var waitForInvokedStateMachineToFinishState = new InlineBlock();

            // Check if the running state machine finished.
            var stateMachineFinishedCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
            waitForInvokedStateMachineToFinishState.Add(stateMachineFinishedCheckCase);
            for (int i = 0; i < maxCallStackDepth; i++)
            {
                var finishedVariableName = MethodStateMachineNameFactory
                    .CreateFinishedVariableName(MethodStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i));

                stateMachineFinishedCheckCase.Whens.Add(new When
                {
                    Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() },
                    Body = new List<IVhdlElement>
                    {
                        new IfElse
                        {
                            Condition = new Binary
                            {
                                Left = finishedVariableName.ToVhdlVariableReference(),
                                Operator = "=",
                                Right = Value.True
                            },
                            True = new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.True }.Terminate()
                        }
                    }
                });
            }

            waitForInvokedStateMachineToFinishState.Add(new IfElse
            {
                Condition = new Binary
                {
                    Left = stateMachineFinishedVariableReference,
                    Operator = "=",
                    Right = Value.True
                },
                True = isInvokedStateMachineFinishedIfElseTrue
            });

            var waitForInvokedStateMachineToFinishStateIndex = stateMachine.AddState(waitForInvokedStateMachineToFinishState);
            currentBlock.Add(stateMachine.CreateStateChange(waitForInvokedStateMachineToFinishStateIndex));

            currentBlock.ChangeBlock(isInvokedStateMachineFinishedIfElseTrue);

            // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to 
            // a non-void method).
            if (!(expression.Parent is ExpressionStatement))
            {
                // We copy the used state machine's return value to a local variable and then use the local variable in
                // place of the original method call.

                var localReturnVariableReference = CreateTemporaryVariableReference(
                    targetStateMachineName,
                    "return",
                    _typeConverter.ConvertTypeReference(expression.GetReturnType()),
                    context);

                var stateMachineReadReturnValueCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
                isInvokedStateMachineFinishedIfElseTrue.Add(stateMachineReadReturnValueCheckCase);
                for (int i = 0; i < maxCallStackDepth; i++)
                {
                    var returnVariableName = MethodStateMachineNameFactory
                        .CreateReturnVariableName(MethodStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i));

                    stateMachineReadReturnValueCheckCase.Whens.Add(new When
                    {
                        Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() },
                        Body = new List<IVhdlElement>
                        {
                            new Assignment
                            {
                                AssignTo = localReturnVariableReference,
                                Expression = returnVariableName.ToVhdlVariableReference()
                            }.Terminate()
                        }
                    });
                }

                return localReturnVariableReference;
            }
            else
            {
                return Empty.Instance;
            }
        }

        private IVhdlElement TransformCastExpression(CastExpression expression, ISubTransformerContext context)
        {
            // This is a temporary workaround to get around cases where operations (e.g. multiplication) with 32b numbers
            // resulting in a 64b number would cause a cast to a 64b number type (what we don't support yet). 
            // See: https://lombiq.atlassian.net/browse/HAST-20
            var toTypeKeyword = ((PrimitiveType)expression.Type).Keyword;
            var fromTypeKeyword = expression.GetActualType().FullName;
            if (toTypeKeyword == "long" || toTypeKeyword == "ulong" || fromTypeKeyword == "System.Int64" || fromTypeKeyword == "System.UInt64")
            {
                Logger.Warning("A cast from " + fromTypeKeyword + " to " + toTypeKeyword + " was omitted because non-32b numbers are not yet supported. If the result can indeed reach values above the 32b limit then overflow errors will occur. The affected expression: " + expression.ToString() + " in method " + context.Scope.Method.GetFullName() + ".");
                return Transform(expression.Expression, context);
            }

            var fromType = _typeConverter.ConvertTypeReference(expression.GetActualType());
            var toType = _typeConverter.Convert(expression.Type);

            return _typeConversionTransformer.ImplementTypeConversion(fromType, toType, Transform(expression.Expression, context));
        }


        /// <summary>
        /// Making sure that the e.g. return variable names are unique per method call (to transfer procedure outputs).
        /// </summary>
        private static string GetNextUnusedTemporaryVariableName(string targetName, string suffix, ISubTransformerContext context)
        {
            targetName = targetName.TrimExtendedVhdlIdDelimiters();

            var stateMachine = context.Scope.StateMachine;

            var variableName = (targetName + "." + suffix + "0");
            var returnVariableNameIndex = 0;
            while (context.Scope.StateMachine.LocalVariables.Any(variable => variable.Name == variableName.ToExtendedVhdlId()))
            {
                variableName = (targetName + "." + suffix + ++returnVariableNameIndex);
            }

            return MethodStateMachineNameFactory.CreatePrefixedVariableName(context.Scope.StateMachine, variableName);
        }

        private static DataObjectReference CreateTemporaryVariableReference(
            string targetName,
            string suffix,
            DataType dataType,
            ISubTransformerContext context)
        {
            var returnVariable = new Variable
            {
                Name = GetNextUnusedTemporaryVariableName(targetName, suffix, context),
                DataType = dataType
            };

            context.Scope.StateMachine.LocalVariables.Add(returnVariable);

            return returnVariable.ToReference();
        }

        /// <summary>
        /// Procedures can't just be assigned to variables like methods as they don't have a return value, just out 
        /// parameters. Thus here we create a variable for the out parameter (the return variable), run the procedure
        /// with it and then use it later too.
        /// </summary>
        private static DataObjectReference CreateProcedureReturnReference(
            string targetName,
            DataType returnType,
            Invokation invokation,
            ISubTransformerContext context)
        {
            var returnVariableReference = CreateTemporaryVariableReference(targetName, "return", returnType, context);

            invokation.Parameters.Add(returnVariableReference);

            // Adding the procedure invokation directly to the body so it's before the current expression...
            context.Scope.CurrentBlock.Add(invokation.Terminate());

            // ...and using the return variable in place of the original call.
            return returnVariableReference;
        }
    }
}
