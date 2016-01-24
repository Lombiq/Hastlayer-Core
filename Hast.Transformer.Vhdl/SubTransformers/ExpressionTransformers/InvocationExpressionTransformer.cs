using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.StateMachineGeneration;

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
                var memoryOperationFinishedStateIndex = stateMachine.AddState(memoryOperationFinishedBlock);

                currentBlock.Add(stateMachine.CreateStateChange(memoryOperationFinishedStateIndex));

                if (isWrite)
                {
                    // TODO: WriteEnable and CellIndex should be set in currentBlock and be re-set in
                    // memoryOperationFinishedBlock.
                    memoryOperationFinishedBlock.Add(new VhdlBuilder.Representation.Declaration.Comment("Write finish"));

                    currentBlock.Add(memoryOperationInvokation.Terminate());
                    currentBlock.ChangeBlockToDifferentState(memoryOperationFinishedBlock, memoryOperationFinishedStateIndex);

                    return Empty.Instance;
                }
                else
                {
                    // TODO: ReadEnable should be set in currentBlock and re-set in memoryOperationFinishedBlock.
                    currentBlock.Add(new VhdlBuilder.Representation.Declaration.Comment("ReadEnable"));

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

            var stateMachineRunningIndexVariableName = MemberStateMachineVariableHelper
                .GetNextUnusedTemporaryVariableName(targetStateMachineName + "." + "runningIndex", context.Scope.StateMachine);
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
                var indexedStateMachineName = MemberStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i);
                var startVariableReference = MemberStateMachineNameFactory
                    .CreateStartVariableName(indexedStateMachineName)
                    .ToVhdlVariableReference();

                var trueBlock = new InlineBlock();

                trueBlock.Add(new Assignment
                {
                    AssignTo = startVariableReference,
                    Expression = Value.True
                });

                trueBlock.Add(new Assignment
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
                            MemberStateMachineNameFactory.CreatePrefixedVariableName(indexedStateMachineName, methodParametersEnumerator.Current.Name)
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
                        Left = startVariableReference,
                        Operator = Operator.Equality,
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
            var stateMachineFinishedVariableName = MemberStateMachineVariableHelper
                .GetNextUnusedTemporaryVariableName(targetStateMachineName + "." + "finished", context.Scope.StateMachine);
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
            for (int i = 0; i < maxCallStackDepth; i++)
            {
                var finishedVariableName = MemberStateMachineNameFactory
                    .CreateFinishedVariableName(MemberStateMachineNameFactory.CreateStateMachineName(targetStateMachineName, i));

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
                                Operator = Operator.Equality,
                                Right = Value.True
                            },
                            True = new Assignment { AssignTo = stateMachineFinishedVariableReference, Expression = Value.True }
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

                var localReturnVariableReference = MemberStateMachineVariableHelper
                    .CreateTemporaryVariable(
                        targetStateMachineName + "." + "return",
                        _typeConverter.ConvertTypeReference(expression.GetReturnType()),
                        context.Scope.StateMachine)
                    .ToReference();

                var stateMachineReadReturnValueCheckCase = new Case { Expression = stateMachineRunningIndexVariable.ToReference() };
                isInvokedStateMachineFinishedIfElseTrue.Add(stateMachineReadReturnValueCheckCase);
                for (int i = 0; i < maxCallStackDepth; i++)
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
            var returnVariableReference = MemberStateMachineVariableHelper
                .CreateTemporaryVariable(targetName + "." + "return", returnType, context.Scope.StateMachine)
                .ToReference();

            invokation.Parameters.Add(returnVariableReference);

            // Adding the procedure invokation directly to the body so it's before the current expression...
            context.Scope.CurrentBlock.Add(invokation.Terminate());

            // ...and using the return variable in place of the original call.
            return returnVariableReference;
        }
    }
}
