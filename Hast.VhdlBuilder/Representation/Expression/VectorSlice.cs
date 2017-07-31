using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class VectorSlice : IVhdlElement
    {
        public IVhdlElement Vector { get; set; }

        public int IndexFrom { get; set; }
        public int IndexTo { get; set; }

        public bool IsDownTo { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Vector.ToVhdl(vhdlGenerationOptions) +
            "(" + IndexFrom + " " + (IsDownTo ? "down" : string.Empty) + "to " + IndexTo + ")";
    }
}
