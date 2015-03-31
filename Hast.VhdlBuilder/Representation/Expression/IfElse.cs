using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class IfElse : IVhdlElement
    {
        public string Condition { get; set; }
        public List<IVhdlElement> TrueElements { get; set; }
        public List<IVhdlElement> ElseElements { get; set; }


        public IfElse()
        {
            TrueElements = new List<IVhdlElement>();
            ElseElements = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "if (" +
                Condition +
                ") then " +
                TrueElements.ToVhdl() +
                (ElseElements != null && ElseElements.Any() ? "else " + ElseElements.ToVhdl() : string.Empty) +
                "end if;";
        }
    }
}
