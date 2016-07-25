using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class TypedDataObject : TypedDataObjectBase
    {
        /// <summary>
        /// If specified, this value will be set for the object when the hardware is initialized.
        /// </summary>
        public Value InitialValue { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                DataObjectKind.ToString() +
                " " +
                vhdlGenerationOptions.ShortenName(Name) +
                (DataType != null ? ": " + DataType.ToReference().ToVhdl(vhdlGenerationOptions) : string.Empty) +
                // The default value should be specified with ":=", even for signals.
                (InitialValue != null ? ( " := " + InitialValue.ToVhdl(vhdlGenerationOptions)) : string.Empty),
                vhdlGenerationOptions);
        }
    }
}
