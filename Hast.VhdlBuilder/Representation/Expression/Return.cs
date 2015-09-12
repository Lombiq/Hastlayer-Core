using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Return : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminated.Terminate(
                "return" +
                (Expression != null ? Expression.ToVhdl(vhdlGenerationContext) : string.Empty),
                vhdlGenerationContext);
        }
    }
}
