using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Invocation : IVhdlElement
    {
        public IVhdlElement Target { get; set; }
        public List<IVhdlElement> Parameters { get; set; }


        public Invocation()
        {
            Parameters = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return 
                Target.ToVhdl(vhdlGenerationOptions) +
                (Parameters != null && Parameters.Any() ? "(" + Parameters.ToVhdl(vhdlGenerationOptions, ", ", string.Empty) + ")" : string.Empty);
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class NamedInvocationParameter : IVhdlElement
    {
        public INamedElement FormalParameter { get; set; }
        public INamedElement ActualParameter { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.ShortenName(FormalParameter.Name) + " => " + vhdlGenerationOptions.ShortenName(ActualParameter.Name);
        }
    }
}
