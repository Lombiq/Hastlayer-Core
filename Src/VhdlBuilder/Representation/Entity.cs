using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class Entity : IVhdlElement
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }


        public Entity()
        {
            Ports = new List<Port>();
        }


        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            var portsVhdl = Ports.ToVhdl();

            builder
                .Append("entity ")
                .Append(Name)
                .Append(" is ")
                .Append("port(")
                .Append(portsVhdl.Substring(0, portsVhdl.Length - 1)) // Cutting off trailing semicolon
                .Append(");")
                .Append("end ")
                .Append(Name)
                .Append(";");

            return builder.ToString();
        }
    }
}
