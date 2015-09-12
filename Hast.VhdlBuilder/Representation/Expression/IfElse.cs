using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class IfElse : IVhdlElement
    {
        public IVhdlElement Condition { get; set; }
        public IVhdlElement True { get; set; }
        public IVhdlElement Else { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "if (" + Condition.ToVhdl(vhdlGenerationOptions) + ") then " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    True.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    (Else != null ? 
                    "else " + vhdlGenerationOptions.NewLineIfShouldFormat() + Else.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) : 
                    string.Empty) +
                "end if", vhdlGenerationOptions);
        }
    }
}
