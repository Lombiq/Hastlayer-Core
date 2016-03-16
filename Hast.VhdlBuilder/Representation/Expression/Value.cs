using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Value : IVhdlElement
    {
        public static readonly Value True = "true".ToVhdlIdValue();
        public static readonly Value False = "false".ToVhdlIdValue();
        public static readonly Value ZeroCharacter = new Character('0');
        public static readonly Value OneCharacter = new Character('1');

        
        public DataType DataType { get; set; }
        public string Content { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            if (DataType == null) return Content;

            if (DataType.TypeCategory == DataTypeCategory.Numeric || 
                DataType.TypeCategory == DataTypeCategory.Unit) return Content;

            if (DataType.TypeCategory == DataTypeCategory.Identifier) return vhdlGenerationOptions.ShortenName(Content);

            if (DataType.TypeCategory == DataTypeCategory.Array)
            {
                if (DataType.IsLiteralArrayType()) return "\"" + Content + "\"";
                else return "(" + Content + ")";
            }

            if (DataType.TypeCategory == DataTypeCategory.Character) return "'" + Content + "'";

            return Content;
        }
    }
}
