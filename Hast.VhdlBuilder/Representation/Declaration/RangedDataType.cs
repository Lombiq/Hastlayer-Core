using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class RangedDataType : DataType
    {
        public int RangeMin { get; set; }
        public int RangeMax { get; set; }


        public override string ToReferenceVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return ToVhdl(vhdlGenerationOptions);
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Name + " range " + RangeMin + " to " + RangeMax;
        }
    }
}
