using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class IfElse<T> : IVhdlElement where T : IVhdlElement
    {
        public IVhdlElement Condition { get; set; }
        public T True { get; set; }
        public T Else { get; set; }


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

    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class IfElse : IfElse<IVhdlElement>
    {
    }
}
