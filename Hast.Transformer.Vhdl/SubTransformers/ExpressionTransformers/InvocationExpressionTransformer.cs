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
using Hast.Transformer.Vhdl.SimpleMemory;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    // SimpleMemory and member invocation transformation are factored out into two methods so the class has some
    // structure, not to have one giant TransformInvocationExpression method.
    public class InvocationExpressionTransformer : IInvocationExpressionTransformer
    {
        private readonly IStateMachineInvocationBuilder _stateMachineInvocationBuilder;
        private readonly ITypeConverter _typeConverter;


        public InvocationExpressionTransformer(IStateMachineInvocationBuilder stateMachineInvocationBuilder, ITypeConverter typeConverter)
        {
            _stateMachineInvocationBuilder = stateMachineInvocationBuilder;
            _typeConverter = typeConverter;
        }


        public IVhdlElement TransformInvocationExpression(
            InvocationExpression expression,
            IEnumerable<IVhdlElement> transformedParameters,
            ISubTransformerContext context)
        {
            var targetMemberReference = expression.Target as MemberReferenceExpression;

            // This is a SimpleMemory access.
            if (context.TransformationContext.UseSimpleMemory() &&
                targetMemberReference != null &&
                targetMemberReference.Target.Is<IdentifierExpression>(identifier =>
                    identifier.Identifier == context.Scope.Method.GetSimpleMemoryParameterName()))
            {
                return TransformSimpleMemoryInvocation(expression, transformedParameters, targetMemberReference, context);
            }
            // This is a standard member access.
            else
            {
                return TransformMemberInvocation(expression, transformedParameters, targetMemberReference, context);
            }
        }


        private IVhdlElement TransformSimpleMemoryInvocation(
            InvocationExpression expression,
            IEnumerable<IVhdlElement> transformedParameters,
            MemberReferenceExpression targetMemberReference,
            ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

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
                Expression = new Invokation
                {
                    // Resizing the CellIndex parameter to the length of the signal, so there is no type mismatch.
                    Target = "resize".ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                        {
                            // CellIndex is conventionally the first invokation parameter. 
                            { invokationParameters[0] },
                            SimpleMemoryTypes.CellIndexInternalSignalDataType.Size.ToVhdlValue(KnownDataTypes.UnrangedInt)
                        }
                }
            });

            var enablePortReference = isWrite ?
                stateMachine.CreateSimpleMemoryWriteEnableSignalReference() :
                stateMachine.CreateSimpleMemoryReadEnableSignalReference();
            currentBlock.Add(new Assignment
            {
                AssignTo = enablePortReference,
                Expression = Value.True
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
                        Right = Value.True
                    },
                    True = memoryOperationFinishedBlock
                });
            var memoryOperationFinishedStateIndex = stateMachine.AddState(endMemoryOperationBlock);

            memoryOperationFinishedBlock.Add(new Assignment
            {
                AssignTo = enablePortReference,
                Expression = Value.False
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

        private IVhdlElement TransformMemberInvocation(
            InvocationExpression expression,
            IEnumerable<IVhdlElement> transformedParameters,
            MemberReferenceExpression targetMemberReference,
            ISubTransformerContext context)
        {
            var targetMethodName = expression.GetFullName();


            // This is a Task.FromResult() method call.
            if (targetMethodName.IsTaskFromResultMethodName())
            {
                return transformedParameters.Single();
            }

            // This is a Task.Wait() call so needs special care.
            if (targetMethodName == "System.Void System.Threading.Tasks.Task::Wait()")
            {
                // Tasks aren't awaited where they're started so we only need to await the already started state
                // machines here.
                var waitTarget = ((MemberReferenceExpression)expression.Target).Target;

                // Is it a Task.Something().Wait() call?
                var memberName = string.Empty;
                if (waitTarget.Is<InvocationExpression>(invocation => 
                    invocation.Target.Is<MemberReferenceExpression>(member =>
                    {
                        memberName = member.MemberName;
                        return member.Target.Is<TypeReferenceExpression>(type =>
                            _typeConverter.ConvertAstType(type.Type) == SpecialTypes.Task);
                    })))
                {
                    if (memberName == "WhenAll" || memberName == "WhenAny")
                    {
                        // Since it's used in a WhenAll() or WhenAny() call the argument should be an array.
                        var taskArrayIdentifier = 
                            ((IdentifierExpression)((InvocationExpression)waitTarget).Arguments.Single()).Identifier;

                        // This array originally stored the Task<T> objects but now is just for the results, so we have 
                        // to move the results to its elements.
                        var targetMethod = context.Scope.TaskVariableNameToDisplayClassMethodMappings[taskArrayIdentifier];
                        var resultReferences = _stateMachineInvocationBuilder.BuildInvocationWait(
                            targetMethod,
                            context.TransformationContext.GetTransformerConfiguration()
                                .GetMaxInvokationInstanceCountConfigurationForMember(targetMethod).MaxDegreeOfParallelism,
                            memberName == "WhenAll",
                            context);

                        var index = 0;
                        var stateMachine = context.Scope.StateMachine;
                        var arrayReference = stateMachine.CreatePrefixedObjectName(taskArrayIdentifier).ToVhdlVariableReference();
                        var resultBlock = new InlineBlock();
                        foreach (var resultReference in resultReferences)
                        {
                            resultBlock.Add(
                                new Assignment
                                {
                                    AssignTo = new ArrayElementAccess
                                    {
                                        Array = arrayReference,
                                        IndexExpression = index.ToVhdlValue(KnownDataTypes.UnrangedInt)
                                    },
                                    Expression = resultReference
                                });
                            index++;
                        }

                        return resultBlock;
                    }
                }
            }


            EntityDeclaration targetDeclaration = null;

            // Is this a reference to a member of the parent class from a compiler-generated DisplayClass?
            // These look like following: this.<>4__this.IsPrimeNumberInternal()
            if (targetMemberReference.Target is MemberReferenceExpression)
            {
                var targetTargetFullName = ((MemberReferenceExpression)targetMemberReference.Target).GetFullName();
                if (targetTargetFullName.IsDisplayClassMemberName())
                {
                    // We need to find the corresponding member in the parent class of this expression's class.
                    targetDeclaration = expression
                        .FindFirstParentTypeDeclaration() // This is the level of the DisplayClass.
                        .FindFirstParentTypeDeclaration() // The parent class of the DisplayClass.
                        .Members.Single(member => member.GetFullName() == targetMethodName);
                }
            }
            else
            {
                targetDeclaration = targetMemberReference.GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);
            }

            if (targetDeclaration == null || !(targetDeclaration is MethodDeclaration))
            {
                throw new InvalidOperationException("The invoked method " + targetMethodName + " can't be found.");
            }


            _stateMachineInvocationBuilder
                .BuildInvocation(targetDeclaration, transformedParameters, context);

            return _stateMachineInvocationBuilder.BuildInvocationWait(targetDeclaration, 1, true, context).Single();
        }
    }
}
