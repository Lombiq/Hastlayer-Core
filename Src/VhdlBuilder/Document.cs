using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    public class Document : IVhdlElement
    {
        public Library[] Libraries { get; set; }
        public Entity Entity { get; set; }
        public Architecture Architecture { get; set; }


        public Document()
        {
            Libraries = new Library[] { };
        }


        public string ToVhdl()
        {
            return Libraries.ToVhdl() + Entity.ToVhdl() + Architecture.ToVhdl();
        }
    }
}
