using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Instatiation of an unconstrained VHDL array.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class UnconstrainedArrayInstantiation : ArrayTypeBase
    {
        public IDataObject ArrayReference { get; set; }
        public int RangeFrom { get; set; }
        public int RangeTo { get; set; }


        public override DataType ToReference() => this;

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            (ArrayReference == null ? vhdlGenerationOptions.NameShortener(Name) : ArrayReference.ToVhdl(vhdlGenerationOptions))
            + "(" + RangeFrom + " to " + RangeTo + ")";
    }
}
