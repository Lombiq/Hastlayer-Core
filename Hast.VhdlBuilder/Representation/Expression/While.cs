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


        public string ToVhdl()
        {
            return
                "while " +
                Condition.ToVhdl() +
                " loop " +
                Body.ToVhdl() +
                "end loop;";
        }
    }
}
