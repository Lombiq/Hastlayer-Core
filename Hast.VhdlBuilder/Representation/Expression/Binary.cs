using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Binary : IVhdlElement
    {
        public IVhdlElement Left { get; set; }
        public string Operator { get; set; }
        public IVhdlElement Right { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                Left.ToVhdl() +
                " " + Operator + " " +
                Right.ToVhdl();
        }
    }
}
