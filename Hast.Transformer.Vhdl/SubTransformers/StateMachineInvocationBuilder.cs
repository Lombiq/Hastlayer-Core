using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Models;
using Hast.Common.Configuration;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;

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
                    RangeMax = (uint)instanceCount - 1
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

        public IEnumerable<IVhdlElement> BuildInvocationWait(
            EntityDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            var waitForInvocationFinishedIfElse = InvocationHelper
                .CreateWaitForInvocationFinished(stateMachine, targetDeclaration.GetFullName(), instanceCount, waitForAll);

            var currentStateName = stateMachine.CreateStateName(currentBlock.CurrentStateMachineStateIndex);
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

            var returnVariableReferences = new List<DataObjectReference>();

            for (int i = 0; i < instanceCount; i++)
            {
                // Creating the return variable if it doesn't exist.
                var returnVariableName = stateMachine.CreateReturnVariableNameForTargetComponent(targetMethodName, i);

                stateMachine.GlobalVariables.AddIfNew(new Variable
                {
                    DataType = returnType,
                    Name = returnVariableName
                });

                // Using the reference of the state machine's return value in place of the original method call.
                returnVariableReferences.Add(returnVariableName.ToVhdlVariableReference());

                // Noting that this component was finished in this state.
                var finishedInvokedComponentsForStates = context.Scope.FinishedInvokedStateMachinesForStates;
                ISet<string> finishedComponents;
                if (!finishedInvokedComponentsForStates
                    .TryGetValue(currentBlock.CurrentStateMachineStateIndex, out finishedComponents))
                {
                    finishedComponents = finishedInvokedComponentsForStates[currentBlock.CurrentStateMachineStateIndex] = 
                        new HashSet<string>();
                }
                finishedComponents.Add(ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMethodName, i));
            }

            return returnVariableReferences;
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


            // Is there an invocation await for this target in the current state? Because if yes, we can't immediately
            // restart it (due to the latency of the invocation proxy), need to move to a new state.
            var finishedInvokedComponentsForStates = scope.FinishedInvokedStateMachinesForStates;
            ISet<string> finishedComponents;
            if (finishedInvokedComponentsForStates
                .TryGetValue(scope.CurrentBlock.CurrentStateMachineStateIndex, out finishedComponents))
            {
                if (finishedComponents.Contains(indexedStateMachineName))
                {
                    var currentBlock = scope.CurrentBlock;

                    var newBlock = new InlineBlock();
                    var newStateIndex = stateMachine.AddState(newBlock);
                    currentBlock.Add(stateMachine.CreateStateChange(newStateIndex));
                    currentBlock.ChangeBlockToDifferentState(newBlock, newStateIndex);
                }
            }


            var invocationBlock = new InlineBlock();

            var methodParametersEnumerator = targetDeclaration.Parameters
                .Where(parameter => !parameter.IsSimpleMemoryParameter())
                .GetEnumerator();
            methodParametersEnumerator.MoveNext();

            foreach (var parameter in parameters)
            {
                // Adding variable for parameter passing if it doesn't exist.
                var currentParameter = methodParametersEnumerator.Current;

                var parameterVariableName = stateMachine
                    .CreatePrefixedSegmentedObjectName(
                        ArchitectureComponentNameHelper
                            .CreateParameterVariableName(targetMethodName, currentParameter.Name).TrimExtendedVhdlIdDelimiters(),
                        index.ToString());

                var parameterVariableType = _typeConverter.ConvertAstType(currentParameter.Type);
                stateMachine.GlobalVariables.AddIfNew(new ParameterVariable(targetMethodName, currentParameter.Name)
                {
                    DataType = parameterVariableType,
                    Name = parameterVariableName,
                    Index = index
                });


                // Assign local values to be passed to the intermediary variable.
                var assignmentExpression = parameter;
                // Only trying casting if the parameter is not a constant or something other than an IDataObject.
                if (parameter is IDataObject)
                {
                    assignmentExpression = _typeConversionTransformer
                        .ImplementTypeConversion(
                            stateMachine.LocalVariables
                                .Single(variable => variable.Name == ((IDataObject)parameter).Name).DataType,
                            parameterVariableType,
                            parameter)
                        .Expression;
                }
                invocationBlock.Add(new Assignment
                {
                    AssignTo = parameterVariableName.ToVhdlVariableReference(),
                    Expression = assignmentExpression
                });

                methodParametersEnumerator.MoveNext();
            }


            invocationBlock.Add(InvocationHelper.CreateInvocationStart(stateMachine, targetMethodName, index));

            return invocationBlock;
        }
    }
}
