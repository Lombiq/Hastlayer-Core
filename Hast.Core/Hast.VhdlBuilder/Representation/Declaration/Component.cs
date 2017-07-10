using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Component : INamedElement
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; } = new List<Port>();


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var name = vhdlGenerationOptions.ShortenName(Name);
            return Terminated.Terminate(
                "component " + name + vhdlGenerationOptions.NewLineIfShouldFormat() +

                    "port(" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                        Ports
                            .ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions))
                            .IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    Terminated.Terminate(")", vhdlGenerationOptions) +

                "end " + name, vhdlGenerationOptions);
        }
    }
}
