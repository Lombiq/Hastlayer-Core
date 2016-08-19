using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Instatiation of an unconstrained VHDL array.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class UnconstrainedArrayInstantiation : DataType
    {
        public int RangeFrom { get; set; }
        public int RangeTo { get; set; }


        public UnconstrainedArrayInstantiation()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override DataType ToReference()
        {
            return this;
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.NameShortener(Name) + "(" + RangeFrom + " to " + RangeTo + ")";
        }
    }
}
