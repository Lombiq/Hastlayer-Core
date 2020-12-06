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
        public List<Value> Values { get; } = new List<Value>();

        public Enum() => TypeCategory = DataTypeCategory.Composite;

        public Enum(IEnumerable<Value> values)
            : this()
        {
            if (values != null) Values.AddRange(values);
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "type " + vhdlGenerationOptions.ShortenName(Name) + " is (" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Values.ToVhdl(vhdlGenerationOptions, ", " + Environment.NewLine, string.Empty).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                ")", vhdlGenerationOptions);
    }
}
