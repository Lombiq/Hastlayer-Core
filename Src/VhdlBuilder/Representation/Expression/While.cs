using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation.Declaration;

namespace VhdlBuilder.Representation.Expression
{
    public class While : IBlockElement
    {
        public string Condition { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public While()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "while " +
                Condition +
                " loop " +
                Body.ToVhdl() +
                "end loop;";
        }
    }
}
