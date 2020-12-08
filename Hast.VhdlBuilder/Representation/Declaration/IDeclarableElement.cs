using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents <see href="https://www.ics.uci.edu/~jmoorkan/vhdlref/sig_dec.html">VHDL declarations</see>.
    /// </summary>
    public interface IDeclarableElement : IVhdlElement
    {
        /// <summary>
        /// Gets the list of declarations.
        /// </summary>
        List<IVhdlElement> Declarations { get; }
    }
}
