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
using Hast.Transformer.Vhdl.Extensions;

namespace Hast.Transformer.Vhdl.InvokationProxyBuilders
{
    public class InternalInvokationProxyBuilder : IInternalInvokationProxyBuilder
    {
        public IEnumerable<IArchitectureComponent> BuildProxy(
            IEnumerable<IArchitectureComponent> components,
            IVhdlTransformationContext transformationContext)
        {
            // [invoked member name][from component name][invokation instance count]
            var invokedMembers = new Dictionary<string, List<KeyValuePair<string, int>>>();


            // Summarizing which member was invoked with how many instances from which component.
            foreach (var component in components.Where(component => component.OtherMemberMaxInvokationInstanceCounts.Any()))
            {
                foreach (var memberInvokationCount in component.OtherMemberMaxInvokationInstanceCounts)
                {
                    var memberName = memberInvokationCount.Key;

                    List<KeyValuePair<string, int>> invokedFromList;

                    if (!invokedMembers.TryGetValue(memberName, out invokedFromList))
                    {
                        invokedMembers[memberName] = invokedFromList = new List<KeyValuePair<string, int>>();
                    }

                    invokedFromList.Add(new KeyValuePair<string, int>(component.Name, memberInvokationCount.Value));
                }
            }


            var proxyComponents = new List<IArchitectureComponent>(invokedMembers.Count);
            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would 
            // look from a generated state machine.
            var namePrefix = "System.Void Hast::InternalInvokationProxy().";

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
                var memberName = invokedMember.Key;

                var proxyComponent = new ConfigurableComponent(namePrefix + memberName);
                proxyComponents.Add(proxyComponent);
                var bodyBlock = new InlineBlock();


                Func<int, string> getMemberComponentName = index =>
                    ArchitectureComponentNameHelper.CreateIndexedComponentName(memberName, index);

                Func<int, DataObjectReference> getStartVariableReference = index => proxyComponent
                            .CreatePrefixedSegmentedObjectName(ArchitectureComponentNameHelper
                                .CreateStartedSignalName(getMemberComponentName(index)).TrimExtendedVhdlIdDelimiters())
                            .ToVhdlVariableReference();

                Func<int, DataObjectReference> getJustFinishedVariableReference = index =>
                    proxyComponent
                        .CreatePrefixedSegmentedObjectName(getMemberComponentName(index), "justFinished")
                        .ToVhdlVariableReference();


                // How many instances does this member have in form of components, e.g. how many state machines are
                // there for this member?
                var componentCount = transformationContext
                    .GetTransformerConfiguration()
                    .GetMaxInvokationInstanceCountConfigurationForMember(memberName.ToSimpleName()).MaxInvokationInstanceCount;


                // Creating started variables for each indexed component of the member, e.g. all state machines of a 
                // transformed method. These are needed so inside the proxy it's immediately visible if an instance
                // is already started.
                var startedVariablesWriteBlock = new LogicalBlock(
                    new LineComment("Temporary Started variables are needed in place of the original signals so inside the proxy it's immediately visible if an instance is already started."));
                bodyBlock.Add(startedVariablesWriteBlock);

                var justFinishedWriteBlock = new LogicalBlock(
                    new LineComment("JustFinished states should be only kept for one clock cycle, since they are used not to immediately restart a state machine once it finished."));
                bodyBlock.Add(justFinishedWriteBlock);

                for (int i = 0; i < componentCount; i++)
                {
                    var startedVariableReference = getStartVariableReference(i);
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
                                .CreateStartedSignalName(getMemberComponentName(i)).ToVhdlSignalReference()
                        });

                    var finishedVariableReference = getJustFinishedVariableReference(i);
                    proxyComponent.LocalVariables.Add(new Variable
                        {
                            DataType = KnownDataTypes.Boolean,
                            Name = finishedVariableReference.Name
                        });
                    justFinishedWriteBlock.Add(new Assignment
                        {
                            AssignTo = finishedVariableReference,
                            Expression = Value.False
                        });
                }


                // Building the invokation handlers.
                foreach (var invokation in invokedMember.Value)
                {
                    var invokerName = invokation.Key;
                    var instanceCount = invokation.Value;

                    for (int i = 0; i < instanceCount; i++)
                    {
                        proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = new RangedDataType(KnownDataTypes.UnrangedInt)
                                {
                                    RangeMin = 0,
                                    RangeMax = instanceCount
                                },
                                Name = proxyComponent.CreatePrefixedSegmentedObjectName(invokerName, "runningIndex", i.ToString())
                            });

                        proxyComponent.LocalVariables.Add(new Variable
                            {
                                DataType = runningStates,
                                Name = "runningState",
                                InitialValue = waitingForFinishedStateValue
                            });


                        var invokationHandlerBlock = new LogicalBlock(
                            new LineComment("Invokation handler corresponding to " + invokerName));
                        bodyBlock.Add(invokationHandlerBlock);

                        var startedIf = new IfElse
                        {
                            Condition = InvokationHelper.CreateStartedSignalReference(invokerName, memberName, i),
                            True = Empty.Instance
                        };

                        invokationHandlerBlock.Add(startedIf);
                    }
                }


                // Writing Started variables back to signals.
                var startedWriteBackBlock = new LogicalBlock(
                    new LineComment("Writing Started variable values back to signals."));
                for (int i = 0; i < componentCount; i++)
                {
                    startedWriteBackBlock.Add(new Assignment
                        {
                            AssignTo = ArchitectureComponentNameHelper
                                .CreateStartedSignalName(getMemberComponentName(i)).ToVhdlSignalReference(),
                            Expression = getStartVariableReference(i)
                        });
                }
                bodyBlock.Add(startedWriteBackBlock);

                proxyComponent.ProcessNotInReset = bodyBlock;
            }


            return proxyComponents;
        }
    }
}
