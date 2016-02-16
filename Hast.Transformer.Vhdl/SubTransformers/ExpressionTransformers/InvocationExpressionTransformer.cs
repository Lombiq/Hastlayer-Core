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
using Hast.Transformer.Vhdl.ArchitectureComponents;
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
                stateMachine.AddSimpleMemorySignalsIfNew();

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

                // Setting SimpleMemory control signals:
                currentBlock.Add(new Assignment
                {
                    AssignTo = stateMachine.CreateSimpleMemoryCellIndexSignalReference(),
                    Expression = invokationParameters[0] // CellIndex is conventionally the first invokation parameter.
                });

                var enablePortReference = isWrite ?
                    stateMachine.CreateSimpleMemoryWriteEnableSignalReference() :
                    stateMachine.CreateSimpleMemoryReadEnableSignalReference();
                currentBlock.Add(new Assignment
                {
                    AssignTo = enablePortReference,
                    Expression = Value.OneCharacter
                });


                Func<IVhdlElement, bool, IVhdlElement> implementSimpleMemoryTypeConversion = 
                    (variableToConvert, directionIsLogicVectorToType) =>
                    {
                        // If the memory operations was Read/Write4Bytes then no need to do any conversions.
                        if (targetMemberReference.MemberName.EndsWith("4Bytes"))
                        {
                            return variableToConvert;
                        }

                        string dataConversionInvokationTarget = null;
                        var memoryType = memberName.Replace("Write", string.Empty).Replace("Read", string.Empty);

                        // Using the built-in conversion functions to handle known data types.
                        if (memoryType == "UInt32" ||
                            memoryType == "Int32" ||
                            memoryType == "Boolean" ||
                            memoryType == "Char")
                        {
                            if (directionIsLogicVectorToType)
                            {
                                dataConversionInvokationTarget = "ConvertStdLogicVectorTo" + memoryType;
                            }
                            else
                            {
                                dataConversionInvokationTarget = "Convert" + memoryType + "ToStdLogicVector";
                            }
                        }

                        return new Invokation
                        {
                            Target = dataConversionInvokationTarget.ToVhdlIdValue(),
                            Parameters = new List<IVhdlElement> { { variableToConvert } }
                        };
                    };

                if (isWrite)
                {
                    currentBlock.Add(new Assignment
                    {
                        AssignTo = stateMachine.CreateSimpleMemoryDataOutSignalReference(),
                        // The data to write is conventionally the second parameter.
                        Expression = implementSimpleMemoryTypeConversion(invokationParameters[1], false)
                    });
                }

                // The memory operation should be initialized in this state, then finished in another one.
                var memoryOperationFinishedBlock = new InlineBlock();
                var endMemoryOperationBlock = new InlineBlock(
                    new LineComment("Waiting for the SimpleMemory operation to finish."),
                    new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = (isWrite ? SimpleMemoryPortNames.WritesDone : SimpleMemoryPortNames.ReadsDone)
                                .ToExtendedVhdlId()
                                .ToVhdlSignalReference(),
                            Operator = BinaryOperator.Equality,
                            Right = Value.OneCharacter
                        },
                        True = memoryOperationFinishedBlock
                    });
                var memoryOperationFinishedStateIndex = stateMachine.AddState(endMemoryOperationBlock);

                memoryOperationFinishedBlock.Add(new Assignment
                {
                    AssignTo = enablePortReference,
                    Expression = Value.ZeroCharacter
                });

                currentBlock.Add(stateMachine.CreateStateChange(memoryOperationFinishedStateIndex));

                if (isWrite)
                {
                    memoryOperationFinishedBlock.Body.Insert(0, new LineComment("SimpleMemory write finished."));

                    currentBlock.ChangeBlockToDifferentState(memoryOperationFinishedBlock, memoryOperationFinishedStateIndex);

                    return Empty.Instance;
                }
                else
                {
                    memoryOperationFinishedBlock.Body.Insert(0, new LineComment("SimpleMemory read finished."));


                    currentBlock.ChangeBlockToDifferentState(memoryOperationFinishedBlock, memoryOperationFinishedStateIndex);

                    return implementSimpleMemoryTypeConversion(
                        SimpleMemoryPortNames.DataIn.ToExtendedVhdlId().ToVhdlSignalReference(),
                        true);
                }
            }


            var targetMethodName = expression.GetFullName();

            var targetDeclaration = targetMemberReference.GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);

            if (targetDeclaration == null || !(targetDeclaration is MethodDeclaration))
            {
                throw new InvalidOperationException("The invoked method " + targetMethodName + " can't be found.");
            }


            var maxDegreeOfParallelism = context.TransformationContext.GetTransformerConfiguration()
                .GetMaxInvokationInstanceCountConfigurationForMember(targetDeclaration.GetSimpleName()).MaxDegreeOfParallelism;
            // Eventually this should be determined, dummy value for now.
            var currentInvokationDegreeOfParallelism = 1;

            if (currentInvokationDegreeOfParallelism > maxDegreeOfParallelism)
            {
                throw new InvalidOperationException(
                    "This parallelized call from " + context.Scope.Method + " to " + targetMethodName + " would do " +
                    currentInvokationDegreeOfParallelism +
                    " calls in parallel but the maximal degree of parallelism for this member was set up as " +
                    maxDegreeOfParallelism + ".");
            }

            int previousMaxCallInstanceCount;
            if (!stateMachine.OtherMemberMaxInvokationInstanceCounts.TryGetValue(targetMethodName, out previousMaxCallInstanceCount) ||
                previousMaxCallInstanceCount < currentInvokationDegreeOfParallelism)
            {
                stateMachine.OtherMemberMaxInvokationInstanceCounts[targetMethodName] = currentInvokationDegreeOfParallelism;
            }


            currentBlock.Add(new LineComment("Starting state machine invokation (transformed from a method call)."));


            for (int i = 0; i < currentInvokationDegreeOfParallelism; i++)
            {
                var indexedStateMachineName = ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMethodName, i);

                var methodParametersEnumerator = ((MethodDeclaration)targetDeclaration).Parameters
                    .Where(parameter => !parameter.IsSimpleMemoryParameter())
                    .GetEnumerator();
                methodParametersEnumerator.MoveNext();

                foreach (var parameter in transformedParameters)
                {
                    // Adding variable for parameter passing if it doesn't exist.
                    var currentParameter = methodParametersEnumerator.Current;

                    var parameterVariableName = stateMachine
                        .CreatePrefixedSegmentedObjectName(
                            ArchitectureComponentNameHelper
                                .CreateParameterVariableName(targetMethodName, currentParameter.Name).TrimExtendedVhdlIdDelimiters(),
                            i.ToString());

                    stateMachine.GlobalVariables.AddIfNew(new ParameterVariable(targetMethodName, currentParameter.Name)
                        {
                            DataType = _typeConverter.ConvertAstType(currentParameter.Type),
                            Name = parameterVariableName,
                            Index = i
                        });


                    // Assign local values to be passed to the intermediary variable.
                    currentBlock.Add(new Assignment
                        {
                            AssignTo = parameterVariableName.ToVhdlVariableReference(),
                            Expression = parameter
                        });

                    methodParametersEnumerator.MoveNext();
                }


                currentBlock.Add(InvokationHelper.CreateInvokationStart(stateMachine, targetMethodName, i));
            }


            var waitForInvokationFinishedIfElse = InvokationHelper
                .CreateWaitForInvokationFinished(stateMachine, targetMethodName, currentInvokationDegreeOfParallelism);

            var currentStateName = stateMachine.CreateStateName(currentBlock.CurrentStateMachineStateIndex);
            var waitForInvokedStateMachinesToFinishState = new InlineBlock(
                new GeneratedComment(vhdlGenerationOptions =>
                    "Waiting for the state machine invokation to finish, which was started in state " +
                    vhdlGenerationOptions.NameShortener(currentStateName) +
                    "."),
                waitForInvokationFinishedIfElse);

            var waitForInvokedStateMachineToFinishStateIndex = stateMachine.AddState(waitForInvokedStateMachinesToFinishState);
            currentBlock.Add(stateMachine.CreateStateChange(waitForInvokedStateMachineToFinishStateIndex));

            currentBlock.ChangeBlockToDifferentState(waitForInvokationFinishedIfElse.True, waitForInvokedStateMachineToFinishStateIndex);

            // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to 
            // a non-void method).
            if (!(expression.Parent is ExpressionStatement))
            {
                // Using the references of the state machines' return values in place of the original method calls.
                var returnVariableReferences = new List<DataObjectReference>();

                for (int i = 0; i < currentInvokationDegreeOfParallelism; i++)
                {
                    // Creating return variable if it doesn't exist.
                    var returnVariableName = stateMachine.CreateReturnVariableNameForTargetComponent(targetMethodName, i);

                    stateMachine.GlobalVariables.AddIfNew(new Variable
                    {
                        DataType = _typeConverter.ConvertTypeReference(expression.GetReturnTypeReference()),
                        Name = returnVariableName
                    });


                    returnVariableReferences.Add(returnVariableName.ToVhdlVariableReference());
                }

                // Just handling a single method call now, parallelization will come later.
                return returnVariableReferences[0];
            }
            else
            {
                return Empty.Instance;
            }
        }
    }
}
