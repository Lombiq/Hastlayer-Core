using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Component : INamedElement
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }


        public Component()
        {
            Ports = new List<Port>();
        }


        public string ToVhdl()
        {
            return
                "component " +
                Name.ToExtendedVhdlId() +
                " port(" +
                string.Join(", ", Ports.Select(parameter => parameter.ToVhdl())) +
                ");" +
                "end component;";
        }
    }
}
