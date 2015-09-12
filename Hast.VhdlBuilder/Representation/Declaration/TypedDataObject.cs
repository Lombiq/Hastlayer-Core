using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class TypedDataObject : TypedDataObjectBase
    {
        /// <summary>
        /// If specified, this default value will be used to reset the object's value.
        /// </summary>
        public Value DefaultValue { get; set; }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminated.Terminate(
                DataObjectKind.ToString() +
                " " +
                Name +
                (DataType != null ? ": " + DataType.ToReferenceVhdl() : string.Empty) +
                (DefaultValue != null ? ((DataObjectKind == DataObjectKind.Variable ? " := " : " <= ") + DefaultValue.ToVhdl(vhdlGenerationContext)) : string.Empty),
                vhdlGenerationContext);
        }
    }
}
