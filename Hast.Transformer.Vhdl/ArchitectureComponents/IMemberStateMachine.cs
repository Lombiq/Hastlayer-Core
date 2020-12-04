using Hast.VhdlBuilder.Representation.Declaration;
using System.Collections.Generic;

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
        /// Makes note of a new multi-cycle operations. See <see cref="IArchitectureComponent.MultiCycleOperations"/>.
        /// </summary>
        /// <param name="operationResultReference">Reference to the result data object of the operation.</param>
        /// <param name="requiredClockCyclesCeiling">The clock cycles needed to complete the operation, rounded up.</param>
        void RecordMultiCycleOperation(IDataObject operationResultReference, int requiredClockCyclesCeiling);
    }
}
