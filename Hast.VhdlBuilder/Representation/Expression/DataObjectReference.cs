using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class DataObjectReference : DataObjectBase
    {
        public override string ToVhdl()
        {
            // Shouldn't use extended identifier as the reference can be a normal one too.
            return Name;
        }
    }
}
