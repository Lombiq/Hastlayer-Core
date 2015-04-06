﻿using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class Value : IVhdlElement
    {
        public DataType DataType { get; set; }
        public string Content { get; set; }


        public string ToVhdl()
        {
            if (DataType == null) return Content;

            if (DataType.TypeCategory == DataTypeCategory.Numeric || 
                DataType.TypeCategory == DataTypeCategory.Unit ||
                DataType.TypeCategory == DataTypeCategory.Identifier) return Content;

            if (DataType.TypeCategory == DataTypeCategory.Array) return "\"" + Content + "\"";

            if (DataType.TypeCategory == DataTypeCategory.Character) return "'" + Content + "'";

            return Content;
        }
    }
}
