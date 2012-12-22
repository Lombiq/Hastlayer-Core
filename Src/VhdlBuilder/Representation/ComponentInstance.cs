using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class ComponentInstance : IVhdlElement
    {
        public Component Component { get; set; }
        public string Label { get; set; }
        public List<PortMapping> PortMappings { get; set; }


        public ComponentInstance()
        {
            PortMappings = new List<PortMapping>();
        }


        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            builder
                .Append(Label)
                .Append(" : ")
                .Append(Component.Name.ToVhdlId())
                .Append(" port map (")
                .Append(String.Join(", ", PortMappings.Select(mapping => mapping.ToVhdl())))
                .Append(");");

            return builder.ToString();
        }
    }

    public class PortMapping : IVhdlElement
    {
        public string From { get; set; }
        public string To { get; set; }

        public string ToVhdl()
        {
            return From.ToVhdlId() + " => " + To.ToVhdlId();
        }
    }
}
