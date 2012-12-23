using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
    public static class DataTypes
    {
        public static DataType Bit = new DataType { TypeCategory = DataTypeCategory.Character, Name = "bit" };
        public static DataType Boolean = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "boolean" };
        public static DataType Character = new DataType { TypeCategory = DataTypeCategory.Character, Name = "character" };
        public static Enum Enum = new Enum();
        public static Identifier Identifier = new Identifier();
        public static RangedDataType Int16 = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer", RangeMin = -32768, RangeMax = 32767 };
        public static RangedDataType Int32 = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer", RangeMin = -2147483647, RangeMax = 2147483647 };
        public static RangedDataType Int = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer" };
        public static DataType Natural = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "natural" };
        public static String String = new String { Length = 256 };
        public static DataType Real = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "real" };
    }
}
