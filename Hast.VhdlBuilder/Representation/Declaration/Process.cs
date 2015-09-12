﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Process : ISubProgram
    {
        public string Label { get; set; }
        public string Name { get { return Label; } set { Label = value; } }
        public List<IDataObject> SensitivityList { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Process()
        {
            SensitivityList = new List<IDataObject>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
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
}
