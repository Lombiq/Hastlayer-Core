﻿using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ComponentInstance : IVhdlElement
    {
        public Component Component { get; set; }
        public string Label { get; set; }
        public List<PortMapping> PortMappings { get; set; } = new List<PortMapping>();


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


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            vhdlGenerationOptions.ShortenName(From) + " => " + vhdlGenerationOptions.ShortenName(To);
    }
}