using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class SizedDataType : DataType
    {
        public int Size { get; set; }


        public override string ToVhdl()
        {
            if (Size == 0) return Name;
            return Name + "(" + (Size - 1) + " downto 0)";
        }
    }
}
