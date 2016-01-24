using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public interface IMemberStateMachineState
    {
        IBlockElement Body { get; }
        decimal RequiredClockCycles { get; set; }
    }


    /// <summary>
    /// A state machine generated from a .NET member.
    /// </summary>
    public interface IMemberStateMachine
    {
        /// <summary>
        /// Name of the state machine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// States of the state machine. The state with the index 0 is the start state, the one with the index 1 is the
        /// final state.
        /// </summary>
        IReadOnlyList<IMemberStateMachineState> States { get; }

        /// <summary>
        /// Input/output parameters of the state machine which can be used to communicate with other state machines.
        /// </summary>
        IList<Variable> Parameters { get; }

        /// <summary>
        /// Variables local to the state machine.
        /// </summary>
        IList<Variable> LocalVariables { get; }


        /// <summary>
        /// Adds a new state to the state machine.
        /// </summary>
        /// <param name="state">The state's VHDL element.</param>
        /// <returns>The index of the state.</returns>
        int AddState(IBlockElement state);

        /// <summary>
        /// Generates the name for the state with the given index.
        /// </summary>
        /// <param name="index">The index of the state.</param>
        /// <returns>The name of the state.</returns>
        string CreateStateName(int index);

        /// <summary>
        /// Implements a change from the current state to the state with the given index in VHDL.
        /// </summary>
        /// <param name="nextStateIndex">The index of the state to change to.</param>
        /// <returns>The state change implemented in VHDL.</returns>
        IVhdlElement CreateStateChange(int nextStateIndex);

        /// <summary>
        /// Produces the declarations corresponding to the state machine that should be inserted into the head of the
        /// architecture element.
        /// </summary>
        IVhdlElement BuildDeclarations();

        /// <summary>
        /// Produces the body of the state machine that should be inserted into the body of the architecture element.
        /// </summary>
        IVhdlElement BuildBody();
    }
}
