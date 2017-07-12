using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class IfElse<T> : If<T>, IVhdlElement where T : IVhdlElement
    {
        public List<If<T>> ElseIfs { get; set; } = new List<If<T>>();
        public T Else { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var vhdl =
                "if (" + Condition.ToVhdl(vhdlGenerationOptions) + ") then " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    True.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions);

            foreach (var elseIf in ElseIfs)
            {
                vhdl +=
                    "elsif (" + elseIf.Condition.ToVhdl(vhdlGenerationOptions) + ") then " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                        elseIf.True.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions);
            }

            if (Else != null)
            {
                vhdl += "else " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Else.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions);
            }

            vhdl += "end if";

            return Terminated.Terminate(vhdl, vhdlGenerationOptions);
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class IfElse : IfElse<IVhdlElement>
    {
    }
}
