using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// An element that has a body but is inline, i.e. not surrounded by anything.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class InlineBlock : IBlockElement
    {
        public List<IVhdlElement> Body { get; set; } = new List<IVhdlElement>();

        public InlineBlock(params IVhdlElement[] vhdlElements)
        {
            Body = vhdlElements.ToList();
        }

        public InlineBlock(IEnumerable<IVhdlElement> vhdlElements)
        {
            Body = vhdlElements.ToList();
        }

        public virtual string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) => Body.ToVhdl(vhdlGenerationOptions);
    }
}
