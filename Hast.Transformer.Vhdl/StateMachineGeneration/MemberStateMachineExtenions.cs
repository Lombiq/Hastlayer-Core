using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public static class MemberStateMachineExtenions
    {
        public static IVhdlElement ChangeToStartState(this IMemberStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(0);
        }

        public static IVhdlElement ChangeToFinalState(this IMemberStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(1);
        }

        /// <summary>
        /// Implements a change from the current state to the state with the given index in VHDL.
        /// </summary>
        /// <param name="destinationStateIndex">The index of the state to change to.</param>
        /// <returns>The state change implemented in VHDL.</returns>
        public static IVhdlElement CreateStateChange(this IMemberStateMachine stateMachine, int destinationStateIndex)
        {
            return new Assignment
            {
                AssignTo = stateMachine.CreateStateVariableName().ToVhdlVariableReference(),
                Expression = stateMachine.CreateStateName(destinationStateIndex).ToVhdlIdValue()
            };
        }

        /// <summary>
        /// Generates the name for the state with the given index.
        /// </summary>
        /// <param name="index">The index of the state.</param>
        /// <returns>The name of the state.</returns>
        public static string CreateStateName(this IMemberStateMachine stateMachine, int index)
        {
            // This doesn't need a static helper method because we deliberately don't want to generate state names for
            // other state machines, since we don't want to directly set other state machines' states.
            return MemberStateMachineNameFactory.CreatePrefixedObjectName(stateMachine.Name, "_State_" + index);
        }

        public static string CreateStartedSignalName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateStartedSignalName(stateMachine.Name);
        }

        public static string CreateFinishedSignalName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateFinishedSignalName(stateMachine.Name);
        }

        public static string CreateReturnVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateReturnVariableName(stateMachine.Name);
        }

        public static string CreateStateVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreatePrefixedObjectName(stateMachine.Name, "_State");
        }

        public static string CreatePrefixedSegmentedObjectName(this IMemberStateMachine stateMachine, params string[] segments)
        {
            return stateMachine.CreatePrefixedObjectName(string.Join(".", segments));
        }

        /// <summary>
        /// Creates a VHDL object (i.e. signal or variable) name prefixes with the state machine's name.
        /// </summary>
        public static string CreatePrefixedObjectName(this IMemberStateMachine stateMachine, string name)
        {
            return MemberStateMachineNameFactory.CreatePrefixedObjectName(stateMachine.Name, name);
        }

        /// <summary>
        /// Determines the name of the next available name for a VHDL object (i.e. signal or variable) whose name is
        /// suffixed with a numerical index.
        /// </summary>
        /// <example>
        /// If we need a variable with the name "number" then this method will create a name like "StateMachineName.number.0",
        /// or if that exists, then the next available variation like "StateMachineName.number.5".
        /// </example>
        /// <returns>An object name prefixed with the state machine's name and suffixed with a numerical index.</returns>
        public static string GetNextUnusedIndexedObjectName(this IMemberStateMachine stateMachine, string name)
        {
            var objectName = name + ".0";
            var objectNameIndex = 0;

            while (
                stateMachine.LocalVariables.Any(variable => variable.Name == stateMachine.CreatePrefixedObjectName(objectName)) ||
                stateMachine.Signals.Any(signal => signal.Name == stateMachine.CreatePrefixedObjectName(objectName)))
            {
                objectName = name + "." + ++objectNameIndex;
            }

            return stateMachine.CreatePrefixedObjectName(objectName);
        }

        public static Variable CreateVariableWithNextUnusedIndexedName(this IMemberStateMachine stateMachine, string name, DataType dataType)
        {
            var returnVariable = new Variable
            {
                Name = stateMachine.GetNextUnusedIndexedObjectName(name),
                DataType = dataType
            };

            stateMachine.LocalVariables.Add(returnVariable);

            return returnVariable;
        }
    }
}
