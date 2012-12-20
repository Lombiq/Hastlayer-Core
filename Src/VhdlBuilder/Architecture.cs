using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    public class Architecture : IVhdlElement
    {
        public string Name { get; set; }
        public Entity Entity { get; set; }
        public IVhdlElement[] Declarations { get; set; }
        public IVhdlElement[] Body { get; set; }


        public Architecture()
        {
            Declarations = new IVhdlElement[] { };
            Body = new IVhdlElement[] { };
        }


        public string ToVhdl()
        {
            var builder = new StringBuilder(11);

            builder
                .Append("architecture ")
                .Append(Name)
                .Append(" of ")
                .Append(Entity.Name)
                .Append(" is ")
                .Append(Declarations.ToVhdl())
                .Append(" begin ")
                .Append(Body.ToVhdl())
                .Append(" end ")
                .Append(Name)
                .Append(";");

            return builder.ToString();
        }
    }
}
