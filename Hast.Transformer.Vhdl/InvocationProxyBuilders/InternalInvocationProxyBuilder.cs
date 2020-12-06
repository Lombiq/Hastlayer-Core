using Hast.Common.Configuration;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public class InternalInvocationProxyBuilder : IInternalInvocationProxyBuilder
    {
        public IEnumerable<IArchitectureComponent> BuildProxy(
            IEnumerable<IArchitectureComponent> components,
            IVhdlTransformationContext transformationContext)
        {
            var componentsByName = components.ToDictionary(component => component.Name);

            // [invoked member declaration][from component name][invocation instance count]
            var invokedMembers = new Dictionary<EntityDeclaration, List<KeyValuePair<string, int>>>();

            // Summarizing which member was invoked with how many instances from which component.
            foreach (var component in components.Where(component => component.OtherMemberMaxInvocationInstanceCounts.Any()))
            {
                foreach (var memberInvocationCount in component.OtherMemberMaxInvocationInstanceCounts)
                {
                    var targetMember = memberInvocationCount.Key;

                    if (!invokedMembers.TryGetValue(targetMember, out var invokedFromList))
                    {
                        invokedMembers[targetMember] = invokedFromList = new List<KeyValuePair<string, int>>();
                    }

                    invokedFromList.Add(new KeyValuePair<string, int>(component.Name, memberInvocationCount.Value));
                }
            }

            foreach (var invokedMember in invokedMembers)
            {
                var memberFullName = invokedMember.Key.GetFullName();

                // Is this a recursive member? If yes then remove the last component (i.e. the one with the highest
                // index, the deepest one in the call stack) from the list of invoking ones, because that won't invoke
                // anything else.
                var maxIndexComponents = invokedMember.Value
                    .Where(component => component.Key.StartsWith(memberFullName, StringComparison.InvariantCulture))
                    .OrderByDescending(component => component.Key);

                if (maxIndexComponents.Any())
                {
                    invokedMember.Value.Remove(maxIndexComponents.First());
                }
            }

            var proxyComponents = new List<IArchitectureComponent>(invokedMembers.Count);
            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would
            // look from a generated state machine.
            const string namePrefix = "System.Void Hast::InternalInvocationProxy().";

            var waitingForStartedStateValue = "WaitingForStarted".ToVhdlIdValue();
            var waitingForFinishedStateValue = "WaitingForFinished".ToVhdlIdValue();
            var afterFinishedStateValue = "AfterFinished".ToVhdlIdValue();
            var booleanArrayType = new ArrayType
            {
                ElementType = KnownDataTypes.Boolean,
                // Prefixing the name so it won't clash with boolean arrays created by the transforming logic.
                Name = ("InternalInvocationProxy_" +
                    ArrayHelper.CreateArrayTypeName(KnownDataTypes.Boolean).TrimExtendedVhdlIdDelimiters())
                    .ToExtendedVhdlId(),
            };
            var runningStates = new VhdlBuilder.Representation.Declaration.Enum
            {
                Name = (namePrefix + "_RunningStates").ToExtendedVhdlId(),
            };
            runningStates.Values.Add(waitingForStartedStateValue);
            runningStates.Values.Add(waitingForFinishedStateValue);
            runningStates.Values.Add(afterFinishedStateValue);
            proxyComponents.Add(new BasicComponent((namePrefix + "_CommonDeclarations").ToExtendedVhdlId())
            {
                Declarations = new InlineBlock(booleanArrayType, runningStates),
            });

            foreach (var invokedMember in invokedMembers)
            {
                var targetMember = invokedMember.Key;
                var invokedFromComponents = invokedMember.Value;
                var targetMemberName = targetMember.GetFullName();
                var proxyComponentName = namePrefix + targetMemberName;

                // How many instances does this member have in form of components, e.g. how many state machines are
                // there for this member? This is not necessarily the same as the invocation instance count.
                var targetComponentCount = transformationContext
                    .GetTransformerConfiguration()
                    .GetMaxInvocationInstanceCountConfigurationForMember(targetMember)
                    .MaxInvocationInstanceCount;

                Func<int, string> getTargetMemberComponentName = index =>
                    ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMemberName, index);

                Func<string, int, int, IEnumerable<IVhdlElement>> buildInParameterAssignments =
                    (invokerName, invokerIndex, targetIndex) =>
                    {
                        var passedParameters = componentsByName[invokerName]
                            .GetOutParameterSignals()
                            .Where(parameter =>
                                parameter.TargetMemberFullName == targetMemberName &&
                                parameter.Index == invokerIndex &&
                                !parameter.IsOwn);

                        var targetComponentName = ArchitectureComponentNameHelper
                            .CreateIndexedComponentName(targetMemberName, targetIndex);
                        var targetParameters =
                            componentsByName[targetComponentName]
                            .GetInParameterSignals()
                            .Where(parameter =>
                                parameter.TargetMemberFullName == targetMemberName &&
                                parameter.IsOwn);

                        if (!targetParameters.Any()) return Enumerable.Empty<IVhdlElement>();

                        return passedParameters.Select(parameter => new Assignment
                        {
                            AssignTo = targetParameters
                                        .Single(p => p.TargetParameterName == parameter.TargetParameterName),
                            Expression = parameter.ToReference(),
                        });
                    };

                Func<string, int, int, IVhdlElement> buildReturnAssigment =
                    (invokerName, invokerIndex, targetIndex) =>
                    {
                        // Does the target have a return value?
                        var targetComponentName = ArchitectureComponentNameHelper
                            .CreateIndexedComponentName(targetMemberName, targetIndex);
                        var targetComponent = componentsByName[targetComponentName];
                        var returnSignal =
                            targetComponent
                            .InternallyDrivenSignals
                            .SingleOrDefault(signal => signal.Name == targetComponent.CreateReturnSignalReference().Name);

                        if (returnSignal == null) return null;

                        return new Assignment
                        {
                            AssignTo = componentsByName[invokerName]
                                    .CreateReturnSignalReferenceForTargetComponent(targetMemberName, invokerIndex),
                            Expression = returnSignal.ToReference(),
                        };
                    };

                Func<string, int, int, IEnumerable<IVhdlElement>> buildOutParameterAssignments =
                    (invokerName, invokerIndex, targetIndex) =>
                    {
                        var targetComponentName = ArchitectureComponentNameHelper
                            .CreateIndexedComponentName(targetMemberName, targetIndex);
                        var passedBackParameters =
                            componentsByName[targetComponentName]
                            .GetOutParameterSignals()
                            .Where(parameter =>
                                parameter.TargetMemberFullName == targetMemberName &&
                                parameter.IsOwn);

                        var receivingParameters = componentsByName[invokerName]
                                    .GetInParameterSignals()
                                    .Where(parameter =>
                                        parameter.TargetMemberFullName == targetMemberName &&
                                        parameter.Index == invokerIndex &&
                                        !parameter.IsOwn);

                        if (!receivingParameters.Any()) return Enumerable.Empty<IVhdlElement>();

                        return passedBackParameters.Select(parameter => new Assignment
                        {
                            AssignTo = receivingParameters
                                        .Single(p => p.TargetParameterName == parameter.TargetParameterName),
                            Expression = parameter.ToReference(),
                        });
                    };

                // Is this member's component only invoked from a single other component? Because then we don't need a
                // full invocation proxy: local Start and Finished signals can be directly connected to the target
                // component's signals.
                // (Every member at this point is invoked at least once.)
                var invokedFromSingleComponent = invokedFromComponents.Take(2).Count() == 1;

                // Is this member's component invoked from multiple components, but just once from each of them and there
                // are a sufficient number of target components available? Then we can pair them together.
                var invocationsCount = 0;
                var invocationsCanBePaired =
                    !invokedFromSingleComponent &&
                    !invokedFromComponents.Any(componentInvocation => componentInvocation.Value > 1) &&
                    (invocationsCount = invokedFromComponents.Sum(invokingComponent => invokingComponent.Value)) <= targetComponentCount;

                if (invokedFromSingleComponent || invocationsCanBePaired)
                {
                    var proxyComponent = new BasicComponent(proxyComponentName);
                    var signalConnectionsBlock = new InlineBlock();
                    proxyComponent.Body = signalConnectionsBlock;
                    proxyComponents.Add(proxyComponent);

                    for (int i = 0; i < invokedFromComponents.Count; i++)
                    {
                        var invokedFromComponent = invokedFromComponents[i];
                        var invokerName = invokedFromComponent.Key;

                        for (int j = 0; j < invokedFromComponent.Value; j++)
                        {
                            var targetIndex = invokedFromSingleComponent ? j : i;
                            var targetComponentName = getTargetMemberComponentName(targetIndex);
                            var invokerIndex = invokedFromSingleComponent ? j : 0;

                            signalConnectionsBlock.Add(new LineComment(
                                "Signal connections for " + invokerName + " (#" + targetIndex + "):"));

                            signalConnectionsBlock.Add(new Assignment
                            {
                                AssignTo = ArchitectureComponentNameHelper
                                    .CreateStartedSignalName(targetComponentName)
                                    .ToVhdlSignalReference(),
                                Expression = InvocationHelper
                                        .CreateStartedSignalReference(invokerName, targetMemberName, invokerIndex),
                            });

                            signalConnectionsBlock.Body.AddRange(buildInParameterAssignments(invokerName, invokerIndex, targetIndex));

                            signalConnectionsBlock.Add(new Assignment
                            {
                                AssignTo = InvocationHelper
                                        .CreateFinishedSignalReference(invokerName, targetMemberName, invokerIndex),
                                Expression = ArchitectureComponentNameHelper
                                    .CreateFinishedSignalName(targetComponentName)
                                    .ToVhdlSignalReference(),
                            });

                            var returnAssignment = buildReturnAssigment(invokerName, invokerIndex, targetIndex);
                            if (returnAssignment != null) signalConnectionsBlock.Add(returnAssignment);
                            signalConnectionsBlock.Body.AddRange(buildOutParameterAssignments(invokerName, invokerIndex, targetIndex));
                        }
                    }
                }
                else
                {
                    // Note that the below implementation does not work perfectly. As the number of components increases
                    // it becomes unstable. For example the CalculateFibonacchiSeries sample without debug memory writes
                    // won't finish while the CalculateFactorial with them will work properly.
                    var proxyComponent = new ConfigurableComponent(proxyComponentName);
                    proxyComponents.Add(proxyComponent);

                    var proxyInResetBlock = new InlineBlock();
                    proxyComponent.ProcessInReset = proxyInResetBlock;
                    var bodyBlock = new InlineBlock();

                    DataObjectReference targetAvailableIndicatorVariableReference = null;
                    SizedDataType targetAvailableIndicatorDataType = null;

                    if (targetComponentCount > 1)
                    {
                        // Creating a boolean vector where each of the elements will indicate whether the target
                        // component with that index is available and can be started. I.e. targetAvailableIndicator(0)
                        // being true tells that the target component with index 0 can be started.
                        // All this is necessary to avoid ifs with large conditions which would cause timing errors
                        // with more than cca. 20 components. This implementation can be better implemented with
                        // parallel paths.
                        targetAvailableIndicatorVariableReference = proxyComponent
                            .CreatePrefixedSegmentedObjectName("targetAvailableIndicator")
                            .ToVhdlVariableReference();
                        targetAvailableIndicatorDataType = new SizedDataType
                        {
                            Name = booleanArrayType.Name,
                            TypeCategory = DataTypeCategory.Array,
                            Size = targetComponentCount,
                        };
                        proxyComponent.LocalVariables.Add(new Variable
                        {
                            DataType = targetAvailableIndicatorDataType,
                            Name = targetAvailableIndicatorVariableReference.Name,
                            InitialValue = ("others => " + KnownDataTypes.Boolean.DefaultValue.ToVhdl())
                                .ToVhdlValue(targetAvailableIndicatorDataType),
                        });

                        bodyBlock.Add(new LineComment(
                            "Building a boolean array where each of the elements will indicate whether the component with the given index should be started next."));

                        for (int c = 0; c < targetComponentCount; c++)
                        {
                            var selectorConditions = new List<IVhdlElement>
                            {
                                new Binary
                                {
                                    Left = ArchitectureComponentNameHelper
                                    .CreateStartedSignalName(getTargetMemberComponentName(c))
                                    .ToVhdlSignalReference(),
                                    Operator = BinaryOperator.Equality,
                                    Right = Value.False,
                                },
                            };
                            for (int s = c + 1; s < targetComponentCount; s++)
                            {
                                selectorConditions.Add(new Binary
                                {
                                    Left = ArchitectureComponentNameHelper
                                        .CreateStartedSignalName(getTargetMemberComponentName(s))
                                        .ToVhdlSignalReference(),
                                    Operator = BinaryOperator.Equality,
                                    Right = Value.True,
                                });
                            }

                            // Assignments to the boolean array where each element will indicate whether the
                            // component with the given index can be started.
                            bodyBlock.Add(
                                new Assignment
                                {
                                    AssignTo = new ArrayElementAccess
                                    {
                                        ArrayReference = targetAvailableIndicatorVariableReference,
                                        IndexExpression = c.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                    },
                                    Expression = BinaryChainBuilder.BuildBinaryChain(selectorConditions, BinaryOperator.And),
                                });
                        }
                    }

                    // Building the invocation handlers.
                    foreach (var invocation in invokedFromComponents)
                    {
                        var invokerName = invocation.Key;
                        var invocationInstanceCount = invocation.Value;

                        // Check if the component would invoke itself. This can happen with recursive calls.
                        Func<int, IVhdlElement> createNullOperationIfTargetComponentEqualsInvokingComponent = index =>
                        {
                            if (getTargetMemberComponentName(index) != invokerName) return null;

                            return new InlineBlock(
                                new LineComment("The component can't invoke itself, so not putting anything here."),
                                new Terminated(Null.Instance));
                        };

                        for (int i = 0; i < invocationInstanceCount; i++)
                        {
                            var runningIndexName = proxyComponent
                                .CreatePrefixedSegmentedObjectName(invokerName, "runningIndex", i.ToString(CultureInfo.InvariantCulture));
                            var runningIndexVariableReference = runningIndexName.ToVhdlVariableReference();
                            proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = new RangedDataType(KnownDataTypes.UnrangedInt)
                                {
                                    RangeMin = 0,
                                    RangeMax = targetComponentCount - 1,
                                },
                                Name = runningIndexName,
                            });

                            var runningStateVariableName = proxyComponent
                                .CreatePrefixedSegmentedObjectName(invokerName, "runningState", i.ToString(CultureInfo.InvariantCulture));
                            var runningStateVariableReference = runningStateVariableName.ToVhdlVariableReference();
                            proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = runningStates,
                                Name = runningStateVariableName,
                                InitialValue = waitingForStartedStateValue,
                            });

                            var invocationHandlerBlock = new LogicalBlock(
                                new LineComment(
                                    "Invocation handler #" + i.ToString(CultureInfo.InvariantCulture) +
                                    " out of " + invocationInstanceCount.ToString(CultureInfo.InvariantCulture) +
                                    " corresponding to " + invokerName));
                            bodyBlock.Add(invocationHandlerBlock);

                            var runningStateCase = new Case
                            {
                                Expression = runningStateVariableReference,
                            };

                            // WaitingForStarted state
                            {
                                var waitingForStartedInnnerBlock = new InlineBlock();

                                Func<int, IVhdlElement> createComponentAvailableBody = targetIndex =>
                                {
                                    var componentAvailableBody = createNullOperationIfTargetComponentEqualsInvokingComponent(targetIndex);

                                    if (componentAvailableBody != null) return componentAvailableBody;

                                    var componentAvailableBodyBlock = new InlineBlock(
                                        new Assignment
                                        {
                                            AssignTo = runningStateVariableReference,
                                            Expression = waitingForFinishedStateValue,
                                        },
                                        new Assignment
                                        {
                                            AssignTo = runningIndexVariableReference,
                                            Expression = targetIndex.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        },
                                        new Assignment
                                        {
                                            AssignTo = ArchitectureComponentNameHelper
                                                .CreateStartedSignalName(getTargetMemberComponentName(targetIndex))
                                                .ToVhdlSignalReference(),
                                            Expression = Value.True,
                                        });

                                    if (targetComponentCount > 1)
                                    {
                                        componentAvailableBodyBlock.Add(new Assignment
                                        {
                                            AssignTo = new ArrayElementAccess
                                            {
                                                ArrayReference = targetAvailableIndicatorVariableReference,
                                                IndexExpression = targetIndex.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                            },
                                            Expression = Value.False,
                                        });
                                    }

                                    componentAvailableBodyBlock.Body.AddRange(buildInParameterAssignments(invokerName, i, targetIndex));

                                    return componentAvailableBodyBlock;
                                };

                                if (targetComponentCount == 1)
                                {
                                    // If there is only a single target component then the implementation can be simpler.
                                    // Also having a case targetAvailableIndicator is (true) when =>... isn't syntactically
                                    // correct, single-element arrays can't be matched like this. "[Synth 8-2778] type
                                    // error near true ; expected type internalinvocationproxy_boolean_array".

                                    waitingForStartedInnnerBlock.Add(createComponentAvailableBody(0));
                                }
                                else
                                {
                                    var availableTargetSelectingCase = new Case
                                    {
                                        Expression = targetAvailableIndicatorVariableReference,
                                    };

                                    for (int c = 0; c < targetComponentCount; c++)
                                    {
                                        availableTargetSelectingCase.Whens.Add(new CaseWhen(
                                            expression: CreateBooleanIndicatorValue(targetAvailableIndicatorDataType, c),
                                            body: new List<IVhdlElement> { { createComponentAvailableBody(c) } }));
                                    }

                                    availableTargetSelectingCase.Whens.Add(new CaseWhen(
                                        expression: "others".ToVhdlIdValue(),
                                        body: new List<IVhdlElement> { { Null.Instance.Terminate() } }));

                                    waitingForStartedInnnerBlock.Add(availableTargetSelectingCase);
                                }

                                runningStateCase.Whens.Add(new CaseWhen(
                                    expression: waitingForStartedStateValue,
                                    body: new List<IVhdlElement>
                                    {
                                        {
                                            new If
                                            {
                                                Condition = InvocationHelper
                                                    .CreateStartedSignalReference(invokerName, targetMemberName, i),
                                                True = new InlineBlock(
                                                new Assignment
                                                {
                                                    AssignTo = InvocationHelper
                                                        .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                                    Expression = Value.False
                                                },
                                                waitingForStartedInnnerBlock),
                                            }
                                        },
                                    }));
                            }

                            // WaitingForFinished state
                            {
                                var runningIndexCase = new Case
                                {
                                    Expression = runningIndexVariableReference,
                                };

                                for (int c = 0; c < targetComponentCount; c++)
                                {
                                    var caseWhenBody = createNullOperationIfTargetComponentEqualsInvokingComponent(c);

                                    if (caseWhenBody == null)
                                    {
                                        var isFinishedIfTrue = new InlineBlock(
                                            new Assignment
                                            {
                                                AssignTo = runningStateVariableReference,
                                                Expression = afterFinishedStateValue,
                                            },
                                            new Assignment
                                            {
                                                AssignTo = InvocationHelper
                                                    .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                                Expression = Value.True,
                                            },
                                            new Assignment
                                            {
                                                AssignTo = ArchitectureComponentNameHelper
                                                    .CreateStartedSignalName(getTargetMemberComponentName(c))
                                                    .ToVhdlSignalReference(),
                                                Expression = Value.False,
                                            });

                                        var returnAssignment = buildReturnAssigment(invokerName, i, c);
                                        if (returnAssignment != null) isFinishedIfTrue.Add(returnAssignment);

                                        isFinishedIfTrue.Body.AddRange(buildOutParameterAssignments(invokerName, i, c));

                                        caseWhenBody = new If
                                        {
                                            Condition = ArchitectureComponentNameHelper
                                                .CreateFinishedSignalName(getTargetMemberComponentName(c))
                                                .ToVhdlSignalReference(),
                                            True = isFinishedIfTrue,
                                        };
                                    }

                                    runningIndexCase.Whens.Add(new CaseWhen(
                                        expression: c.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        body: new List<IVhdlElement> { { caseWhenBody } }));
                                }

                                runningStateCase.Whens.Add(new CaseWhen(
                                    expression: waitingForFinishedStateValue,
                                    body: new List<IVhdlElement>
                                    {
                                        { runningIndexCase },
                                    }));
                            }

                            // AfterFinished state
                            {
                                runningStateCase.Whens.Add(new CaseWhen(
                                    expression: afterFinishedStateValue,
                                    body: new List<IVhdlElement>
                                    {
                                        {
                                            new LineComment("Invoking components need to pull down the Started signal to false.")
                                        },
                                        {
                                            new IfElse
                                            {
                                                Condition = new Binary
                                                {
                                                    Left = InvocationHelper
                                                        .CreateStartedSignalReference(invokerName, targetMemberName, i),
                                                    Operator = BinaryOperator.Equality,
                                                    Right = Value.False,
                                                },
                                                True = new InlineBlock(
                                                    new Assignment
                                                    {
                                                        AssignTo = runningStateVariableReference,
                                                        Expression = waitingForStartedStateValue
                                                    },
                                                    new Assignment
                                                    {
                                                        AssignTo = InvocationHelper
                                                            .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                                        Expression = Value.False
                                                    }),
                                            }
                                        },
                                    }));
                            }

                            // Adding reset for the finished signal.
                            proxyInResetBlock.Add(new Assignment
                            {
                                AssignTo = InvocationHelper
                                    .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                Expression = Value.False,
                            });

                            invocationHandlerBlock.Add(runningStateCase);
                        }
                    }

                    proxyComponent.ProcessNotInReset = bodyBlock;
                }
            }

            return proxyComponents;
        }

        private static IVhdlElement CreateBooleanIndicatorValue(SizedDataType targetAvailableIndicatorDataType, int indicatedIndex)
        {
            // This will create a boolean array where the everything is false except for the element with the given index.

            var booleanArray = Enumerable.Repeat(Value.False, targetAvailableIndicatorDataType.Size).ToArray();
            // Since the bit vector is downto the rightmost element is the 0th.
            booleanArray[targetAvailableIndicatorDataType.Size - 1 - indicatedIndex] = Value.True;
            return new Value
            {
                DataType = targetAvailableIndicatorDataType,
                EvaluatedContent = new InlineBlock(booleanArray),
            };
        }
    }
}
