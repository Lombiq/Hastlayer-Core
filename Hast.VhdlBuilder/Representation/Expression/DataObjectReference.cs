using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class DataObjectReference : DataObjectBase
    {
        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Name;
        }
    }
}
