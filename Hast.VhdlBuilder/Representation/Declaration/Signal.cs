using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Signal : TypedDataObject
    {
        public Signal()
        {
            DataObjectKind = DataObjectKind.Signal;
        }
    }
}
