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

            component.InternallyDrivenSignals.AddIfNew(new Signal
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
                    Operator = BinaryOperator.ConditionalOr
                };
                var firstBinary = currentBinary;

                for (int i = 2; i < degreeOfParallelism; i++)
                {
                    var newBinary = new Binary
                    {
                        Left = CreateFinishedSignalReference(component, targetStateMachineName, i),
                        Operator = BinaryOperator.ConditionalOr
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
                component.InternallyDrivenSignals.AddIfNew(new Signal
                {
                    DataType = KnownDataTypes.Boolean,
                    Name = CreateFinishedSignalReference(component, targetStateMachineName, i).Name
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
            return CreateStartedSignalReference(component.Name, targetStateMachineName, index);
        }

        public static DataObjectReference CreateStartedSignalReference(
            string componentName,
            string targetStateMachineName,
            int index)
        {
            return CreateSignalReference(componentName, targetStateMachineName, NameSuffixes.Started, index);
        }


        public static DataObjectReference CreateFinishedSignalReference(
            IArchitectureComponent component,
            string targetStateMachineName,
            int index)
        {
            return CreateFinishedSignalReference(component.Name, targetStateMachineName, index);
        }


        public static DataObjectReference CreateFinishedSignalReference(
            string componentName,
            string targetStateMachineName,
            int index)
        {
            return CreateSignalReference(componentName, targetStateMachineName, NameSuffixes.Finished, index);
        }


        private static DataObjectReference CreateSignalReference(
            string componentName,
            string targetStateMachineName,
            string signalName,
            int index)
        {
            return ArchitectureComponentNameHelper
                .CreatePrefixedSegmentedObjectName(componentName, targetStateMachineName, signalName, index.ToString())
                .ToVhdlSignalReference();
        }
    }
}
