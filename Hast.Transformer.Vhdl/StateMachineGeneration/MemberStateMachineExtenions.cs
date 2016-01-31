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
            return (stateMachine.Name + "_State_" + index).ToExtendedVhdlId();
        }

        public static string CreateStartSignalName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateStartSignalName(stateMachine.Name);
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

        public static string CreatePrefixedObjectName(this IMemberStateMachine stateMachine, string name)
        {
            return MemberStateMachineNameFactory.CreatePrefixedObjectName(stateMachine.Name, name);
        }

        public static string CreateNamePrefixedExtendedVhdlId(this IMemberStateMachine stateMachine, string id)
        {
            return MemberStateMachineNameFactory.CreatePrefixedExtendedVhdlId(stateMachine.Name, id);
        }

        /// <summary>
        /// Making sure that the e.g. return variable names are unique per method call (to transfer procedure outputs).
        /// </summary>
        public static string GetNextUnusedTemporaryVariableName(this IMemberStateMachine stateMachine, string name)
        {
            var variableName = name + "0";
            var returnVariableNameIndex = 0;

            while (stateMachine.LocalVariables.Any(variable =>
                variable.Name == stateMachine.CreatePrefixedObjectName(variableName)))
            {
                variableName = name + ++returnVariableNameIndex;
            }

            return stateMachine.CreatePrefixedObjectName(variableName);
        }

        public static Variable CreateTemporaryVariable(this IMemberStateMachine stateMachine, string name, DataType dataType)
        {
            var returnVariable = new Variable
            {
                Name = stateMachine.GetNextUnusedTemporaryVariableName(name),
                DataType = dataType
            };

            stateMachine.LocalVariables.Add(returnVariable);

            return returnVariable;
        }
    }
}
