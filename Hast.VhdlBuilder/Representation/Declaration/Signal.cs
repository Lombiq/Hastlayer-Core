using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Signal : TypedDataObject
    {
        public Signal()
        {
            DataObjectKind = DataObjectKind.Signal;
        }
    }
}
