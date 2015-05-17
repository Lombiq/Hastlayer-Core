using System.Diagnostics;
using System.Text;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class TypedDataObject : TypedDataObjectBase
    {
        /// <summary>
        /// If specified, this default value will be used to reset the object's value.
        /// </summary>
        public Value DefaultValue { get; set; }


        public override string ToVhdl()
        {
            return
                DataObjectKind.ToString() +
                " " +
                Name +
                (DataType != null ? ": " + DataType.ToVhdl() : string.Empty) +
                ";";
        }
    }
}
