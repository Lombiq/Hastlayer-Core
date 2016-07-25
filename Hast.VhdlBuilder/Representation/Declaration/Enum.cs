using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using System;

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
