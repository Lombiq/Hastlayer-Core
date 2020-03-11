using Hast.VhdlBuilder.Extensions;
using System.Linq;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public static class KnownDataTypes
    {
        // There is a private field and a public one for each type because the DefaultValue construction needs the type
        // itself too.

        private static readonly DataType _bit = new DataType { TypeCategory = DataTypeCategory.Character, Name = "bit" };
        public static readonly DataType Bit = new DataType(_bit)
        {
            DefaultValue = "0".ToVhdlValue(_bit)
        };

        private static readonly DataType _boolean = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "boolean" };
        public static readonly DataType Boolean = new DataType(_boolean)
        {
            DefaultValue = "false".ToVhdlValue(_boolean)
        };

        private static readonly SizedDataType _binaryString = new SizedDataType
        {
            TypeCategory = DataTypeCategory.Scalar,
            Name = "signed",
            Size = 16
        };
        public static readonly SizedDataType BinaryString = new SizedDataType(_binaryString)
        {
            DefaultValue = default(short).ToString().ToVhdlValue(_int16)
        };

        private static readonly DataType _character = new DataType { TypeCategory = DataTypeCategory.Character, Name = "character" };
        public static readonly DataType Character = new DataType(_character)
        {
            DefaultValue = default(char).ToString().ToVhdlValue(_character)
        };

        public static readonly Identifier Identifier = new Identifier();

        public static readonly DataType _unrangedInt = new DataType { TypeCategory = DataTypeCategory.Scalar, Name = "integer" };
        public static readonly DataType UnrangedInt = new DataType(_unrangedInt)
        {
            DefaultValue = default(int).ToVhdlValue(_unrangedInt)
        };


        private static readonly SizedDataType _int8 = new SizedDataType
        {
            TypeCategory = DataTypeCategory.Scalar,
            Name = "signed",
            Size = 8
        };
        public static readonly SizedDataType Int8 = new SizedDataType(_int8)
        {
            DefaultValue = default(sbyte).ToString().ToVhdlValue(_int8)
        };

        private static readonly SizedDataType _int16 = new SizedDataType(_int8) { Size = 16 };
        public static readonly SizedDataType Int16 = new SizedDataType(_int16)
        {
            DefaultValue = default(short).ToString().ToVhdlValue(_int16)
        };

        private static readonly SizedDataType _int32 = new SizedDataType(_int16) { Size = 32 };
        public static readonly SizedDataType Int32 = new SizedDataType(_int32)
        {
            DefaultValue = default(int).ToVhdlValue(_int32)
        };

        private static readonly SizedDataType _int64 = new SizedDataType(_int16) { Size = 64 };
        public static readonly SizedDataType Int64 = new SizedDataType(_int64)
        {
            DefaultValue = default(long).ToString().ToVhdlValue(_int64)
        };

        public static readonly SizedDataType[] SignedIntegers = new[] { Int8, Int16, Int32, Int64 };


        private static readonly SizedDataType _uint8 = new SizedDataType(_int16) { Name = "unsigned", Size = 8 };
        public static readonly SizedDataType UInt8 = new SizedDataType(_uint8)
        {
            DefaultValue = default(byte).ToString().ToVhdlValue(_uint8)
        };

        private static readonly SizedDataType _uint16 = new SizedDataType(_uint8) { Size = 16 };
        public static readonly SizedDataType UInt16 = new SizedDataType(_uint16)
        {
            DefaultValue = default(ushort).ToString().ToVhdlValue(_uint16)
        };

        private static readonly SizedDataType _uint32 = new SizedDataType(_uint16) { Size = 32 };
        public static readonly SizedDataType UInt32 = new SizedDataType(_uint32)
        {
            DefaultValue = default(uint).ToString().ToVhdlValue(_uint32)
        };

        private static readonly SizedDataType _uint64 = new SizedDataType(_uint16) { Size = 64 };
        public static readonly SizedDataType UInt64 = new SizedDataType(_uint64)
        {
            DefaultValue = default(ulong).ToString().ToVhdlValue(_uint64)
        };

        public static readonly SizedDataType[] UnsignedIntegers = new[] { UInt8, UInt16, UInt32, UInt64 };


        public static readonly SizedDataType[] Integers = SignedIntegers.Union(UnsignedIntegers).ToArray();


        private static readonly DataType _stdLogic = new DataType { TypeCategory = DataTypeCategory.Character, Name = "std_logic" };
        public static readonly DataType StdLogic = new DataType(_stdLogic)
        {
            DefaultValue = "0".ToVhdlValue(_stdLogic)
        };

        private static readonly StdLogicVector _stdLogicVector32 = new StdLogicVector { Size = 32 };
        public static readonly StdLogicVector StdLogicVector32 = new StdLogicVector(_stdLogicVector32);

        private static readonly DataType _unrangedString = new String();
        public static readonly DataType UnrangedString = new DataType(_unrangedString)
        {
            DefaultValue = default(string).ToVhdlValue(_unrangedString)
        };

        private static readonly DataType _real = new DataType { TypeCategory = DataTypeCategory.Scalar, Name = "real" };
        public static readonly DataType Real = new DataType(_real)
        {
            DefaultValue = default(double).ToString().ToVhdlValue(_real)
        };

        public static readonly DataType Void = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "void" };
    }
}
