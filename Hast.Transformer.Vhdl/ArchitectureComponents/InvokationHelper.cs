using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    internal static class InvokationHelper
    {
        public static IVhdlElement CreateInvokationStart(
            IArchitectureComponent component,
            string targetStateMachineName,
            int targetIndex)
        {
            var startedSignalReference = CreateStartedSignalReference(component, targetStateMachineName, targetIndex);

            component.Signals.AddIfNew(new Signal
            {
                DataType = KnownDataTypes.Boolean,
                Name = startedSignalReference.Name,
                InitialValue = Value.False
            });

            // Set the start signal for the state machine.
            return new Assignment
            {
                AssignTo = startedSignalReference,
                Expression = Value.True
            };
        }

        public static IfElse<IBlockElement> CreateWaitForInvokationFinished(
            IArchitectureComponent component,
            string targetStateMachineName,
            int degreeOfParallelism)
        {
            // Iteratively building a binary expression chain to OR together all Finished signals.
            IVhdlElement allInvokedStateMachinesFinishedExpression;

            allInvokedStateMachinesFinishedExpression = CreateFinishedSignalReference(component, targetStateMachineName, 0);

            if (degreeOfParallelism > 1)
            {
                var currentBinary = new Binary
                {
                    Left = CreateFinishedSignalReference(component, targetStateMachineName, 1),
                    Operator = Operator.ConditionalOr
                };
                var firstBinary = currentBinary;

                for (int i = 2; i < degreeOfParallelism; i++)
                {
                    var newBinary = new Binary
                    {
                        Left = CreateFinishedSignalReference(component, targetStateMachineName, i),
                        Operator = Operator.ConditionalOr
                    };

                    currentBinary.Right = newBinary;
                    currentBinary = newBinary;
                }

                currentBinary.Right = allInvokedStateMachinesFinishedExpression;
                allInvokedStateMachinesFinishedExpression = firstBinary;
            }


            var allInvokedStateMachinesFinishedIfElseTrue = new InlineBlock();

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                component.Signals.AddIfNew(new Signal
                {
                    DataType = KnownDataTypes.Boolean,
                    Name = CreateFinishedSignalReference(component, targetStateMachineName, i).Name,
                    InitialValue = Value.False
                });

                // Reset the start signal in the finished block.
                allInvokedStateMachinesFinishedIfElseTrue.Add(new Assignment
                {
                    AssignTo = CreateStartedSignalReference(component, targetStateMachineName, i),
                    Expression = Value.False
                });
            }


            return new IfElse<IBlockElement>
            {
                Condition = allInvokedStateMachinesFinishedExpression,
                True = allInvokedStateMachinesFinishedIfElseTrue
            };
        }

        public static DataObjectReference CreateStartedSignalReference(
            IArchitectureComponent component,
            string targetStateMachineName,
            int index)
        {
            return component
                    .CreatePrefixedSegmentedObjectName(targetStateMachineName, NameSuffixes.Started, index.ToString())
                    .ToVhdlSignalReference();
        }

        public static DataObjectReference CreateFinishedSignalReference(
            IArchitectureComponent component,
            string targetStateMachineName,
            int index)
        {
            return component
                    .CreatePrefixedSegmentedObjectName(targetStateMachineName, NameSuffixes.Finished, index.ToString())
                    .ToVhdlSignalReference();
        }
    }
}
