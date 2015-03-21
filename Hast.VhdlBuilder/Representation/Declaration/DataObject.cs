using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Declaration of a data object
    /// </summary>
    public class DataObject : DataObjectBase
    {
        /// <summary>
        /// If specified, this default value will be used to reset the object's value.
        /// </summary>
        public Value DefaultValue { get; set; }


        public override string ToVhdl()
        {
            var builder = new StringBuilder();

            builder
                .Append(ObjectType)
                .Append(" ")
                .Append(Name.ToVhdlId())
                .Append(": ");

            if (DataType != null) builder.Append(DataType.ToVhdl());

            builder.Append(";");

            return builder.ToString();
        }
    }
}
