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

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class StateMachineInvocationBuilder : IStateMachineInvocationBuilder
    {
        private readonly ITypeConverter _typeConverter;


        public StateMachineInvocationBuilder(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public void BuildInvocation(
            EntityDeclaration targetDeclaration,
            IEnumerable<IVhdlElement> parameters,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            currentBlock.Add(new LineComment("Starting state machine invocation for the following method: " + targetMethodName));


            var maxDegreeOfParallelism = context.TransformationContext.GetTransformerConfiguration()
                .GetMaxInvokationInstanceCountConfigurationForMember(targetDeclaration).MaxDegreeOfParallelism;

            int previousMaxInvocationInstanceCount;
            if (!stateMachine.OtherMemberMaxInvocationInstanceCounts.TryGetValue(targetMethodName, out previousMaxInvocationInstanceCount) ||
                previousMaxInvocationInstanceCount < maxDegreeOfParallelism)
            {
                stateMachine.OtherMemberMaxInvocationInstanceCounts[targetMethodName] = maxDegreeOfParallelism;
            }


            var targetMethodDeclaration = (MethodDeclaration)targetDeclaration;

            if (maxDegreeOfParallelism == 1)
            {
                currentBlock.Add(BuildInvocationBlock(
                    targetMethodDeclaration,
                    targetMethodName,
                    parameters,
                    stateMachine,
                    0));
            }
            else
            {
                var invocationIndexVariableName = CreateInvocationIndexVariableName(stateMachine, targetMethodName);
                var invocationIndexVariableType = new RangedDataType(KnownDataTypes.UnrangedInt)
                {
                    RangeMax = maxDegreeOfParallelism
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

                for (int i = 0; i < maxDegreeOfParallelism; i++)
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
                                    stateMachine,
                                    i)
                            }
                        }
                    });
                }

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
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;
            var targetMethodName = targetDeclaration.GetFullName();


            var waitForInvokationFinishedIfElse = InvokationHelper
                .CreateWaitForInvokationFinished(stateMachine, targetDeclaration.GetFullName(), instanceCount);

            var currentStateName = stateMachine.CreateStateName(currentBlock.CurrentStateMachineStateIndex);
            var waitForInvokedStateMachinesToFinishState = new InlineBlock(
                new LineComment(
                    "Waiting for the state machine invocation of the following method to finish: " + targetMethodName),
                waitForInvokationFinishedIfElse);

            var waitForInvokedStateMachineToFinishStateIndex = stateMachine.AddState(waitForInvokedStateMachinesToFinishState);
            currentBlock.Add(stateMachine.CreateStateChange(waitForInvokedStateMachineToFinishStateIndex));

            currentBlock.ChangeBlockToDifferentState(waitForInvokationFinishedIfElse.True, waitForInvokedStateMachineToFinishStateIndex);


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
            }

            return returnVariableReferences;
        }


        private IVhdlElement BuildInvocationBlock(
            MethodDeclaration targetDeclaration, 
            string targetMethodName,
            IEnumerable<IVhdlElement> parameters,
            IMemberStateMachine stateMachine,
            int index)
        {
            var block = new InlineBlock();

            var indexedStateMachineName = ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMethodName, index);

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

                stateMachine.GlobalVariables.AddIfNew(new ParameterVariable(targetMethodName, currentParameter.Name)
                {
                    DataType = _typeConverter.ConvertAstType(currentParameter.Type),
                    Name = parameterVariableName,
                    Index = index
                });


                // Assign local values to be passed to the intermediary variable.
                block.Add(new Assignment
                {
                    AssignTo = parameterVariableName.ToVhdlVariableReference(),
                    Expression = parameter
                });

                methodParametersEnumerator.MoveNext();
            }


            block.Add(InvokationHelper.CreateInvokationStart(stateMachine, targetMethodName, index));

            return block;
        }


        private static string CreateInvocationIndexVariableName(IMemberStateMachine stateMachine, string targetMethodName)
        {
            return stateMachine.CreatePrefixedSegmentedObjectName(targetMethodName, "invocationIndex");
        }
    }
}
