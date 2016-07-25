using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class While : IBlockElement
    {
        public IVhdlElement Condition { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public While()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "while " + Condition.ToVhdl(vhdlGenerationOptions) + " loop " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end loop", vhdlGenerationOptions);
        }
    }
}
