using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Return : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }


        public string ToVhdl()
        {
            return
                "return" +
                (Expression != null ? Expression.ToVhdl() : string.Empty) +
                ";";
        }
    }
}
