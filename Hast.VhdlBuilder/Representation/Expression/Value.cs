using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class Value : IVhdlElement
    {
        public static readonly Value True = "true".ToVhdlIdValue();
        public static readonly Value False = "false".ToVhdlIdValue();
            

        public DataType DataType { get; set; }
        public string Content { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
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
