
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public static class KnownDataTypes
    {
        public static DataType Bit = new DataType { TypeCategory = DataTypeCategory.Character, Name = "bit" };
        public static DataType Boolean = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "boolean" };
        public static DataType Character = new DataType { TypeCategory = DataTypeCategory.Character, Name = "character" };
        public static Identifier Identifier = new Identifier();
        public static RangedDataType Int16 = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer", RangeMin = -32768, RangeMax = 32767 };
        public static RangedDataType Int32 = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer", RangeMin = -2147483648, RangeMax = 2147483647 };
        public static DataType Int64 = new SizedDataType { Name = "signed", Size = 64 };
        public static DataType UnrangedInt = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer" };
        public static DataType UInt16 = new RangedDataType { TypeCategory = DataTypeCategory.Numeric, Name = "natural", RangeMin = 0, RangeMax = 65535 };
        public static DataType UInt32 = new SizedDataType { Name = "unsigned", Size = 32 };
        public static DataType UInt64 = new SizedDataType { Name = "unsigned", Size = 64 };
        public static DataType StdLogic = new DataType { TypeCategory = DataTypeCategory.Character, Name = "std_logic" };
        public static String String = new String { Length = 256 };
        public static DataType Real = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "real" };
        public static DataType Void = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "void" };
    }
}
