using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class While : IBlockElement
    {
        public IVhdlElement Condition { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public While()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminated.Terminate(
                "while " + Condition.ToVhdl(vhdlGenerationContext) + " loop " + vhdlGenerationContext.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationContext.CreateContextForSubLevel()) +
                "end loop", vhdlGenerationContext);
        }
    }
}
