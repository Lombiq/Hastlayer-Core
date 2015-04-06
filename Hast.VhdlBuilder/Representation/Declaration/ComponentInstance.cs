using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
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
            return
                Label +
                " : " +
                Component.Name.ToExtendedVhdlId() +
                " port map (" +
                string.Join(", ", PortMappings.Select(mapping => mapping.ToVhdl())) +
                ");";
        }
    }


    public class PortMapping : IVhdlElement
    {
        public string From { get; set; }
        public string To { get; set; }


        public string ToVhdl()
        {
            return From.ToExtendedVhdlId() + " => " + To.ToExtendedVhdlId();
        }
    }
}
