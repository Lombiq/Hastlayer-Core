using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Variable : TypedDataObject
    {
        public bool Shared { get; set; }


        public Variable()
        {
            DataObjectKind = DataObjectKind.Variable;
        }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return (Shared ? "shared " : string.Empty) + base.ToVhdl(vhdlGenerationContext);
        }
    }
}
