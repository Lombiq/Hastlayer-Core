﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Models;
using Hast.Common.Configuration;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Hast.Transformer.Vhdl.Helpers;

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

                    List<KeyValuePair<string, int>> invokedFromList;

                    if (!invokedMembers.TryGetValue(targetMember, out invokedFromList))
                    {
                        invokedMembers[targetMember] = invokedFromList = new List<KeyValuePair<string, int>>();
                    }

                    invokedFromList.Add(new KeyValuePair<string, int>(component.Name, memberInvocationCount.Value));
                }
            }


            var proxyComponents = new List<IArchitectureComponent>(invokedMembers.Count);
            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would 
            // look from a generated state machine.
            var namePrefix = "System.Void Hast::InternalInvocationProxy().";

            var waitingForStartedStateValue = "WaitingForStarted".ToVhdlIdValue();
            var waitingForFinishedStateValue = "WaitingForFinished".ToVhdlIdValue();
            var afterFinishedStateValue = "AfterFinished".ToVhdlIdValue();
            var runningStates = new Hast.VhdlBuilder.Representation.Declaration.Enum
            {
                Name = (namePrefix + "_RunningStates").ToExtendedVhdlId(),
                Values = new[]
                {
                    waitingForStartedStateValue,
                    waitingForFinishedStateValue,
                    afterFinishedStateValue
                }.ToList()
            };
            proxyComponents.Add(new BasicComponent(runningStates.Name) { Declarations = runningStates });


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

                Func<string, int, int, IEnumerable<IVhdlElement>> buildParameterAssignments =
                    (invokerName, invokerIndex, targetIndex) =>
                    {
                        var passedParameters = componentsByName[invokerName]
                            .GetParameterSignals()
                            .Where(parameter =>
                                parameter.TargetMemberFullName == targetMemberName && 
                                parameter.Index == invokerIndex && 
                                !parameter.IsOwn);

                        var targetComponentName = ArchitectureComponentNameHelper
                            .CreateIndexedComponentName(targetMemberName, targetIndex);
                        var targetParameters =
                            componentsByName[targetComponentName]
                            .GetParameterSignals()
                            .Where(parameter => parameter.TargetMemberFullName == targetMemberName);

                        return passedParameters.Select(parameter => new Assignment
                        {
                            AssignTo = targetParameters
                                        .Single(p =>
                                            p.TargetParameterName == parameter.TargetParameterName && p.IsOwn),
                            Expression = parameter.ToReference()
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
                            Expression = returnSignal.ToReference()
                        };
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
                    invokedFromComponents
                        .FirstOrDefault(componentInvocation => componentInvocation.Value > 1)
                        .Equals(default(KeyValuePair<string, int>)) &&
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
                                        .CreateStartedSignalReference(invokerName, targetMemberName, invokerIndex)
                            });

                            signalConnectionsBlock.Body.AddRange(buildParameterAssignments(invokerName, invokerIndex, targetIndex));

                            signalConnectionsBlock.Add(new Assignment
                            {
                                AssignTo = InvocationHelper
                                        .CreateFinishedSignalReference(invokerName, targetMemberName, invokerIndex),
                                Expression = ArchitectureComponentNameHelper
                                    .CreateFinishedSignalName(targetComponentName)
                                    .ToVhdlSignalReference()
                            });

                            var returnAssignment = buildReturnAssigment(invokerName, invokerIndex, targetIndex);
                            if (returnAssignment != null) signalConnectionsBlock.Add(returnAssignment);
                        }
                    }
                }
                else
                {
                    var proxyComponent = new ConfigurableComponent(proxyComponentName);
                    proxyComponents.Add(proxyComponent);

                    var proxyInResetBlock = new InlineBlock();
                    proxyComponent.ProcessInReset = proxyInResetBlock;
                    var bodyBlock = new InlineBlock();


                    Func<int, DataObjectReference> getTargetStartedVariableReference = index => proxyComponent
                        .CreatePrefixedSegmentedObjectName(ArchitectureComponentNameHelper
                            .CreateStartedSignalName(getTargetMemberComponentName(index)).TrimExtendedVhdlIdDelimiters())
                        .ToVhdlVariableReference();


                    // Creating started variables for each indexed component of the member, e.g. all state machines of a 
                    // transformed method. These are needed so inside the proxy it's immediately visible if an instance
                    // is already started.
                    var startedVariablesWriteBlock = new LogicalBlock(
                        new LineComment("Temporary Started variables are needed in place of the original signals so inside the proxy it's immediately visible if an instance is already started."),
                        new LineComment("Only true values are written to these, but not false, those are directly written to the original signals. This is so instances are not immediately restarted."));
                    bodyBlock.Add(startedVariablesWriteBlock);

                    for (int i = 0; i < targetComponentCount; i++)
                    {
                        var startedVariableReference = getTargetStartedVariableReference(i);
                        proxyComponent.LocalVariables.Add(new Variable
                        {
                            DataType = KnownDataTypes.Boolean,
                            Name = startedVariableReference.Name,
                            InitialValue = Value.False
                        });
                        startedVariablesWriteBlock.Add(new Assignment
                        {
                            AssignTo = startedVariableReference,
                            Expression = ArchitectureComponentNameHelper
                                    .CreateStartedSignalName(getTargetMemberComponentName(i)).ToVhdlSignalReference()
                        });
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
                                .CreatePrefixedSegmentedObjectName(invokerName, "runningIndex", i.ToString());
                            var runningIndexVariableReference = runningIndexName.ToVhdlVariableReference();
                            proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = new RangedDataType(KnownDataTypes.UnrangedInt)
                                {
                                    RangeMin = 0,
                                    RangeMax = targetComponentCount - 1
                                },
                                Name = runningIndexName
                            });

                            var runningStateVariableName = proxyComponent
                                .CreatePrefixedSegmentedObjectName(invokerName, "runningState", i.ToString());
                            var runningStateVariableReference = runningStateVariableName.ToVhdlVariableReference();
                            proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = runningStates,
                                Name = runningStateVariableName,
                                InitialValue = waitingForStartedStateValue
                            });


                            var invocationHandlerBlock = new LogicalBlock(
                                new LineComment(
                                    "Invocation handler #" + i.ToString() +
                                    " out of " + invocationInstanceCount.ToString() +
                                    " corresponding to " + invokerName));
                            bodyBlock.Add(invocationHandlerBlock);


                            var runningStateCase = new Case
                            {
                                Expression = runningStateVariableReference
                            };


                            // WaitingForStarted state
                            {
                                // A series of ifs to check all the instances of the target component whether they are
                                // already started.
                                var notStartedComponentSelectingIfsBlock = new InlineBlock();

                                for (int c = 0; c < targetComponentCount; c++)
                                {
                                    var componentStartVariableReference = getTargetStartedVariableReference(c);
                                    var ifComponentStartedTrue = createNullOperationIfTargetComponentEqualsInvokingComponent(c);

                                    if (ifComponentStartedTrue == null)
                                    {
                                        var ifComponentStartedTrueBlock = new InlineBlock(
                                            new Assignment
                                            {
                                                AssignTo = runningStateVariableReference,
                                                Expression = waitingForFinishedStateValue
                                            },
                                            new Assignment
                                            {
                                                AssignTo = runningIndexVariableReference,
                                                Expression = c.ToVhdlValue(KnownDataTypes.UnrangedInt)
                                            },
                                            new Assignment
                                            {
                                                AssignTo = componentStartVariableReference,
                                                Expression = Value.True
                                            },
                                            new Assignment
                                            {
                                                AssignTo = ArchitectureComponentNameHelper
                                                    .CreateStartedSignalName(getTargetMemberComponentName(c)).ToVhdlSignalReference(),
                                                Expression = Value.True
                                            });

                                        ifComponentStartedTrueBlock.Body.AddRange(buildParameterAssignments(invokerName, i, c));

                                        ifComponentStartedTrue = ifComponentStartedTrueBlock;
                                    }

                                    var selectorConditions = new List<IVhdlElement>();
                                    selectorConditions.Add(new Binary
                                    {
                                        Left = componentStartVariableReference,
                                        Operator = BinaryOperator.Equality,
                                        Right = Value.False
                                    });
                                    for (int s = c + 1; s < targetComponentCount; s++)
                                    {
                                        selectorConditions.Add(new Binary
                                        {
                                            Left = getTargetStartedVariableReference(s),
                                            Operator = BinaryOperator.Equality,
                                            Right = Value.True
                                        });
                                    }

                                    notStartedComponentSelectingIfsBlock.Add(new IfElse
                                    {
                                        Condition = BinaryChainBuilder.BuildBinaryChain(selectorConditions, BinaryOperator.ConditionalAnd),
                                        True = ifComponentStartedTrue
                                    });
                                }

                                runningStateCase.Whens.Add(new CaseWhen
                                {
                                    Expression = waitingForStartedStateValue,
                                    Body = new List<IVhdlElement>
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
                                                notStartedComponentSelectingIfsBlock)
                                            }
                                        }
                                    }
                                });
                            }


                            // WaitingForFinished state
                            {
                                var runningIndexCase = new Case
                                {
                                    Expression = runningIndexVariableReference
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
                                                Expression = afterFinishedStateValue
                                            },
                                            new Assignment
                                            {
                                                AssignTo = InvocationHelper
                                                    .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                                Expression = Value.True
                                            },
                                            new Assignment
                                            {
                                                AssignTo = ArchitectureComponentNameHelper
                                                    .CreateStartedSignalName(getTargetMemberComponentName(c))
                                                    .ToVhdlSignalReference(),
                                                Expression = Value.False
                                            });

                                        var returnAssignment = buildReturnAssigment(invokerName, i, c);
                                        if (returnAssignment != null) isFinishedIfTrue.Add(returnAssignment);

                                        caseWhenBody = new If
                                        {
                                            Condition = ArchitectureComponentNameHelper
                                                .CreateFinishedSignalName(getTargetMemberComponentName(c))
                                                .ToVhdlSignalReference(),
                                            True = isFinishedIfTrue
                                        };
                                    }

                                    runningIndexCase.Whens.Add(new CaseWhen
                                    {
                                        Expression = c.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        Body = new List<IVhdlElement> { { caseWhenBody } }
                                    });
                                }

                                runningStateCase.Whens.Add(new CaseWhen
                                {
                                    Expression = waitingForFinishedStateValue,
                                    Body = new List<IVhdlElement>
                                    {
                                        { runningIndexCase }
                                    }
                                });
                            }


                            // AfterFinished state
                            {
                                runningStateCase.Whens.Add(new CaseWhen
                                {
                                    Expression = afterFinishedStateValue,
                                    Body = new List<IVhdlElement>
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
                                                    Right = Value.False
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
                                                    })
                                            }
                                        }
                                    }
                                });
                            }


                            // Adding reset for the finished signal.
                            proxyInResetBlock.Add(new Assignment
                            {
                                AssignTo = InvocationHelper
                                    .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                Expression = Value.False
                            });

                            invocationHandlerBlock.Add(runningStateCase);
                        }
                    }


                    proxyComponent.ProcessNotInReset = bodyBlock;
                }
            }


            return proxyComponents;
        }


        private static IVhdlElement CreateBinaryIndicatorValue(int indicatedIndex, int size)
        {
            // This will create a binary array where the everything until the 1 is filled with dashes (don't care values)
            // and everything after it with zeros like: "--1000".
            var binaryArray = Enumerable.Repeat('-', size).ToArray();
            // Since the bit vector is downto the rightmost element is the 0th.
            binaryArray[size - 1 - indicatedIndex] = '1';
            for (int i = size - indicatedIndex; i < size; i++)
            {
                binaryArray[i] = '0';
            }
            return ("\"" + string.Join("", binaryArray) + "\"").ToVhdlValue(KnownDataTypes.Identifier);
        }
    }
}
