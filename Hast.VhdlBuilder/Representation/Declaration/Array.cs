using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Array : DataType
    {
        public int MaxLength { get; set; }
        public DataType ElementType { get; set; }


        public Array()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                "type " +
                vhdlGenerationOptions.ShortenName(Name) +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : "integer range <>") +
                ") of " +
                ElementType.ToReferenceVhdl(vhdlGenerationOptions);
        }
    }
}
