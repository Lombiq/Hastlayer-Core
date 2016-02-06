using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public interface IMemberStateMachineState
    {
        IBlockElement Body { get; }
        decimal RequiredClockCycles { get; set; }
    }


    /// <summary>
    /// A state machine generated from a .NET member.
    /// </summary>
    public interface IMemberStateMachine : IArchitectureComponent
    {
        /// <summary>
        /// States of the state machine. The state with the index 0 is the start state, the one with the index 1 is the
        /// final state.
        /// </summary>
        IReadOnlyList<IMemberStateMachineState> States { get; }

        /// <summary>
        /// Adds a new state to the state machine.
        /// </summary>
        /// <param name="state">The state's VHDL element.</param>
        /// <returns>The index of the state.</returns>
        int AddState(IBlockElement state);

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
