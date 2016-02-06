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
        /// Variables local to the state machine.
        /// </summary>
        IList<Variable> LocalVariables { get; }

        /// <summary>
        /// Variables corresponding to the state machine that are in the global namespace.
        /// </summary>
        IList<Variable> GlobalVariables { get; }

        /// <summary>
        /// Global signals declared for this state machine.
        /// </summary>
        IList<Signal> Signals { get; }

        /// <summary>
        /// Track which other members are called from this state machine and in how many instances at a given time. I.e.
        /// if this FSM starts another FSM (which was originally e.g. a method call) then it will be visible here. If
        /// parallelization happens then the call instance count will be greater than 1 (i.e. the other member is called
        /// in more than one instance at a given time).
        /// </summary>
        IDictionary<string, int> OtherMemberMaxCallInstanceCounts { get; }


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
