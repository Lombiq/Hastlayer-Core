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


        public static Invocation ToInteger(IVhdlElement value) => 
            new Invocation
            {
                Target = "to_integer".ToVhdlIdValue(),
                Parameters = new List<IVhdlElement> { { value } }
            };

        public static Invocation Resize(IVhdlElement value, int size) => InvokeSizingFunction("resize", value, size);

        public static Invocation ToSigned(IVhdlElement value, int size) => InvokeSizingFunction("to_signed", value, size);

        public static Invocation ToUnsigned(IVhdlElement value, int size) => InvokeSizingFunction("to_unsigned", value, size);


        private static Invocation InvokeSizingFunction(string functionName, IVhdlElement value, int size) =>
            new Invocation
            {
                Target = functionName.ToVhdlIdValue(),
                Parameters = new List<IVhdlElement> { { value }, { size.ToVhdlValue(KnownDataTypes.UnrangedInt) } }
            };
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
