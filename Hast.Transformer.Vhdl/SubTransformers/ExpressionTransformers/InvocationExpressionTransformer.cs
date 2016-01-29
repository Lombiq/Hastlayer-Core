using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.StateMachineGeneration;
using Hast.Common.Configuration;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class InvocationExpressionTransformer : IInvocationExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public InvocationExpressionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }
        
        
        public IVhdlElement TransformInvocationExpression(
            InvocationExpression expression,
            ISubTransformerContext context,
            IEnumerable<IVhdlElement> transformedParameters)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

            var targetMemberReference = expression.Target as MemberReferenceExpression;


            // This is a SimpleMemory access.
            if (context.TransformationContext.UseSimpleMemory() &&
                targetMemberReference != null &&
                targetMemberReference.Target is IdentifierExpression &&
                ((IdentifierExpression)targetMemberReference.Target).Identifier == context.Scope.Method.GetSimpleMemoryParameterName())
            {
                var memberName = targetMemberReference.MemberName;

                var isWrite = memberName.StartsWith("Write");
                var invokationParameters = transformedParameters.ToList();

                if (isWrite)
                {
                    currentBlock.Add(new LineComment("Begin SimpleMemory write."));
                }
                else
                {
                    currentBlock.Add(new LineComment("Begin SimpleMemory read."));
                }

                // Directly setting SimpleMemory ports since the SimpleMemory library doesn't handle these yet.
                // See: https://lombiq.atlassian.net/browse/HAST-44
                currentBlock.Add(new Assignment
                {
                    AssignTo = SimpleMemoryPortNames.CellIndexOut.ToVhdlSignalReference(),
                    Expression = invokationParameters[0] // CellIndex is conventionally the first invokation parameter.
                });
                invokationParameters.RemoveAt(0);
                var enablePortReference = (isWrite ? SimpleMemoryPortNames.WriteEnable : SimpleMemoryPortNames.ReadEnable)
                    .ToVhdlSignalReference();
                currentBlock.Add(new Assignment
                {
                    AssignTo = enablePortReference,
                    Expression = Value.OneCharacter
                });

                invokationParameters.AddRange(new[]
                {
                    (isWrite ? SimpleMemoryPortNames.DataOut : SimpleMemoryPortNames.DataIn).ToVhdlSignalReference()
                    // The SimpleMemory library doesn't handle the CellIndex yet, we need to set that directly.
                    //SimpleMemoryNames.CellIndexOutPort.ToVhdlSignalReference()
                });

                var target = "SimpleMemory" + targetMemberReference.MemberName;
                var memoryOperationInvokation = new Invokation
                {
                    Target = new Value { Content = target },
                    Parameters = invokationParameters
                };

                // The memory operation should be initialized in this state, then finished in another one.
                var memoryOperationFinishedBlock = new InlineBlock();
                var endMemoryOperationBlock = new InlineBlock(
                    new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = (isWrite ? SimpleMemoryPortNames.WritesDone : SimpleMemoryPortNames.ReadsDone).ToVhdlSignalReference(),
                            Operator = Operator.Equality,
                            Right = Value.OneCharacter
                        },
                        True = memoryOperationFinishedBlock
                    });
                var memoryOperationFinishedStateIndex = stateMachine.AddState(endMemoryOperationBlock);

                // Directly resetting SimpleMemory *Enable port since the SimpleMemory library doesn't handle these yet.
                memoryOperationFinishedBlock.Add(new Assignment
                {
                    AssignTo = enablePortReference,
                    Expression = Value.ZeroCharacter
                });

                currentBlock.Add(stateMachine.CreateStateChange(memoryOperationFinishedStateIndex));

                if (isWrite)
                {
                    memoryOperationFinishedBlock.Body.Insert(0, new LineComment("SimpleMemory write finished."));

                    currentBlock.Add(memoryOperationInvokation.Terminate());
                    currentBlock.ChangeBlockToDifferentState(memoryOperationFinishedBlock, memoryOperationFinishedStateIndex);

                    return Empty.Instance;
                }
                else
                {
                    memoryOperationFinishedBlock.Body.Insert(0, new LineComment("SimpleMemory read finished."));

                    currentBlock.ChangeBlockToDifferentState(memoryOperationFinishedBlock, memoryOperationFinishedStateIndex);

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

            var targetDeclaration = targetMemberReference.GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);

            if (targetDeclaration == null || !(targetDeclaration is MethodDeclaration))
            {
                throw new InvalidOperationException("The invoked method " + targetMethodName + " can't be found.");
            }


            // Since .NET methods can be recursive but a hardware state machine can only have one "instance" we need to
            // have multiple state machines with the same logic. This way even with recursive calls there will always be
            // an idle, usable state machine (these state machines are distinguished by an index).

            var maxRecursionDepth = context.TransformationContext.GetTransformerConfiguration()
                .GetMaxRecursionDepthForMember(targetDeclaration.GetSimpleName());


            var stateMachineRunningIndexVariableName = context.Scope.StateMachine
                .GetNextUnusedTemporaryVariableName(targetStateMachineName + "." + "runningIndex");
            var stateMachineRunningIndexVariable = new Variable
            {
                Name = stateMachineRunningIndexVariableName,
                DataType = new RangedDataType
                {
                    TypeCategory = DataTypeCategory.Numeric,
                    Name = "integer",
                    RangeMin = 0,
                    RangeMax = maxRecursionDepth - 1
                }
            };
            stateMachine.LocalVariables.Add(stateMachineRunningIndexVariable);

            // Logic for determining which state machine is idle and thus can be invoked. We probe every state machine.
            // If we can't find any idle one that is an issue, we should probably have a fail safe for that somehow.
            var stateMachineSelectingConditionsBlock = new InlineBlock();
            currentBlock.Add(stateMachineSelectingConditionsBlock);
            for (int i = 0; i < maxRecursionDepth; i++)
            {
                var indexedStateMachineName = MemberStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i);
                var startSignalName = stateMachine.CreatePrefixedObjectName(MemberStateMachineNameFactory
                    .CreateStartSignalName(indexedStateMachineName).TrimExtendedVhdlIdDelimiters());

                context.TransformationContext.MemberStateMachineStartSignalFunnel
                    .AddDrivingStartSignalForStateMachine(startSignalName, indexedStateMachineName);

                stateMachine.Signals.AddIfNew(new Signal
                    {
                        DataType = KnownDataTypes.Boolean,
                        Name = startSignalName,
                        InitialValue = Value.False
                    });

                var trueBlock = new InlineBlock();

                trueBlock.Add(new Assignment
                {
                    AssignTo = startSignalName.ToVhdlSignalReference(),
                    Expression = Value.True
                });

                trueBlock.Body.Add(new Assignment
                {
                    AssignTo = stateMachineRunningIndexVariable.ToReference(),
                    Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() }
                });

                var methodParametersEnumerator = ((MethodDeclaration)targetDeclaration).Parameters.GetEnumerator();
                methodParametersEnumerator.MoveNext();
                foreach (var parameter in transformedParameters)
                {
                    trueBlock.Add(new Assignment
                    {
                        AssignTo =
                            MemberStateMachineNameFactory.CreatePrefixedObjectName(indexedStateMachineName, methodParametersEnumerator.Current.Name)
                            .ToVhdlVariableReference(),
                        Expression = parameter
                    });
                    methodParametersEnumerator.MoveNext();
                }

                var elseBlock = new InlineBlock();

                stateMachineSelectingConditionsBlock.Add(new IfElse
                {
                    Condition = new Binary
                    {
                        // We need to check for the main start signal here, not this state machine's driving signal.
                        Left = MemberStateMachineNameFactory.CreateStartSignalName(indexedStateMachineName).ToVhdlSignalReference(),
                        Operator = Operator.Equality,
                        Right = Value.False
                    },
                    True = trueBlock,
                    Else = elseBlock
                });

                stateMachineSelectingConditionsBlock = elseBlock;
            }

            stateMachineSelectingConditionsBlock.Add(new LineComment("No idle state machine could be found. This is an error."));

            // Common variable to signal that the invoked state machine finished.
            var stateMachineFinishedVariableName = context.Scope.StateMachine
                .GetNextUnusedTemporaryVariableName(targetStateMachineName + "." + "finished");
            var stateMachineFinishedVariableReference = stateMachineFinishedVariableName.ToVhdlVariableReference();
            stateMachine.LocalVariables.Add(new Variable
            {
                Name = stateMachineFinishedVariableName,
                DataType = KnownDataTypes.Boolean
            });

            var isInvokedStateMachineFinishedIfElseTrue = new InlineBlock(
                new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.False });

            var waitForInvokedStateMachineToFinishState = new InlineBlock();

            // Check if the running state machine finished.
            var stateMachineFinishedCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
            waitForInvokedStateMachineToFinishState.Add(stateMachineFinishedCheckCase);
            for (int i = 0; i < maxRecursionDepth; i++)
            {
                var indexedStateMachineName = MemberStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i);
                var startSignalName = stateMachine.CreatePrefixedObjectName(MemberStateMachineNameFactory
                    .CreateStartSignalName(indexedStateMachineName).TrimExtendedVhdlIdDelimiters());
                var finishedSignalName = MemberStateMachineNameFactory
                    .CreateFinishedSignalName(indexedStateMachineName);

                stateMachineFinishedCheckCase.Whens.Add(new When
                {
                    Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() },
                    Body = new List<IVhdlElement>
                    {
                        new IfElse
                        {
                            Condition = new Binary
                            {
                                Left = finishedSignalName.ToVhdlSignalReference(),
                                Operator = Operator.Equality,
                                Right = Value.True
                            },
                            True = new InlineBlock(
                                new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.True },
                                new Assignment { AssignTo = startSignalName.ToVhdlSignalReference(), Expression = Value.False })
                        }
                    }
                });
            }

            waitForInvokedStateMachineToFinishState.Add(new IfElse
            {
                Condition = new Binary
                {
                    Left = stateMachineFinishedVariableReference,
                    Operator = Operator.Equality,
                    Right = Value.True
                },
                True = isInvokedStateMachineFinishedIfElseTrue
            });

            var waitForInvokedStateMachineToFinishStateIndex = stateMachine.AddState(waitForInvokedStateMachineToFinishState);
            currentBlock.Add(stateMachine.CreateStateChange(waitForInvokedStateMachineToFinishStateIndex));

            currentBlock.ChangeBlockToDifferentState(isInvokedStateMachineFinishedIfElseTrue, waitForInvokedStateMachineToFinishStateIndex);

            // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to 
            // a non-void method).
            if (!(expression.Parent is ExpressionStatement))
            {
                // We copy the used state machine's return value to a local variable and then use the local variable in
                // place of the original method call.

                var localReturnVariableReference = context.Scope.StateMachine
                    .CreateTemporaryVariable(
                        targetStateMachineName + "." + "return",
                        _typeConverter.ConvertTypeReference(expression.GetReturnType()))
                    .ToReference();

                var stateMachineReadReturnValueCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
                isInvokedStateMachineFinishedIfElseTrue.Add(stateMachineReadReturnValueCheckCase);
                for (int i = 0; i < maxRecursionDepth; i++)
                {
                    var returnVariableName = MemberStateMachineNameFactory
                        .CreateReturnVariableName(MemberStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i));

                    stateMachineReadReturnValueCheckCase.Whens.Add(new When
                    {
                        Expression = new Value { DataType = stateMachineRunningIndexVariable.DataType, Content = i.ToString() },
                        Body = new List<IVhdlElement>
                        {
                            new Assignment
                            {
                                AssignTo = localReturnVariableReference,
                                Expression = returnVariableName.ToVhdlVariableReference()
                            }
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
            var returnVariableReference = context.Scope.StateMachine
                .CreateTemporaryVariable(targetName + "." + "return", returnType)
                .ToReference();

            invokation.Parameters.Add(returnVariableReference);

            // Adding the procedure invokation directly to the body so it's before the current expression...
            context.Scope.CurrentBlock.Add(invokation.Terminate());

            // ...and using the return variable in place of the original call.
            return returnVariableReference;
        }
    }
}
