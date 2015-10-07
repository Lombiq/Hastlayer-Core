using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Generic : IVhdlElement
    {
        public List<GenericItem> Items { get; set; }


        public Generic()
        {
            Items = new List<GenericItem>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "generic (" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    (Items != null ? Items.ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions)).IndentLinesIfShouldFormat(vhdlGenerationOptions) : string.Empty) +
                ")", vhdlGenerationOptions);
        }
    }


    public class GenericItem : DataObjectBase
    {
        public Value Value { get; set; }


        public GenericItem()
        {
            DataObjectKind = DataObjectKind.Constant;
        }


        override public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                vhdlGenerationOptions.ShortenName(Name) +
                ": " +
                Value.DataType.ToVhdl(vhdlGenerationOptions) +
                " := " +
                Value.ToVhdl(vhdlGenerationOptions);
        }
    }
}
