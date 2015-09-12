using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

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


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                Label + " : " + vhdlGenerationOptions.ShortenName(Component.Name) + vhdlGenerationOptions.NewLineIfShouldFormat() +

                    "port map (" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                        PortMappings
                            .ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions))
                            .IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    Terminated.Terminate(")", vhdlGenerationOptions) +

                ")", vhdlGenerationOptions);
        }
    }


    public class PortMapping : IVhdlElement
    {
        public string From { get; set; }
        public string To { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.ShortenName(From) + " => " + vhdlGenerationOptions.ShortenName(To);
        }
    }
}
