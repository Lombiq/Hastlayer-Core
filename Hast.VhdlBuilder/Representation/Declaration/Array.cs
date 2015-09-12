using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Array : DataType
    {
        public int MaxLength { get; set; }
        public DataType StoredType { get; set; }


        public Array()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                "type " +
                Name +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : "integer range <>") +
                ") of " +
                StoredType.Name;
        }
    }
}
