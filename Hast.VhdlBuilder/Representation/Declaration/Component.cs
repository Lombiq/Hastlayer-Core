using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            var vhdl =
                "component " + Name  + vhdlGenerationContext.NewLineIfShouldFormat() +
                    subContext.IndentIfShouldFormat() + "port(" + vhdlGenerationContext.NewLineIfShouldFormat();

            var portContext = subContext.CreateContextForSubLevel();
            foreach (var port in Ports)
            {
                vhdl += portContext.IndentIfShouldFormat() + Terminated.Terminate(port.ToVhdl(portContext), portContext);
            }

            vhdl +=
                    Terminated.Terminate(subContext.IndentIfShouldFormat() + ")", vhdlGenerationContext) +
                "end " + Name;

            return Terminated.Terminate(vhdl, vhdlGenerationContext);
        }
    }
}
