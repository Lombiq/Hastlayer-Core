using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Architecture : IVhdlElement
    {
        public string Name { get; set; }
        public Entity Entity { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Architecture()
        {
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "architecture " +
                Name.ToVhdlId() +
                " of " +
                Entity.Name.ToVhdlId() +
                " is " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end " +
                Name.ToVhdlId() +
                ";";
        }
    }
}
