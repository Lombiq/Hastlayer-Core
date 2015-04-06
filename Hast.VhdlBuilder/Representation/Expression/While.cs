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
