using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Generic : IVhdlElement
    {
        public List<GenericItem> Items { get; } = new List<GenericItem>();

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "generic (" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    (Items != null ? Items.ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions)).IndentLinesIfShouldFormat(vhdlGenerationOptions) : string.Empty) +
                ")",
                vhdlGenerationOptions);
    }

    public class GenericItem : DataObjectBase
    {
        public Value Value { get; set; }

        public GenericItem() => DataObjectKind = DataObjectKind.Constant;

        public override IDataObject ToReference() => this;

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            vhdlGenerationOptions.ShortenName(Name) +
            ": " +
            Value.DataType.ToVhdl(vhdlGenerationOptions) +
            " := " +
            Value.ToVhdl(vhdlGenerationOptions);
    }
}
