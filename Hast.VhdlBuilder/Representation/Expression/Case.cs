using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Case : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }
        public List<When> Whens { get; set; }


        public Case()
        {
            Whens = new List<When>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            var builder = new StringBuilder();

            builder
                .Append("case ")
                .Append(Expression.ToVhdl(vhdlGenerationContext))
                .Append(" is ")
                .Append(vhdlGenerationContext.NewLineIfShouldFormat());

            var subContext = vhdlGenerationContext.CreateContextForSubLevel();
            foreach (var when in Whens)
            {
                builder.Append(when.ToVhdl(subContext));
            }

            builder.Append("end case");

            return Terminated.Terminate(builder.ToString(), vhdlGenerationContext);
        }
    }


    [DebuggerDisplay("{ToVhdl()}")]
    public class When : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public When()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return 
                "when " + Expression.ToVhdl(vhdlGenerationContext) + " => " + vhdlGenerationContext.NewLineIfShouldFormat() +
                (Body.Count != 0 ? 
                    Body.ToVhdl(vhdlGenerationContext.CreateContextForSubLevel()) : 
                    Terminated.Terminate(vhdlGenerationContext.IndentIfShouldFormat () + "null", vhdlGenerationContext));
        }
    }
}
