﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
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
    public interface IExpressionTransformer : IDependency
    {
        /// <summary>
        /// Transforms an expression into a VHDL element that can be used in place of the original expression. Be aware
        /// that <code>currentBlock</code>, being a reference, can change.
        /// </summary>
        IVhdlElement Transform(Expression expression, ISubTransformerContext context);
    }


    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;

        public ILogger Logger { get; set; }


        public ExpressionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;

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

                return ImplementTypeConversionForBinaryExpression((BinaryOperatorExpression)identifier.Parent, reference, context);

            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = (PrimitiveExpression)expression;

                var typeReference = expression.GetActualType();
                if (typeReference != null)
                {
                    var type = _typeConverter.ConvertTypeReference(typeReference);
                    var valueString = primitive.Value.ToString();
                    if (type.TypeCategory == DataTypeCategory.Numeric) valueString = valueString.Replace(',', '.'); // Replacing decimal comma to decimal dot.

                    // If a constant value of type real doesn't contain a decimal separator then it will be detected as integer and a type conversion would
                    // be needed. Thus we add a .0 to the end to indicate it's a real.
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
                    return ImplementTypeConversionForBinaryExpression((BinaryOperatorExpression)primitive.Parent, reference, context);
                }

                throw new InvalidOperationException(
                    "The type of the following primitive expression couldn't be determined: " +
                    expression.ToString());
            }
            else if (expression is BinaryOperatorExpression) return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context);
            else if (expression is InvocationExpression) return TransformInvocationExpression((InvocationExpression)expression, context);
            // These are not needed at the moment. MemberReferenceExpression is handled in TransformInvocationExpression and a
            // ThisReferenceExpression can only happen if "this" is passed to a method, which is not supported.
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
                    memoryOperationFinishedBlock.Body.Add(new VhdlBuilder.Representation.Declaration.Comment("Write finish"));

                    currentBlock.Add(memoryOperationInvokation.Terminate());
                    currentBlock.ChangeBlock(memoryOperationFinishedBlock);

                    return Empty.Instance;
                }
                else
                {
                    // TODO: ReadEnable should be set in currentBlock and re-set in memoryOperationFinishedBlock.
                    currentBlock.Add(new VhdlBuilder.Representation.Declaration.Comment("ReadEnable"));

                    // Looking up the type information that will tell us what the return type of the memory read is. 
                    // This might be some nodes up if e.g. there is an immediate cast expression.
                    AstNode currentNode = expression;
                    while (currentNode.Annotation<TypeInformation>() == null)
                    {
                        currentNode = currentNode.Parent;
                    }


                    currentBlock.ChangeBlock(memoryOperationFinishedBlock);

                    // If this is a memory read then comes the juggling with funneling the out parameter of the memory 
                    // write procedure to the original location.
                    var returnReference = BuildProcedureReturnReference(
                        target,
                        _typeConverter.ConvertTypeReference(currentNode.GetActualType()),
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
                var indexedStateMachineName = MethodStateMachine.CreateStateMachineName(targetStateMachineName, i);
                var startVariableReference = MethodStateMachine
                    .CreateStartVariableName(indexedStateMachineName)
                    .ToVhdlVariableReference();

                var trueBlock = new InlineBlock();

                trueBlock.Body.Add(new Assignment
                {
                    AssignTo = startVariableReference,
                    Expression = Value.True
                }.Terminate());

                trueBlock.Body.Add(new Assignment
                {
                    AssignTo = stateMachineRunningIndexVariable.ToReference(),
                    Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() }
                }.Terminate());

                var methodParametersEnumerator = ((MethodDeclaration)targetDeclaration).Parameters.GetEnumerator();
                methodParametersEnumerator.MoveNext();
                foreach (var parameter in transformedParameters)
                {
                    trueBlock.Body.Add(new Assignment
                    {
                        AssignTo =
                            MethodStateMachine.CreatePrefixedVariableName(indexedStateMachineName, methodParametersEnumerator.Current.Name)
                            .ToVhdlVariableReference(),
                        Expression = parameter
                    }.Terminate());
                    methodParametersEnumerator.MoveNext();
                }

                var elseBlock = new InlineBlock();

                stateMachineSelectingConditionsBlock.Body.Add(new IfElse
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

            // Common variable to signal that the invoked state machine finished.
            var stateMachineFinishedVariableName = GetNextUnusedTemporaryVariableName(targetStateMachineName, "finished", context);
            var stateMachineFinishedVariableReference = stateMachineFinishedVariableName.ToVhdlVariableReference();
            stateMachine.LocalVariables.Add(new Variable
            {
                Name = stateMachineFinishedVariableName,
                DataType = KnownDataTypes.Boolean
            });

            var isInvokedStateMachineFinishedIfElseTrue = new InlineBlock(new[]
            {
                new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.False }.Terminate()
            });

            var waitForInvokedStateMachineToFinishState = new InlineBlock();

            // Check if the running state machine finished.
            var stateMachineFinishedCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
            waitForInvokedStateMachineToFinishState.Body.Add(stateMachineFinishedCheckCase);
            for (int i = 0; i < maxCallStackDepth; i++)
            {
                var finishedVariableName = MethodStateMachine
                    .CreateFinishedVariableName(MethodStateMachine.CreateStateMachineName(targetStateMachineName, i));

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

            waitForInvokedStateMachineToFinishState.Body.Add(new IfElse
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


            // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to 
            // a non-void method).
            if (!(expression.Parent is ExpressionStatement))
            {
                // TODO: read out the return variable here.
            }

            currentBlock.ChangeBlock(isInvokedStateMachineFinishedIfElseTrue);
            return Empty.Instance;
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

            return ImplementTypeConversion(fromType, toType, Transform(expression.Expression, context));
        }

        /// <summary>
        /// In VHDL the operands of binary operations should have the same type, so we need to do a type conversion if 
        /// necessary.
        /// </summary>
        private IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            IVhdlElement expression,
            ISubTransformerContext context)
        {
            // If the type of an operand can't be determined the best guess is the expression's type.
            var expressionTypeReference = binaryOperatorExpression.GetActualType();
            var expressionType = expressionTypeReference != null ? _typeConverter.ConvertTypeReference(expressionTypeReference) : null;

            var leftTypeReference = binaryOperatorExpression.Left.GetActualType();
            var leftType = leftTypeReference != null ? _typeConverter.ConvertTypeReference(leftTypeReference) : expressionType;

            var rightTypeReference = binaryOperatorExpression.Right.GetActualType();
            var rightType = rightTypeReference != null ? _typeConverter.ConvertTypeReference(rightTypeReference) : expressionType;

            if (leftType == null || rightType == null)
            {
                throw new InvalidOperationException(
                    "The type of the operands of the following expression could't be determined: " +
                    binaryOperatorExpression.ToString());
            }

            if (leftType == rightType) return expression;

            var isLeft = binaryOperatorExpression.Left == expression;
            var thisType = isLeft ? leftType : rightType;
            var otherType = isLeft ? rightType : leftType;

            // We need to convert types in a way to keep precision. E.g. conerting an int to real is fine, but vica 
            // versa would cause information loss. However excplicit casting in this direction is allowed in CIL so we 
            // need to allow it here as well.
            if (!((thisType == KnownDataTypes.UnrangedInt || thisType == KnownDataTypes.Natural) && otherType == KnownDataTypes.Real))
            {
                Logger.Warning(
                    "Converting from " + thisType.Name +
                    " to " + otherType.Name +
                    " to fix a binary expression. Although valid in .NET this could cause information loss due to rounding. " +
                    "The affected expression: " + binaryOperatorExpression.ToString() +
                    " in method " + context.Scope.Method.GetFullName() + ".");
            }

            return ImplementTypeConversion(thisType, otherType, expression);
        }

        private IVhdlElement ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement expression)
        {
            if (fromType == toType)
            {
                return expression;
            }

            var castInvokation = new Invokation();

            // Trying supported cast scenarios:
            if ((fromType == KnownDataTypes.UnrangedInt || fromType == KnownDataTypes.Natural) && toType == KnownDataTypes.Real)
            {
                castInvokation.Target = new Raw("real");
            }
            else if ((fromType == KnownDataTypes.Real || fromType == KnownDataTypes.Natural) && (toType == KnownDataTypes.UnrangedInt || toType == KnownDataTypes.Natural))
            {
                castInvokation.Target = new Raw("integer");
            }
            else if (fromType == KnownDataTypes.UnrangedInt && toType == KnownDataTypes.Natural)
            {
                castInvokation.Target = new Raw("natural");
            }
            else
            {
                throw new NotSupportedException("Casting from " + fromType.Name + " to " + toType.Name + " is not supported.");
            }

            castInvokation.Parameters.Add(expression);

            return castInvokation;
        }


        /// <summary>
        /// Making sure that the e.g. return variable names are unique per method call (to transfer procedure outputs).
        /// </summary>
        private static string GetNextUnusedTemporaryVariableName(string targetName, string suffix, ISubTransformerContext context)
        {
            targetName = targetName.TrimExtendedVhdlIdDelimiters();

            var procedure = context.Scope.StateMachine;

            var variableName = (targetName + "." + suffix + "0").ToExtendedVhdlId();
            var returnVariableNameIndex = 0;
            while (context.Scope.StateMachine.LocalVariables.Any(variable => variable.Name == variableName))
            {
                variableName = (targetName + "." + suffix + ++returnVariableNameIndex).ToExtendedVhdlId();
            }

            return variableName;
        }

        /// <summary>
        /// Procedures can't just be assigned to variables like methods as they don't have a return value, just out 
        /// parameters. Thus here we create a variable for the out parameter (the return variable), run the procedure
        /// with it and then use it later too.
        /// </summary>
        private static DataObjectReference BuildProcedureReturnReference(
            string targetName,
            DataType returnType,
            Invokation invokation,
            ISubTransformerContext context)
        {
            var returnVariable = new Variable
            {
                Name = GetNextUnusedTemporaryVariableName(targetName, "return", context),
                DataType = returnType
            };

            context.Scope.StateMachine.LocalVariables.Add(returnVariable);
            invokation.Parameters.Add(returnVariable.ToReference());

            // Adding the procedure invokation directly to the body so it's before the current expression...
            context.Scope.CurrentBlock.Add(invokation.Terminate());

            // ...and using the return variable in place of the original call.
            return returnVariable.ToReference();
        }
    }
}
