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


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "component " + Name  + vhdlGenerationOptions.NewLineIfShouldFormat() +

                    "port(" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                        Ports
                            .ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions))
                            .IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    Terminated.Terminate(")", vhdlGenerationOptions) +

                "end " + Name, vhdlGenerationOptions);
        }
    }
}
