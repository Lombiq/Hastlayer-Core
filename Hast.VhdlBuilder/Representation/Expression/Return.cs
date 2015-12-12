using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Return : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }


        public string ToVhdl()
        {
            return
                "return" +
                (Expression != null ? Expression.ToVhdl() : string.Empty) +
                ";";
        }
    }
}
