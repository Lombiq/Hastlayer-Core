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
        private readonly ITypeConversionTransformer _typeConversionTransformer;
        private readonly IDeclarableTypeCreator _declarableTypeCreator;


        public StateMachineInvocationBuilder(
            ITypeConversionTransformer typeConversionTransformer,
            IDeclarableTypeCreator declarableTypeCreator)
        {
            _typeConversionTransformer = typeConversionTransformer;
            _declarableTypeCreator = declarableTypeCreator;
        }


        public IBuildInvocationResult BuildInvocation(
            MethodDeclaration targetDeclaration,
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


            if (instanceCount == 1)
            {
                var buildInvocationBlockResult = BuildInvocationBlock(
                    targetDeclaration,
                    targetMethodName,
                    parameters,
                    context,
                    0);

                addInvocationStartComment();
                currentBlock.Add(buildInvocationBlockResult.InvocationBlock);

                return buildInvocationBlockResult;
            }
            else
            {
                var outParameterBackAssignments = new List<Assignment>();
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
                    var buildInvocationBlockResult = BuildInvocationBlock(
                        targetDeclaration,
                        targetMethodName,
                        parameters,
                        context,
                        i);

                    outParameterBackAssignments.AddRange(buildInvocationBlockResult.OutParameterBackAssignments);

                    proxyCase.Whens.Add(new CaseWhen
                    {
                        Expression = i.ToVhdlValue(invocationIndexVariableType),
                        Body = new List<IVhdlElement>
                        {
                            {
                                buildInvocationBlockResult.InvocationBlock
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

                return new BuildInvocationResult { OutParameterBackAssignments = outParameterBackAssignments };
            }
        }

        public IEnumerable<IVhdlElement> BuildMultiInvocationWait(
            MethodDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            ISubTransformerContext context)
        {
            return BuildInvocationWait(targetDeclaration, instanceCount, -1, waitForAll, context);
        }

        public IVhdlElement BuildSingleInvocationWait(
            MethodDeclaration targetDeclaration,
            int targetIndex,
            ISubTransformerContext context)
        {
            return BuildInvocationWait(targetDeclaration, 1, targetIndex, true, context).Single();
        }


        /// <summary>
        /// Be aware that the method can change the current block!
        /// </summary>
        private BuildInvocationBlockResult BuildInvocationBlock(
            MethodDeclaration targetDeclaration,
            string targetMethodName,
            IEnumerable<IVhdlElement> parameters,
            ISubTransformerContext context,
            int index)
        {
            var scope = context.Scope;
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
            var outParameterBackAssignments = new List<Assignment>();

            var methodParametersEnumerator = targetDeclaration
                .GetNonSimpleMemoryParameters()
                .GetEnumerator();
            methodParametersEnumerator.MoveNext();

            foreach (var parameter in parameters)
            {
                // Managing signals for parameter passing.
                var targetParameter = methodParametersEnumerator.Current;
                var parameterSignalType = _declarableTypeCreator
                    .CreateDeclarableType(targetParameter, targetParameter.Type, context.TransformationContext);

                Func<ParameterFlowDirection, Assignment> createParameterAssignment = (flowDirection) =>
                {
                    var parameterSignalName = stateMachine
                        .CreatePrefixedSegmentedObjectName(
                            ArchitectureComponentNameHelper
                                .CreateParameterSignalName(targetMethodName, targetParameter.Name, flowDirection)
                                .TrimExtendedVhdlIdDelimiters(),
                            index.ToString());
                    var parameterSignalReference = parameterSignalName.ToVhdlSignalReference();

                    var signals = flowDirection == ParameterFlowDirection.Out ?
                        stateMachine.InternallyDrivenSignals :
                        stateMachine.ExternallyDrivenSignals;
                    signals.AddIfNew(new ParameterSignal(
                        targetMethodName,
                        targetParameter.Name,
                        index,
                        false)
                    {
                        DataType = parameterSignalType,
                        Name = parameterSignalName
                    });


                    // Assign local variables to/from the intermediary parameter signal.
                    var assignmentExpression = flowDirection == ParameterFlowDirection.Out ? parameter : parameterSignalReference;
                    // Only trying casting if the parameter is not a constant or something other than an IDataObject.
                    if (parameter is IDataObject)
                    {
                        // Note: the below logic covers the most frequent cases to determine the passed local variable's
                        // data type. However it won't work with arbitrarily deep object graphs, e.g. array inside
                        // object inside array. For this a more generic, iterative implementation could be developed 
                        // that would search until the leaf of the object tree is found (the data type can't be passed 
                        // in the data object references themselves, since at that level (e.g. on the level of a variable
                        // reference) only the reference's name is known, the data type not necessarily.

                        var localVariableDataType = stateMachine.LocalVariables
                            .Single(variable => variable.Name == ((IDataObject)parameter).Name).DataType;

                        // If the parameter is an array access then the actual variable type should be the array
                        // element's type, or if the array element is a record, then its fields' type (in the latter
                        // case the next block will be also run to determine the record field's type).
                        if (localVariableDataType is ArrayType &&
                            (parameter is ArrayElementAccess || parameter is RecordFieldAccess))
                        {
                            localVariableDataType = ((ArrayType)localVariableDataType).ElementType;
                        }

                        if (localVariableDataType is UnconstrainedArrayInstantiation &&
                            (parameter is ArrayElementAccess || parameter is RecordFieldAccess))
                        {
                            localVariableDataType = ((UnconstrainedArrayInstantiation)localVariableDataType).ElementType;
                        }

                        if (localVariableDataType is Record)
                        {
                            var fieldAccess = parameter as RecordFieldAccess;
                            if (fieldAccess != null)
                            {
                                // A member of and object was passed to a method, i.e. Method(object.Property).

                                // This is a recursivel fields access, i.e. instance.Field1.Field1.
                                localVariableDataType = _declarableTypeCreator
                                    .CreateDeclarableType(targetParameter, targetParameter.Type, context.TransformationContext);
                            }

                            // Else the whole object was passed, i.e. Method(object). Nothing else to do.
                        }

                        if (flowDirection == ParameterFlowDirection.Out)
                        {
                            assignmentExpression = _typeConversionTransformer
                                .ImplementTypeConversion(localVariableDataType, parameterSignalType, parameter)
                                .Expression;
                        }
                        else
                        {
                            assignmentExpression = _typeConversionTransformer
                                .ImplementTypeConversion(parameterSignalType, localVariableDataType, parameterSignalReference)
                                .Expression;
                        }
                    }

                    // In this case the parameter is e.g. a primitive value, no need to assign to it.
                    if (flowDirection == ParameterFlowDirection.In && !(parameter is IDataObject))
                    {
                        return null;
                    }

                    return new Assignment
                    {
                        // If the parameter is of direction In then the parameter element should contain an IDataObject.
                        AssignTo = flowDirection == ParameterFlowDirection.Out ? parameterSignalReference : (IDataObject)parameter,
                        Expression = assignmentExpression
                    };
                };


                invocationBlock.Add(createParameterAssignment(ParameterFlowDirection.Out));
                if (targetParameter.IsOutFlowing())
                {
                    var assignment = createParameterAssignment(ParameterFlowDirection.In);
                    if (assignment != null) outParameterBackAssignments.Add(assignment);
                }

                methodParametersEnumerator.MoveNext();
            }


            invocationBlock.Add(InvocationHelper.CreateInvocationStart(stateMachine, targetMethodName, index));

            return new BuildInvocationBlockResult
            {
                InvocationBlock = invocationBlock,
                OutParameterBackAssignments = outParameterBackAssignments
            };
        }

        private IEnumerable<IVhdlElement> BuildInvocationWait(
            MethodDeclaration targetDeclaration,
            int instanceCount,
            int index,
            bool waitForAll,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            var waitForInvocationFinishedIfElse = InvocationHelper
                .CreateWaitForInvocationFinished(stateMachine, targetMethodName, instanceCount, waitForAll);

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


            var returnType = _declarableTypeCreator
                .CreateDeclarableType(targetDeclaration, targetDeclaration.ReturnType, context.TransformationContext);

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


        private class BuildInvocationResult : IBuildInvocationResult
        {
            public IEnumerable<Assignment> OutParameterBackAssignments { get; set; }
        }

        private class BuildInvocationBlockResult : BuildInvocationResult
        {
            public IVhdlElement InvocationBlock { get; set; }
        }
    }
}
