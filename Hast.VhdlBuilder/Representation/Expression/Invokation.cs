using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Invokation : IVhdlElement
    {
        public IVhdlElement Target { get; set; }
        public List<IVhdlElement> Parameters { get; set; }


        public Invokation()
        {
            Parameters = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return 
                Target.ToVhdl(vhdlGenerationContext) +
                (Parameters != null && Parameters.Any() ? "(" + string.Join(", ", Parameters.Select(parameter => parameter.ToVhdl(vhdlGenerationContext))) + ")" : string.Empty);
        }
    }


    [DebuggerDisplay("{ToVhdl()}")]
    public class NamedInvokationParameter : IVhdlElement
    {
        public INamedElement FormalParameter { get; set; }
        public INamedElement ActualParameter { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return FormalParameter.Name + " => " + ActualParameter.Name;
        }
    }
}
