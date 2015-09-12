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
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            return Terminated.Terminate(
                "if (" + Condition.ToVhdl(vhdlGenerationContext) + ") then " + vhdlGenerationContext.NewLineIfShouldFormat() +
                    True.ToVhdl(subContext) +
                    (Else != null ? "else " + Else.ToVhdl(subContext) : string.Empty) +
                "end if", vhdlGenerationContext);
        }
    }
}
