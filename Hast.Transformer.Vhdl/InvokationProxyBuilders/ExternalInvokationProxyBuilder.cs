using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Orchard;
using Hast.VhdlBuilder.Extensions;
using ICSharpCode.NRefactory.CSharp;
using Hast.Common.Extensions;

namespace Hast.Transformer.Vhdl.InvokationProxyBuilders
{
    public class ExternalInvokationProxyBuilder : IExternalInvokationProxyBuilder
    {
        public IArchitectureComponent BuildProxy(
            IEnumerable<IMemberTransformerResult> interfaceMemberResults,
            MemberIdTable memberIdTable)
        {
            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would look 
            // from a generated state machine.
            var proxyComponent = new ConfigurableComponent("System.Void Hast::ExternalInvokationProxy");


            // Since the Finished port is an out port, it can't be read. Adding an internal proxy signal so we can also 
            // read it.
            var finishedSignal = new Signal
            {
                Name = "FinishedInternal".ToExtendedVhdlId(),
                DataType = KnownDataTypes.StdLogic,
                InitialValue = Value.ZeroCharacter
            };
            proxyComponent.Signals.Add(finishedSignal);
            var finishedSignalReference = finishedSignal.ToReference();
            proxyComponent.BeginBodyWith = new Assignment
            {
                AssignTo = CommonPortNames.Finished.ToVhdlSignalReference(),
                Expression = finishedSignalReference
            };


            var memberSelectingCase = new Case { Expression = CommonPortNames.MemberId.ToVhdlIdValue() };

            foreach (var interfaceMemberResult in interfaceMemberResults)
            {
                var memberName = interfaceMemberResult.Member.GetFullName();
                var memberId = memberIdTable.LookupMemberId(memberName);
                proxyComponent.OtherMemberMaxInvokationInstanceCounts[memberName] = 1;
                var when = new When { Expression = new Value { DataType = KnownDataTypes.Int32, Content = memberId.ToString() } };


                var waitForInvokationFinishedIfElse = InvokationHelper.CreateWaitForInvokationFinished(proxyComponent, memberName, 1);

                waitForInvokationFinishedIfElse.True.Add(new Assignment
                    {
                        AssignTo = finishedSignalReference,
                        Expression = Value.OneCharacter
                    });

                when.Add(new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = InvokationHelper.CreateStartedSignalReference(proxyComponent, memberName, 0),
                            Operator = Operator.Equality,
                            Right = Value.False
                        },
                        True = InvokationHelper.CreateInvokationStart(proxyComponent, memberName, 0),
                        Else = waitForInvokationFinishedIfElse
                    });
                ;


                memberSelectingCase.Whens.Add(when);
            }

            memberSelectingCase.Whens.Add(new When { Expression = new Value { DataType = KnownDataTypes.Identifier, Content = "others" } });

            var startedPortReference = CommonPortNames.Started.ToVhdlSignalReference();
            proxyComponent.ProcessNotInReset = new IfElse
            {
                Condition = new Binary
                {
                    Left = new Binary
                    {
                        Left = startedPortReference,
                        Operator = Operator.Equality,
                        Right = Value.OneCharacter
                    },
                    Operator = Operator.ConditionalAnd,
                    Right = new Binary
                    {
                        Left = finishedSignalReference,
                        Operator = Operator.Equality,
                        Right = Value.ZeroCharacter
                    }

                },
                True = new InlineBlock(
                    new LineComment("Starting the state machine corresponding to the given member ID."),
                    memberSelectingCase),
                Else = new InlineBlock(
                    new LineComment("Waiting for Started to be pulled back to zero that signals the framework noting the finish."),
                    new IfElse
                    {
                        Condition = new Binary
                        {
                            Left = new Binary
                            {
                                Left = startedPortReference,
                                Operator = Operator.Equality,
                                Right = Value.ZeroCharacter
                            },
                            Operator = Operator.ConditionalAnd,
                            Right = new Binary
                            {
                                Left = finishedSignalReference,
                                Operator = Operator.Equality,
                                Right = Value.OneCharacter
                            }

                        },
                        True = new Assignment
                        {
                            AssignTo = finishedSignalReference,
                            Expression = Value.ZeroCharacter
                        }
                    })
            };


            return proxyComponent;
        }
    }
}
