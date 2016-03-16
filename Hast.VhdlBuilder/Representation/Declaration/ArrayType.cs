using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ArrayType : DataType // Not named "Array" to avoid naming clash with System.Array.
    {
        public DataType RangeType { get; set; }
        public int MaxLength { get; set; }
        public DataType ElementType { get; set; }


        public ArrayType()
        {
            RangeType = KnownDataTypes.UnrangedInt;
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "type " +
                vhdlGenerationOptions.ShortenName(Name) +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : RangeType.ToReference().ToVhdl(vhdlGenerationOptions) + " range <>") +
                ") of " +
                ElementType.ToReference().ToVhdl(vhdlGenerationOptions), vhdlGenerationOptions);
        }
    }
}
