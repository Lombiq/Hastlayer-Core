using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

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
            var builder = new StringBuilder();

            builder
                .Append(DataObjectKind)
                .Append(" ")
                .Append(Name.ToExtendedVhdlId())
                .Append(": ");

            if (DataType != null) builder.Append(DataType.ToVhdl());

            builder.Append(";");

            return builder.ToString();
        }
    }
}
