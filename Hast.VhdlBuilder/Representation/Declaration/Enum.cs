using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Enum : DataType
    {
        public List<Value> Values { get; set; }


        public Enum()
        {
            TypeCategory = DataTypeCategory.Composite;
            Values = new List<Value>();
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "type " + vhdlGenerationOptions.ShortenName(Name) + " is (" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Values.ToVhdl(vhdlGenerationOptions, ", " + Environment.NewLine, string.Empty).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                ")", vhdlGenerationOptions);
        }
    }
}
