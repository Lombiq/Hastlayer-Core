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
            var builder = new StringBuilder(11);

            builder
                .Append("architecture ")
                .Append(Name.ToVhdlId())
                .Append(" of ")
                .Append(Entity.Name.ToVhdlId())
                .Append(" is ")
                .Append(Declarations.ToVhdl())
                .Append(" begin ")
                .Append(Body.ToVhdl())
                .Append(" end ")
                .Append(Name.ToVhdlId())
                .Append(";");

            return builder.ToString();
        }
    }
}
