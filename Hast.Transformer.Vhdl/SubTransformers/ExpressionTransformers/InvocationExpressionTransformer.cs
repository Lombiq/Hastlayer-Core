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
                    new LineComment("Waiting for the SimpleMemory operation to finish."),
                    new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = (isWrite ? SimpleMemoryPortNames.WritesDone : SimpleMemoryPortNames.ReadsDone).ToVhdlSignalReference(),
                            Operator = BinaryOperator.Equality,
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
                        _typeConverter.ConvertTypeReference(expression.GetReturnTypeReference()),
                        memoryOperationInvokation,
                        context);

                    return returnReference;
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
                    // Adding signal for parameter passing if it doesn't exist.
                    var currentParameter = methodParametersEnumerator.Current;

                    var parameterVariableName = stateMachine
                        .CreatePrefixedSegmentedObjectName(targetMethodName, currentParameter.Name, i.ToString());

                    stateMachine.GlobalVariables.AddIfNew(new Variable
                        {
                            DataType = _typeConverter.ConvertAstType(currentParameter.Type),
                            Name = parameterVariableName
                        });


                    // Assign local values to be passed to the intermediary signal.
                    currentBlock.Add(new Assignment
                        {
                            AssignTo = parameterVariableName.ToVhdlSignalReference(),
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
                    var returnVariableName = stateMachine
                        .CreatePrefixedSegmentedObjectName(targetMethodName, NameSuffixes.Return, i.ToString());

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
                .CreateVariableWithNextUnusedIndexedName(targetName + "." + NameSuffixes.Return, returnType)
                .ToReference();

            invokation.Parameters.Add(returnVariableReference);

            // Adding the procedure invokation directly to the body so it's before the current expression...
            context.Scope.CurrentBlock.Add(invokation.Terminate());

            // ...and using the return variable in place of the original call.
            return returnVariableReference;
        }

        private static DataObjectReference CreateFinishedSignalReference(
            IMemberStateMachine stateMachine,
            string targetMethodName,
            int index)
        {
            return stateMachine
                    .CreatePrefixedSegmentedObjectName(targetMethodName, NameSuffixes.Finished, index.ToString())
                    .ToVhdlSignalReference();
        }
    }
}
