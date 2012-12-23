using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation.Declaration
{
    public class ElementCollection : IVhdlElement
    {
        public List<IVhdlElement> Elements { get; set; }


        public ElementCollection()
        {
            Elements = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return Elements.ToVhdl();
        }
    }
}
