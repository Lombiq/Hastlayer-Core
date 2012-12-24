using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation.Declaration;

namespace VhdlBuilder.Representation.Expression
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
                (ElseElements != null ? "else " + ElseElements.ToVhdl() : string.Empty) +
                "end if;";
        }
    }
}
