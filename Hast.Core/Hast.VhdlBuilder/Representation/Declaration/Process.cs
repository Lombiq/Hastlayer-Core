﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Process : ISubProgram
    {
        public string Label { get; set; }
        public string Name { get { return Label; } set { Label = value; } }
        public List<IDataObject> SensitivityList { get; set; } = new List<IDataObject>();
        public List<IVhdlElement> Declarations { get; set; } = new List<IVhdlElement>();
        public List<IVhdlElement> Body { get; set; } = new List<IVhdlElement>();


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                (!string.IsNullOrEmpty(Label) ? vhdlGenerationOptions.ShortenName(Label) + ": " : string.Empty) +
                "process (" +
                    string.Join("; ", SensitivityList.Select(signal => vhdlGenerationOptions.ShortenName(signal.Name))) +
                ") " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Declarations.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "begin " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end process", vhdlGenerationOptions);
    }
}