﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Vhdl.Helpers;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    internal static class InvocationHelper
    {
        public static IVhdlElement CreateInvocationStart(
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

        public static IfElse<IBlockElement> CreateWaitForInvocationFinished(
            IArchitectureComponent component,
            string targetStateMachineName,
            int degreeOfParallelism,
            bool waitForAll = true)
        {
            // Iteratively building a binary expression chain to OR or AND together all (Started = Finished) expressions.
            // Using (Started = Finished) so it will work even if not all state available state machines were started.

            var allInvokedStateMachinesFinishedIfElseTrue = new InlineBlock();

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                component.ExternallyDrivenSignals.AddIfNew(new Signal
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

            Func<int, IVhdlElement> createStartedEqualsFinishedBinary = index =>
                new Binary
                {
                    Left = CreateStartedSignalReference(component, targetStateMachineName, index),
                    Operator = BinaryOperator.Equality,
                    Right = CreateFinishedSignalReference(component, targetStateMachineName, index)
                };

            return new IfElse<IBlockElement>
            {
                Condition = BinaryChainBuilder.BuildBinaryChain(
                    Enumerable.Range(0, degreeOfParallelism).Select(i => createStartedEqualsFinishedBinary(i)),
                    waitForAll ? BinaryOperator.ConditionalAnd : BinaryOperator.ConditionalOr),
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
