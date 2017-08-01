using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class UnOmittableBlockComment : BlockComment
    {
        public UnOmittableBlockComment(params string[] lines) : base(lines)
        {
            CantBeOmitted = true;
        }
    }
}
