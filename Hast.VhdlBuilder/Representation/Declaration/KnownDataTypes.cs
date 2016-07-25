using System;
using Hast.VhdlBuilder.Representation.Expression;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public static class KnownDataTypes
    {
        private static DataType _bit = new DataType { TypeCategory = DataTypeCategory.Character, Name = "bit" };
        public static DataType Bit = new DataType(_bit)
        {
            DefaultValue = "0".ToVhdlValue(_bit)
        };

        private static DataType _boolean = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "boolean" };
        public static DataType Boolean = new DataType(_boolean)
        {
            DefaultValue = "false".ToVhdlValue(_boolean)
        };

        private static DataType _character = new DataType { TypeCategory = DataTypeCategory.Character, Name = "character" };
        public static DataType Character = new DataType(_character)
        {
            DefaultValue = default(char).ToString().ToVhdlValue(_character)
        };

        public static Identifier Identifier = new Identifier();

        public static DataType _unrangedInt = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "integer" };
        public static DataType UnrangedInt = new DataType(_unrangedInt)
        {
            DefaultValue = default(int).ToVhdlValue(_unrangedInt)
        };


        private static SizedDataType _int16 = new SizedDataType
        {
            TypeCategory = DataTypeCategory.Numeric,
            Name = "signed",
            Size = 16
        };
        public static SizedDataType Int16 = new SizedDataType(_int16)
        {
            DefaultValue = default(short).ToString().ToVhdlValue(_int16)
        };

        private static SizedDataType _int32 = new SizedDataType(_int16) { Size = 32 };
        public static SizedDataType Int32 = new SizedDataType(_int32)
        {
            DefaultValue = default(int).ToVhdlValue(_int32)
        };

        private static SizedDataType _int64 = new SizedDataType(_int16) { Size = 64 };
        public static SizedDataType Int64 = new SizedDataType(_int64)
        {
            DefaultValue = default(Int64).ToString().ToVhdlValue(_int64)
        };

        public static SizedDataType[] SignedIntegers = new[] { Int16, Int32, Int64 };


        private static SizedDataType _uint16 = new SizedDataType(_int16) { Name = "unsigned" };
        public static SizedDataType UInt16 = new SizedDataType(_uint16)
        {
            DefaultValue = default(ushort).ToString().ToVhdlValue(_uint16)
        };

        private static SizedDataType _uint32 = new SizedDataType(_uint16) { Size = 32 };
        public static SizedDataType UInt32 = new SizedDataType(_uint32)
        {
            DefaultValue = default(uint).ToString().ToVhdlValue(_uint32)
        };

        private static SizedDataType _uint64 = new SizedDataType(_uint16) { Size = 64 };
        public static SizedDataType UInt64 = new SizedDataType(_uint64)
        {
            DefaultValue = default(UInt64).ToString().ToVhdlValue(_uint64)
        };

        public static SizedDataType[] UnsignedIntegers = new[] { UInt16, UInt32, UInt64 };


        public static SizedDataType[] Integers = SignedIntegers.Union(UnsignedIntegers).ToArray();


        private static DataType _stdLogic = new DataType { TypeCategory = DataTypeCategory.Character, Name = "std_logic" };
        public static DataType StdLogic = new DataType(_stdLogic)
        {
            DefaultValue = "0".ToVhdlValue(_stdLogic)
        };

        private static StdLogicVector _stdLogicVector32 = new StdLogicVector { Size = 32 };
        public static StdLogicVector StdLogicVector32 = new StdLogicVector(_stdLogicVector32)
        {
            DefaultValue = "00000000000000000000000000000000".ToVhdlValue(_stdLogicVector32)
        };

        private static DataType _string = new String { Length = 256 };
        public static DataType String = new DataType(_string)
        {
            DefaultValue = default(string).ToVhdlValue(_string)
        };

        private static DataType _real = new DataType { TypeCategory = DataTypeCategory.Numeric, Name = "real" };
        public static DataType Real = new DataType(_real)
        {
            DefaultValue = default(double).ToString().ToVhdlValue(_real)
        };

        public static DataType Void = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "void" };
    }
}
