using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Unary : IVhdlElement
    {
        public IVhdlElement Left { get; set; }
        public UnaryOperator Operator { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                Left.ToVhdl(vhdlGenerationOptions) +
                " " + Operator.ToVhdl(vhdlGenerationOptions);
        }
    }
}
