using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Invocation : IVhdlElement
    {
        public IVhdlElement Target { get; set; }
        public List<IVhdlElement> Parameters { get; set; } = new List<IVhdlElement>();

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Target.ToVhdl(vhdlGenerationOptions) +
            (Parameters != null && Parameters.Any() ? "(" + Parameters.ToVhdl(vhdlGenerationOptions, ", ", string.Empty) + ")" : string.Empty);


        public static Invocation ToInteger(IVhdlElement value)
        {
            return new Invocation
            {
                Target = "to_integer".ToVhdlIdValue(),
                Parameters = new List<IVhdlElement> { { value } }
            };
        }

        public static Invocation Resize(IVhdlElement value, int size)
        {
            return new Invocation
            {
                Target = "resize".ToVhdlIdValue(),
                Parameters = new List<IVhdlElement> { { value }, { size.ToVhdlValue(KnownDataTypes.UnrangedInt) } }
            };
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class NamedInvocationParameter : IVhdlElement
    {
        public INamedElement FormalParameter { get; set; }
        public INamedElement ActualParameter { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            vhdlGenerationOptions.ShortenName(FormalParameter.Name) + " => " + vhdlGenerationOptions.ShortenName(ActualParameter.Name);
    }
}
