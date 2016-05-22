using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Constants;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Expression;
using Orchard;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public class SimpleMemoryOperationProxyBuilder : ISimpleMemoryOperationProxyBuilder
    {
        public IArchitectureComponent BuildProxy(IEnumerable<IArchitectureComponent> components)
        {
            var simpleMemoryUsingComponents = components.Where(c => c.AreSimpleMemorySignalsAdded());

            var proxyComponentName = "System.Void Hast::SimpleMemoryOperationProxy()";

            if (!simpleMemoryUsingComponents.Any()) return new BasicComponent(proxyComponentName);

            var signalsAssignmentBlock = new InlineBlock(new LineComment(proxyComponentName + " start"));


            signalsAssignmentBlock.Add(BuildConditionalPortAssignment(
                SimpleMemoryPortNames.CellIndex,
                simpleMemoryUsingComponents,
                component => new Binary
                {
                    Left = component.CreateSimpleMemoryReadEnableSignalReference(),
                    Operator = BinaryOperator.ConditionalOr,
                    Right = component.CreateSimpleMemoryWriteEnableSignalReference()
                }));

            signalsAssignmentBlock.Add(BuildConditionalPortAssignment(
                SimpleMemoryPortNames.DataOut,
                simpleMemoryUsingComponents,
                component => component.CreateSimpleMemoryWriteEnableSignalReference()));

            signalsAssignmentBlock.Add(BuildConditionalOrPortAssignment(
                SimpleMemoryPortNames.ReadEnable,
                simpleMemoryUsingComponents));

            signalsAssignmentBlock.Add(BuildConditionalOrPortAssignment(
                SimpleMemoryPortNames.WriteEnable,
                simpleMemoryUsingComponents));


            signalsAssignmentBlock.Add(new LineComment(proxyComponentName + " start"));


            // So it's not cut off wrongly if names are shortened we need to use a name for this signal as it would look 
            // from a generated state machine.
            return new BasicComponent(proxyComponentName)
            {
                Body = signalsAssignmentBlock
            };
        }


        private static ConditionalSignalAssignment BuildConditionalPortAssignment(
            string portName,
            IEnumerable<IArchitectureComponent> components,
            Func<IArchitectureComponent, IVhdlElement> expressionBuilderForComponentsAssignment)
        {
            var assignment = new ConditionalSignalAssignment
            {
                AssignTo = portName.ToExtendedVhdlId().ToVhdlSignalReference()
            };


            foreach (var component in components)
            {
                IVhdlElement value = component.CreateSimpleMemorySignalName(portName).ToVhdlIdValue();

                // Since CellIndex is an integer but all ints are handled as unsigned types internally we need to do
                // a type conversion.
                if (portName == SimpleMemoryPortNames.CellIndex)
                {
                    value = new Invokation
                    {
                        Target = "to_integer".ToVhdlIdValue(),
                        Parameters = new List<IVhdlElement> { { value } }
                    };
                }

                assignment.Whens.Add(new SignalAssignmentWhen
                    {
                        Expression = expressionBuilderForComponentsAssignment(component),
                        Value = value
                });
            }


            return assignment;
        }

        private static Assignment BuildConditionalOrPortAssignment(
            string portName,
            IEnumerable<IArchitectureComponent> components)
        {
            IVhdlElement assignmentExpression = components.First().CreateSimpleMemorySignalReference(portName);

            // Iteratively build a binary expression chain to OR together all the driving signals.
            if (components.Count() > 1)
            {
                var currentBinary = new Binary
                {
                    Left = components.Skip(1).First().CreateSimpleMemorySignalReference(portName),
                    Operator = BinaryOperator.ConditionalOr
                };
                var firstBinary = currentBinary;

                foreach (var drivingSignal in components.Skip(2).Select(c => c.CreateSimpleMemorySignalReference(portName)))
                {
                    var newBinary = new Binary
                    {
                        Left = drivingSignal,
                        Operator = BinaryOperator.ConditionalOr
                    };

                    currentBinary.Right = newBinary;
                    currentBinary = newBinary;
                }

                currentBinary.Right = assignmentExpression;
                assignmentExpression = firstBinary;
            }

            
            return new Assignment
            {
                AssignTo = portName.ToExtendedVhdlId().ToVhdlSignalReference(),
                Expression = assignmentExpression
            };
        }
    }
}
