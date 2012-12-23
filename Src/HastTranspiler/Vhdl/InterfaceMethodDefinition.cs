using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl
{
    public class InterfaceMethodDefinition
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }


        public InterfaceMethodDefinition()
        {
            Ports = new List<Port>();
        }
    }
}
