using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Generic : IVhdlElement
    {
        public List<GenericItem> Items { get; set; }


        public string ToVhdl()
        {
            return
                "generic (" +
                (Items != null ? string.Join(";", Items.Select(item => item.ToVhdl())) : string.Empty) +
                ");";
        }
    }


    public class GenericItem : DataObjectBase
    {
        public Value Value { get; set; }


        public GenericItem()
        {
            DataObjectKind = DataObjectKind.Constant;
        }


        override public string ToVhdl()
        {
            return
                Name.ToExtendedVhdlId() +
                ": " +
                Value.DataType.ToVhdl() +
                ":=" +
                Value.ToVhdl();
        }
    }
}
