using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a <see href="https://www.ics.uci.edu/~jmoorkan/vhdlref/blocks.html">VHDL block statement</see>.
    /// </summary>
    public interface IBlockElement : IVhdlElement
    {
        /// <summary>
        /// Gets the VHDL element contained inside the block statement.
        /// </summary>
        List<IVhdlElement> Body { get; }
    }

    public static class BlockElementExtensions
    {
        public static void Add(this IBlockElement blockElement, IVhdlElement element) => blockElement.Body.Add(element);
    }
}
