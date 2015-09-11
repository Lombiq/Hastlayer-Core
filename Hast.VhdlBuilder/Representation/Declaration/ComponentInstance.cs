using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            var vhdl =
                Label + " : " + Component.Name;


            vhdl += subContext.IndentIfShouldFormat() + "port map (" + subContext.NewLineIfShouldFormat();

            var portMapContext = subContext.CreateContextForSubLevel();
            foreach (var portMapping in PortMappings)
            {
                vhdl += portMapContext.IndentIfShouldFormat() + Terminated.Terminate(portMapping.ToVhdl(portMapContext), portMapContext);
            }

            vhdl += ")";

            return Terminated.Terminate(vhdl, vhdlGenerationContext);
        }
    }


    public class PortMapping : IVhdlElement
    {
        public string From { get; set; }
        public string To { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return From + " => " + To;
        }
    }
}
