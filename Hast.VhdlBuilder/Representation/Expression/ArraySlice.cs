using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    /// <summary>
    /// A slice of an array data object, i.e. array(fromIndex to toIndex).
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ArraySlice : ArrayAccessBase
    {
        public int IndexFrom { get; set; }
        public int IndexTo { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            ArrayReference.ToVhdl(vhdlGenerationOptions) + "(" + IndexFrom + " to " + IndexTo + ")";
    }
}
