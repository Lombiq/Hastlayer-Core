using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class DataObjectReference : DataObjectBase
    {
        public override string ToVhdl()
        {
            // Shouldn't use extended identifier as the reference can be a normal one too.
            return Name;
        }
    }
}
