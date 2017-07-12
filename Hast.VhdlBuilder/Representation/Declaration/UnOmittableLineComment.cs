using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class UnOmittableLineComment : LineComment
    {
        public UnOmittableLineComment(string text) : base(text)
        {
            CantBeOmitted = true;
        }
    }
}
