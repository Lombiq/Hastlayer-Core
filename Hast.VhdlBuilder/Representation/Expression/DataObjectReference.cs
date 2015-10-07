using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class DataObjectReference : DataObjectBase
    {
        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.ShortenName(Name);
        }
    }
}
