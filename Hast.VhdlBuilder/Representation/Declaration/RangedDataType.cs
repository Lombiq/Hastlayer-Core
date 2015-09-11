using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class RangedDataType : DataType
    {
        public int RangeMin { get; set; }
        public int RangeMax { get; set; }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            if (RangeMin == 0 || RangeMax == 0) return Name;
            return Name + " range " + RangeMin + " to " + RangeMax;
        }
    }
}
