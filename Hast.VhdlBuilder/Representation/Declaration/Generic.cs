using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Generic : IVhdlElement
    {
        public List<GenericItem> Items { get; set; }


        public Generic()
        {
            Items = new List<GenericItem>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminated.Terminate(
                "generic (" + vhdlGenerationContext.NewLineIfShouldFormat() +
                    (Items != null ? Items.ToVhdl(vhdlGenerationContext.CreateContextForSubLevel(), ";") : string.Empty) +
                ")", vhdlGenerationContext);
        }
    }


    public class GenericItem : DataObjectBase
    {
        public Value Value { get; set; }


        public GenericItem()
        {
            DataObjectKind = DataObjectKind.Constant;
        }


        override public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                Name +
                ": " +
                Value.DataType.ToVhdl(vhdlGenerationContext) +
                " := " +
                Value.ToVhdl(vhdlGenerationContext);
        }
    }
}
