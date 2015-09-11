using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class IfElse : IVhdlElement
    {
        public IVhdlElement Condition { get; set; }
        public IVhdlElement True { get; set; }
        public IVhdlElement Else { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                "if (" +
                Condition.ToVhdl() +
                ") then " +
                True.ToVhdl() +
                (Else != null ? "else " + Else.ToVhdl() : string.Empty) +
                "end if;";
        }
    }
}
