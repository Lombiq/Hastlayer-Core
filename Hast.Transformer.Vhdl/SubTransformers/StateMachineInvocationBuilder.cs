using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Common.Configuration;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class StateMachineInvocationBuilder : IStateMachineInvocationBuilder
    {
        private readonly ITypeConverter _typeConverter;
        private readonly ITypeConversionTransformer _typeConversionTransformer;


        public StateMachineInvocationBuilder(
            ITypeConverter typeConverter,
            ITypeConversionTransformer typeConversionTransformer)
        {
            _typeConverter = typeConverter;
            _typeConversionTransformer = typeConversionTransformer;
        }


        public void BuildInvocation(
            EntityDeclaration targetDeclaration,
            IEnumerable<IVhdlElement> parameters,
            int instanceCount,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            Action addInvocationStartComment = () =>
                currentBlock
                .Add(new LineComment("Starting state machine invocation for the following method: " + targetMethodName));


            var maxDegreeOfParallelism = context.TransformationContext.GetTransformerConfiguration()
                .GetMaxInvocationInstanceCountConfigurationForMember(targetDeclaration).MaxDegreeOfParallelism;

            if (instanceCount > maxDegreeOfParallelism)
            {
                throw new InvalidOperationException(
                    "This parallelized call from " + context.Scope.Method + " to " + targetMethodName + " would do " +
                    instanceCount +
                    " calls in parallel but the maximal degree of parallelism for this member was set up as " +
                    maxDegreeOfParallelism + ".");
            }

            int previousMaxInvocationInstanceCount;
            if (!stateMachine.OtherMemberMaxInvocationInstanceCounts.TryGetValue(targetDeclaration, out previousMaxInvocationInstanceCount) ||
                previousMaxInvocationInstanceCount < instanceCount)
            {
                stateMachine.OtherMemberMaxInvocationInstanceCounts[targetDeclaration] = instanceCount;
            }


            var targetMethodDeclaration = (MethodDeclaration)targetDeclaration;

            if (instanceCount == 1)
            {
                var invocationBlock = BuildInvocationBlock(
                    targetMethodDeclaration,
                    targetMethodName,
                    parameters,
                    context.Scope,
                    0);

                addInvocationStartComment();
                currentBlock.Add(invocationBlock);
            }
            else
            {
                var invocationIndexVariableName = stateMachine.CreateInvocationIndexVariableName(targetMethodName);
                var invocationIndexVariableType = new RangedDataType(KnownDataTypes.UnrangedInt)
                {
                    RangeMax = instanceCount - 1
                };
                var invocationIndexVariableReference = invocationIndexVariableName.ToVhdlVariableReference();
                stateMachine.LocalVariables.AddIfNew(new Variable
                {
                    DataType = invocationIndexVariableType,
                    InitialValue = KnownDataTypes.UnrangedInt.DefaultValue,
                    Name = invocationIndexVariableName
                });

                var proxyCase = new Case
                {
                    Expression = invocationIndexVariableReference
                };

                for (int i = 0; i < instanceCount; i++)
                {
                    proxyCase.Whens.Add(new CaseWhen
                    {
                        Expression = i.ToVhdlValue(invocationIndexVariableType),
                        Body = new List<IVhdlElement>
                        {
                            {
                                BuildInvocationBlock(
                                    targetMethodDeclaration,
                                    targetMethodName,
                                    parameters,
                                    context.Scope,
                                    i)
                            }
                        }
                    });
                }

                addInvocationStartComment();
                currentBlock.Add(proxyCase);
                currentBlock.Add(new Assignment
                {
                    AssignTo = invocationIndexVariableReference,
                    Expression = new Binary
                    {
                        Left = invocationIndexVariableReference,
                        Operator = BinaryOperator.Add,
                        Right = 1.ToVhdlValue(invocationIndexVariableType)
                    }
                });
            }
        }

        public IEnumerable<IVhdlElement> BuildMultiInvocationWait(
            EntityDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            ISubTransformerContext context)
        {
            return BuildInvocationWait(targetDeclaration, instanceCount, -1, waitForAll, context);
        }

        public IVhdlElement BuildSingleInvocationWait(
            EntityDeclaration targetDeclaration,
            int targetIndex,
            ISubTransformerContext context)
        {
            return BuildInvocationWait(targetDeclaration, 1, targetIndex, true, context).Single();
        }


        /// <summary>
        /// Be aware that the method can change the current block!
        /// </summary>
        private IVhdlElement BuildInvocationBlock(
            MethodDeclaration targetDeclaration,
            string targetMethodName,
            IEnumerable<IVhdlElement> parameters,
            ISubTransformerScope scope,
            int index)
        {
            var stateMachine = scope.StateMachine;

            var indexedStateMachineName = ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMethodName, index);


            // Due to the time needed for the invocation proxy to register that the invoked state machine is not started
            // any more the same state machine can be restarted in the second state counted from the await state at 
            // earliest. Thus adding a new state and also a wait state if necessary.
            var finishedInvokedComponentsForStates = scope.FinishedInvokedStateMachinesForStates;
            ISet<string> finishedComponents;

            // Would the invocation be restarted in the same state? We need to add a state just to wait, then a new state
            // for the new invocation start.
            if (finishedInvokedComponentsForStates
                .TryGetValue(scope.CurrentBlock.StateMachineStateIndex, out finishedComponents) &&
                finishedComponents.Contains(indexedStateMachineName))
            {
                scope.CurrentBlock.Add(new LineComment(
                    "The last invocation for the target state machine just finished, so need to start the next one in a later state."));

                stateMachine.AddNewStateAndChangeCurrentBlock(
                    scope,
                    new InlineBlock(new LineComment(
                        "This state was just added to leave time for the invocation proxy to register that the previous invocation finished.")));

                stateMachine.AddNewStateAndChangeCurrentBlock(scope);
            }
            // Are we one state later from the await for some other reason already? Still another state needs to be added
            // got leave time for the invocation proxy.
            else if (finishedInvokedComponentsForStates
                .TryGetValue(scope.CurrentBlock.StateMachineStateIndex - 1, out finishedComponents) &&
                finishedComponents.Contains(indexedStateMachineName))
            {
                scope.CurrentBlock.Add(new LineComment(
                    "The last invocation for the target state machine finished in the previous state, so need to start the next one in the next state."));
                stateMachine.AddNewStateAndChangeCurrentBlock(scope);
            }


            var invocationBlock = new InlineBlock();

            var methodParametersEnumerator = targetDeclaration.Parameters
                .Where(parameter => !parameter.IsSimpleMemoryParameter())
                .GetEnumerator();
            methodParametersEnumerator.MoveNext();

            foreach (var parameter in parameters)
            {
                // Adding signal for parameter passing if it doesn't exist.
                var currentParameter = methodParametersEnumerator.Current;

                var parameterSignalName = stateMachine
                    .CreatePrefixedSegmentedObjectName(
                        ArchitectureComponentNameHelper
                            .CreateParameterSignalName(targetMethodName, currentParameter.Name).TrimExtendedVhdlIdDelimiters(),
                        index.ToString());

                var parameterSignalType = _typeConverter.ConvertAstType(currentParameter.Type);
                stateMachine.InternallyDrivenSignals.AddIfNew(new ParameterSignal(targetMethodName, currentParameter.Name)
                {
                    DataType = parameterSignalType,
                    Name = parameterSignalName,
                    Index = index
                });


                // Assign local values to be passed to the intermediary parameter signal.
                var assignmentExpression = parameter;
                // Only trying casting if the parameter is not a constant or something other than an IDataObject.
                if (parameter is IDataObject)
                {
                    assignmentExpression = _typeConversionTransformer
                        .ImplementTypeConversion(
                            stateMachine.LocalVariables
                                .Single(variable => variable.Name == ((IDataObject)parameter).Name).DataType,
                            parameterSignalType,
                            parameter)
                        .Expression;
                }
                invocationBlock.Add(new Assignment
                {
                    AssignTo = parameterSignalName.ToVhdlSignalReference(),
                    Expression = assignmentExpression
                });

                methodParametersEnumerator.MoveNext();
            }


            invocationBlock.Add(InvocationHelper.CreateInvocationStart(stateMachine, targetMethodName, index));

            return invocationBlock;
        }

        private IEnumerable<IVhdlElement> BuildInvocationWait(
            EntityDeclaration targetDeclaration,
            int instanceCount,
            int index,
            bool waitForAll,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            var waitForInvocationFinishedIfElse = InvocationHelper
                .CreateWaitForInvocationFinished(stateMachine, targetDeclaration.GetFullName(), instanceCount, waitForAll);

            var currentStateName = stateMachine.CreateStateName(currentBlock.StateMachineStateIndex);
            var waitForInvokedStateMachinesToFinishState = new InlineBlock(
                new LineComment(
                    "Waiting for the state machine invocation of the following method to finish: " + targetMethodName),
                waitForInvocationFinishedIfElse);

            var waitForInvokedStateMachineToFinishStateIndex = stateMachine.AddState(waitForInvokedStateMachinesToFinishState);
            currentBlock.Add(stateMachine.CreateStateChange(waitForInvokedStateMachineToFinishStateIndex));

            if (instanceCount > 1)
            {
                waitForInvocationFinishedIfElse.True.Add(new Assignment
                {
                    AssignTo = stateMachine
                        .CreateInvocationIndexVariableName(targetMethodName)
                        .ToVhdlVariableReference(),
                    Expression = 0.ToVhdlValue(KnownDataTypes.UnrangedInt)
                });
            }

            currentBlock.ChangeBlockToDifferentState(waitForInvocationFinishedIfElse.True, waitForInvokedStateMachineToFinishStateIndex);


            var returnType = _typeConverter.ConvertAstType(targetDeclaration.ReturnType);

            if (returnType == KnownDataTypes.Void)
            {
                return Enumerable.Repeat<IVhdlElement>(Empty.Instance, instanceCount);
            }

            var returnVariableReferences = new List<IDataObject>();

            Action<int> buildInvocationWaitBlock = targetIndex =>
            {
                // Creating the return signal if it doesn't exist.
                var returnSignalReference = stateMachine.CreateReturnSignalReferenceForTargetComponent(targetMethodName, targetIndex);

                stateMachine.ExternallyDrivenSignals.AddIfNew(new Signal
                {
                    DataType = returnType,
                    Name = returnSignalReference.Name
                });

                // The return signal's value needs to be copied over to a local variable. Otherwise if we'd re-use the
                // signal with multiple invocations the last invocation's value would be present in all references.
                var returnVariableReference = stateMachine
                    .CreateVariableWithNextUnusedIndexedName(NameSuffixes.Return, returnType)
                    .ToReference();

                currentBlock.Add(new Assignment
                {
                    AssignTo = returnVariableReference,
                    Expression = returnSignalReference
                });

                // Using the reference of the state machine's return value in place of the original method call.
                returnVariableReferences.Add(returnVariableReference);

                // Noting that this component was finished in this state.
                var finishedInvokedComponentsForStates = context.Scope.FinishedInvokedStateMachinesForStates;
                ISet<string> finishedComponents;
                if (!finishedInvokedComponentsForStates
                    .TryGetValue(currentBlock.StateMachineStateIndex, out finishedComponents))
                {
                    finishedComponents = finishedInvokedComponentsForStates[currentBlock.StateMachineStateIndex] =
                        new HashSet<string>();
                }
                finishedComponents.Add(ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMethodName, targetIndex));
            };

            if (index == -1)
            {
                for (int i = 0; i < instanceCount; i++)
                {
                    buildInvocationWaitBlock(i);
                }
            }
            else
            {
                buildInvocationWaitBlock(index);
            }

            return returnVariableReferences;
        }
    }
}
