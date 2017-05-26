using System;
using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ArrayType : DataType // Not named "Array" to avoid naming clash with System.Array.
    {
        public DataType RangeType { get; set; } = KnownDataTypes.UnrangedInt;
        public int MaxLength { get; set; }
        public DataType ElementType { get; set; }

        private Value _defaultValue;
        public override Value DefaultValue
        {
            get
            {
                if (_defaultValue == null && ElementType != null)
                {
                    _defaultValue = CreateDefaultInitialization(this, ElementType);
                }

                return _defaultValue;
            }
            set { _defaultValue = value; }
        }


        public ArrayType()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "type " +
                vhdlGenerationOptions.ShortenName(Name) +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : RangeType.ToReference().ToVhdl(vhdlGenerationOptions) + " range <>") +
                ") of " +
                ElementType.ToReference().ToVhdl(vhdlGenerationOptions), vhdlGenerationOptions);


        public static Value CreateDefaultInitialization(DataType arrayInstantiationType, DataType elementType) =>
            new Value
            {
                DataType = arrayInstantiationType,
                Content = "others => " + elementType.DefaultValue.ToVhdl()
            };
    }
}
