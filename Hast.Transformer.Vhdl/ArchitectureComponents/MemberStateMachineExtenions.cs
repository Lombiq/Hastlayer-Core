﻿using System;
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
            return ArchitectureComponentNameHelper.CreatePrefixedObjectName(stateMachine.Name, "_State_" + index);
        }

        public static string CreateStateVariableName(this IMemberStateMachine stateMachine)
        {
            return ArchitectureComponentNameHelper.CreatePrefixedObjectName(stateMachine.Name, "_State");
        }
    }
}