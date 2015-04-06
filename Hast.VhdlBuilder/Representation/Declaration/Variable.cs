using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Variable : TypedDataObject
    {
        public Variable()
        {
            DataObjectKind = DataObjectKind.Variable;
        }
    }
}
