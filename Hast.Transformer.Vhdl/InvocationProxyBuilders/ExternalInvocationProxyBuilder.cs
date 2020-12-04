using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public class ExternalInvocationProxyBuilder : IExternalInvocationProxyBuilder
    {
        public IArchitectureComponent BuildProxy(
            IEnumerable<IMemberTransformerResult> hardwareEntryPointMemberResults,
            MemberIdTable memberIdTable)
        {
            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would look 
            // from a generated state machine.
            var proxyComponent = new ConfigurableComponent("System.Void Hast::ExternalInvocationProxy()");

            // Since the Finished port is an out port, it can't be read. Adding an internal proxy signal so we can also 
            // read it.
            var finishedSignal = new Signal
            {
                Name = "FinishedInternal".ToExtendedVhdlId(),
                DataType = KnownDataTypes.Boolean,
                InitialValue = Value.False
            };
            proxyComponent.InternallyDrivenSignals.Add(finishedSignal);
            var finishedSignalReference = finishedSignal.ToReference();
            proxyComponent.BeginBodyWith = new Assignment
            {
                AssignTo = CommonPortNames.Finished.ToVhdlSignalReference(),
                Expression = finishedSignalReference
            };

            var memberSelectingCase = new Case { Expression = CommonPortNames.MemberId.ToVhdlIdValue() };

            foreach (var hardwareEntryPointMemberResult in hardwareEntryPointMemberResults)
            {
                var memberName = hardwareEntryPointMemberResult.Member.GetFullName();
                var memberId = memberIdTable.LookupMemberId(memberName);
                proxyComponent.OtherMemberMaxInvocationInstanceCounts[hardwareEntryPointMemberResult.Member] = 1;
                var when = new CaseWhen
                {
                    Expression = memberId.ToVhdlValue(KnownDataTypes.UnrangedInt)
                };

                var waitForInvocationFinishedIfElse = InvocationHelper
                    .CreateWaitForInvocationFinished(proxyComponent, memberName, 1);

                waitForInvocationFinishedIfElse.True.Add(new Assignment
                {
                    AssignTo = finishedSignalReference,
                    Expression = Value.True
                });

                when.Add(new IfElse
                {
                    Condition = new Binary
                    {
                        Left = InvocationHelper.CreateStartedSignalReference(proxyComponent, memberName, 0),
                        Operator = BinaryOperator.Equality,
                        Right = Value.False
                    },
                    True = InvocationHelper.CreateInvocationStart(proxyComponent, memberName, 0),
                    ElseIfs = new List<If<IVhdlElement>>
                        {
                            new If
                            {
                                Condition = waitForInvocationFinishedIfElse.Condition,
                                True = waitForInvocationFinishedIfElse.True
                            }
                        }
                });
                ;

                memberSelectingCase.Whens.Add(when);
            }

            memberSelectingCase.Whens.Add(CaseWhen.CreateOthers());

            var startedPortReference = CommonPortNames.Started.ToVhdlSignalReference();
            proxyComponent.ProcessNotInReset = new IfElse
            {
                Condition = new Binary
                {
                    Left = new Binary
                    {
                        Left = startedPortReference,
                        Operator = BinaryOperator.Equality,
                        Right = Value.True
                    },
                    Operator = BinaryOperator.And,
                    Right = new Binary
                    {
                        Left = finishedSignalReference,
                        Operator = BinaryOperator.Equality,
                        Right = Value.False
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
                                Operator = BinaryOperator.Equality,
                                Right = Value.False
                            },
                            Operator = BinaryOperator.And,
                            Right = new Binary
                            {
                                Left = finishedSignalReference,
                                Operator = BinaryOperator.Equality,
                                Right = Value.True
                            }

                        },
                        True = new Assignment
                        {
                            AssignTo = finishedSignalReference,
                            Expression = Value.False
                        }
                    })
            };

            return proxyComponent;
        }
    }
}
