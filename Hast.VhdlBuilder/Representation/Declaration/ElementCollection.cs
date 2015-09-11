using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class ElementCollection : IVhdlElement
    {
        public List<IVhdlElement> Elements { get; set; }


        public ElementCollection()
        {
            Elements = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Elements.ToVhdl(vhdlGenerationContext);
        }
    }
}
