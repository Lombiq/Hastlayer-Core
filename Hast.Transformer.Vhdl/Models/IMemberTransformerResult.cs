using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;

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
        EntityDeclaration Member { get; }
        bool IsHardwareEntryPointMember { get; }
        IEnumerable<IArchitectureComponentResult> ArchitectureComponentResults { get; }
    }
}
