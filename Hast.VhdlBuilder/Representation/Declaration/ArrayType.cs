using System;
using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ArrayType : ArrayTypeBase // Not named "Array" to avoid naming clash with System.Array.
    {
        public DataType RangeType { get; set; } = KnownDataTypes.UnrangedInt;
        public int MaxLength { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "type " +
                vhdlGenerationOptions.ShortenName(Name) +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : RangeType.ToReference().ToVhdl(vhdlGenerationOptions) + " range <>") +
                ") of " +
                ElementType.ToReference().ToVhdl(vhdlGenerationOptions), vhdlGenerationOptions);
    }
}
