using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Component : IVhdlElement
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }


        public Component()
        {
            Ports = new List<Port>();
        }


        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            var portsVhdl = Ports.ToVhdl();

            builder
                .Append("component ")
                .Append(Name.ToVhdlId())
                .Append(" port(")
                .Append(portsVhdl.Substring(0, portsVhdl.Length - 1)) // Cutting off trailing semicolon
                .Append(");")
                .Append("end component;");

            return builder.ToString();
        }
    }
}
