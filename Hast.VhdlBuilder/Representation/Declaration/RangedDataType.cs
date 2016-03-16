using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class RangedDataType : DataType
    {
        public int RangeMin { get; set; }
        public int RangeMax { get; set; }


        public RangedDataType(DataType baseType) : base(baseType)
        {
        }

        public RangedDataType(RangedDataType previous) : base(previous)
        {
            RangeMin = previous.RangeMin;
            RangeMax = previous.RangeMax;
        }

        public RangedDataType()
        {
        }


        public override DataType ToReference()
        {
            return this;
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Name + " range " + RangeMin + " to " + RangeMax;
        }
    }
}
