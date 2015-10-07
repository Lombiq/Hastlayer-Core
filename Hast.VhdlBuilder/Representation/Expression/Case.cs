using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Case : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }
        public List<When> Whens { get; set; }


        public Case()
        {
            Whens = new List<When>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var builder = new StringBuilder();

            builder
                .Append("case ")
                .Append(Expression.ToVhdl(vhdlGenerationOptions))
                .Append(" is ")
                .Append(vhdlGenerationOptions.NewLineIfShouldFormat());

            foreach (var when in Whens)
            {
                builder.Append(when.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions));
            }

            builder.Append("end case");

            return Terminated.Terminate(builder.ToString(), vhdlGenerationOptions);
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class When : IVhdlElement
    {
        public IVhdlElement Expression { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public When()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return 
                "when " + Expression.ToVhdl(vhdlGenerationOptions) + " => " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                (Body.Count != 0 ? 
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) : 
                    Terminated.Terminate(vhdlGenerationOptions.IndentIfShouldFormat () + "null", vhdlGenerationOptions));
        }
    }
}
