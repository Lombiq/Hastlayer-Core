using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public static class KnownDataTypes
    {
        // Without this static ctor the fields, even when actually used, wouldn't be initialized.
        // See: http://stackoverflow.com/questions/3580171/static-member-variable-not-being-initialized-in-release-compiler-clr-bug
        static KnownDataTypes()
        {
        }


        public static DataType Bit = new DataType
        {
            TypeCategory = DataTypeCategory.Character,
            Name = "bit",
            DefaultValue = Value.ZeroCharacter
        };

        public static DataType Boolean = new DataType
        {
            TypeCategory = DataTypeCategory.Identifier,
            Name = "boolean",
            DefaultValue = Value.False
        };

        private static DataType _character = new DataType { TypeCategory = DataTypeCategory.Character, Name = "character" };
        public static DataType Character = new DataType(_character)
        {
            DefaultValue = new Value { DataType = _character, Content = default(char).ToString() }
        };

        public static Identifier Identifier = new Identifier();

        public static DataType _unrangedInt = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer" };
        public static DataType UnrangedInt = new DataType(_unrangedInt)
        {
            DefaultValue = new Value {  DataType = _unrangedInt, Content = default(int).ToString() }
        };

        private static RangedDataType _int16 = new RangedDataType(_unrangedInt) {  RangeMin = -32768, RangeMax = 32767 };
        public static RangedDataType Int16 = new RangedDataType(_int16)
        {
            DefaultValue = new Value { DataType = _int16, Content = default(short).ToString() }
        };

        private static RangedDataType _int32 = new RangedDataType(_unrangedInt) { RangeMin = -2147483647, RangeMax = 2147483647 };
        public static RangedDataType Int32 = new RangedDataType(_int32)
        {
            DefaultValue = new Value { DataType = _int32, Content = default(int).ToString() }
        };

        private static DataType _natural = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "natural" };
        public static DataType Natural = new DataType(_natural)
        {
            DefaultValue = new Value { DataType = _natural, Content = default(uint).ToString() }
        };

        private static DataType _stdLogic = new DataType { TypeCategory = DataTypeCategory.Character, Name = "std_logic" };
        public static DataType StdLogic = new DataType(_stdLogic)
        {
            DefaultValue = new Value { DataType = _stdLogic, Content = "0" }
        };

        private static DataType _string = new String { Length = 256 };
        public static DataType String = new DataType(_string)
        {
            DefaultValue = new Value { DataType = _string, Content = default(string) }
        }; 
        
        private static DataType _real = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "real" };
        public static DataType Real = new DataType(_real)
        {
            DefaultValue = new Value { DataType = _real, Content = default(double).ToString() }
        };
        
        public static DataType Void = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "void" };
    }
}
