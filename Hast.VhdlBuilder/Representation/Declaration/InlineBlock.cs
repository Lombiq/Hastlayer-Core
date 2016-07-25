using System.Collections.Generic;
using Hast.VhdlBuilder.Extensions;
using System.Linq;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// An element that has a body but is inline, i.e. not surrounded by anything
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class InlineBlock : IBlockElement
    {
        public List<IVhdlElement> Body { get; set; }


        public InlineBlock(params IVhdlElement[] vhdlElements)
        {
            Body = vhdlElements.ToList();
        }

        public InlineBlock(IEnumerable<IVhdlElement> vhdlElements)
        {
            Body = vhdlElements.ToList();
        }

        public InlineBlock()
        {
            Body = new List<IVhdlElement>();
        }


        public virtual string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Body.ToVhdl(vhdlGenerationOptions);
        }
    }
}
