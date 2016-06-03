using System;
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
                            var targetComponentName = invokedFromSingleComponent ? 
                                getTargetMemberComponentName(j) : 
                                getTargetMemberComponentName(i);

                            signalConnectionsBlock.Add(new Assignment
                            {
                                AssignTo = ArchitectureComponentNameHelper
                                    .CreateStartedSignalName(targetComponentName)
                                    .ToVhdlSignalReference(),
                                Expression = invokedFromSingleComponent ?
                                    InvocationHelper
                                        .CreateStartedSignalReference(invokerName, targetMemberName, j) :
                                    InvocationHelper
                                        .CreateStartedSignalReference(invokerName, targetMemberName, 0)
                            });

                            signalConnectionsBlock.Add(new Assignment
                            {
                                AssignTo = invokedFromSingleComponent ? 
                                    InvocationHelper
                                        .CreateFinishedSignalReference(invokerName, targetMemberName, j) :
                                    InvocationHelper
                                        .CreateFinishedSignalReference(invokerName, targetMemberName, 0),
                                Expression = ArchitectureComponentNameHelper
                                    .CreateFinishedSignalName(targetComponentName)
                                    .ToVhdlSignalReference()
                            });
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

                    Func<int, DataObjectReference> getJustFinishedVariableReference = index =>
                        proxyComponent
                            .CreatePrefixedSegmentedObjectName(getTargetMemberComponentName(index), "justFinished")
                            .ToVhdlVariableReference();


                    // Creating started variables for each indexed component of the member, e.g. all state machines of a 
                    // transformed method. These are needed so inside the proxy it's immediately visible if an instance
                    // is already started.
                    var startedVariablesWriteBlock = new LogicalBlock(
                        new LineComment("Temporary Started variables are needed in place of the original signals so inside the proxy it's immediately visible if an instance is already started."));
                    bodyBlock.Add(startedVariablesWriteBlock);

                    var justFinishedWriteBlock = new LogicalBlock(
                        new LineComment("JustFinished states should be only kept for one clock cycle, since they are used not to immediately restart a state machine once it finished."));
                    bodyBlock.Add(justFinishedWriteBlock);

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

                        var justFinishedVariableReference = getJustFinishedVariableReference(i);
                        proxyComponent.LocalVariables.Add(new Variable
                        {
                            DataType = KnownDataTypes.Boolean,
                            Name = justFinishedVariableReference.Name
                        });
                        justFinishedWriteBlock.Add(new Assignment
                        {
                            AssignTo = justFinishedVariableReference,
                            Expression = Value.False
                        });
                    }


                    // Building the invocation handlers.
                    foreach (var invocation in invokedFromComponents)
                    {
                        var invokerName = invocation.Key;
                        var invocationInstanceCount = invocation.Value;

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
                                var passedParameters = componentsByName[invokerName]
                                .GetParameterVariables()
                                .Where(parameter => parameter.TargetMemberFullName == targetMemberName && parameter.Index == i);

                                // Chaining together ifs to check all the instances of the target component whether they
                                // are already started.
                                IfElse notStartedComponentSelectingIfElse = null;
                                for (int c = 0; c < targetComponentCount; c++)
                                {
                                    var componentStartVariableReference = getTargetStartedVariableReference(c);

                                    var ifComponentStartedTrue = new InlineBlock(
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
                                        });

                                    var targetParameters =
                                        componentsByName[ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMemberName, c)]
                                        .GetParameterVariables()
                                        .Where(parameter => parameter.TargetMemberFullName == targetMemberName);
                                    foreach (var parameter in passedParameters)
                                    {
                                        ifComponentStartedTrue.Add(new Assignment
                                        {
                                            AssignTo = targetParameters
                                                    .Single(p =>
                                                        p.TargetParameterName == parameter.TargetParameterName && p.IsOwn),
                                            Expression = parameter.ToReference()
                                        });
                                    }

                                    var ifComponentStartedIfElse = new IfElse
                                    {
                                        Condition = new Binary
                                        {
                                            Left = new Binary
                                            {
                                                Left = componentStartVariableReference,
                                                Operator = BinaryOperator.Equality,
                                                Right = Value.False
                                            },
                                            Operator = BinaryOperator.ConditionalAnd,
                                            Right = new Binary
                                            {
                                                Left = getJustFinishedVariableReference(c),
                                                Operator = BinaryOperator.Equality,
                                                Right = Value.False
                                            }
                                        },
                                        True = ifComponentStartedTrue
                                    };

                                    if (notStartedComponentSelectingIfElse != null)
                                    {
                                        notStartedComponentSelectingIfElse.ElseIfs.Add(ifComponentStartedIfElse);
                                    }
                                    else
                                    {
                                        notStartedComponentSelectingIfElse = ifComponentStartedIfElse;
                                    }
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
                                                notStartedComponentSelectingIfElse)
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
                                            AssignTo = getTargetStartedVariableReference(c),
                                            Expression = Value.False
                                        },
                                        new Assignment
                                        {
                                            AssignTo = getJustFinishedVariableReference(c),
                                            Expression = Value.True
                                        });

                                    // Does the target have a return value?
                                    var targetComponent = componentsByName[ArchitectureComponentNameHelper.CreateIndexedComponentName(targetMemberName, c)];
                                    var returnVariable =
                                        targetComponent
                                        .GlobalVariables
                                        .SingleOrDefault(variable => variable.Name == targetComponent.CreateReturnVariableName());
                                    if (returnVariable != null)
                                    {
                                        isFinishedIfTrue.Add(new Assignment
                                        {
                                            AssignTo = componentsByName[invokerName]
                                                    .CreateReturnVariableNameForTargetComponent(targetMemberName, i)
                                                    .ToVhdlVariableReference(),
                                            Expression = returnVariable.ToReference()
                                        });
                                    }

                                    var isFinishedIf = new If
                                    {
                                        Condition = ArchitectureComponentNameHelper
                                            .CreateFinishedSignalName(getTargetMemberComponentName(c))
                                            .ToVhdlSignalReference(),
                                        True = isFinishedIfTrue
                                    };

                                    runningIndexCase.Whens.Add(new CaseWhen
                                    {
                                        Expression = c.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        Body = new List<IVhdlElement> { { isFinishedIf } }
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
                                var runningIndexCase = new Case
                                {
                                    Expression = runningIndexVariableReference
                                };

                                for (int c = 0; c < targetComponentCount; c++)
                                {
                                    runningIndexCase.Whens.Add(new CaseWhen
                                    {
                                        Expression = c.ToVhdlValue(KnownDataTypes.UnrangedInt),
                                        Body = new List<IVhdlElement>
                                        {
                                            {
                                                new Assignment
                                                {
                                                    AssignTo = InvocationHelper
                                                        .CreateFinishedSignalReference(invokerName, targetMemberName, i),
                                                    Expression = Value.False
                                                }
                                            }
                                        }
                                    });
                                }

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
                                                    runningIndexCase)
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


                    // Writing Started variables back to signals.
                    var startedWriteBackBlock = new LogicalBlock(
                        new LineComment("Writing Started variable values back to signals."));
                    for (int i = 0; i < targetComponentCount; i++)
                    {
                        startedWriteBackBlock.Add(new Assignment
                        {
                            AssignTo = ArchitectureComponentNameHelper
                                    .CreateStartedSignalName(getTargetMemberComponentName(i)).ToVhdlSignalReference(),
                            Expression = getTargetStartedVariableReference(i)
                        });
                    }
                    bodyBlock.Add(startedWriteBackBlock);

                    proxyComponent.ProcessNotInReset = bodyBlock; 
                }
            }


            return proxyComponents;
        }
    }
}
