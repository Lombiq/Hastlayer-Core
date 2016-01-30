using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.StateMachineGeneration;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// The result that member transformers return.
    /// </summary>
    /// <remarks>
    /// Declarations and Body wouldn't be needed, since they can be generated from the state machine. However by
    /// requiring transformers to build them the process can be parallelized better.
    /// </remarks>
    public interface IMemberTransformerResult
    {
        IEnumerable<IMemberStateMachineResult> StateMachines { get; }
    }
}
