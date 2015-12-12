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


        public string ToVhdl()
        {
            var builder = new StringBuilder();

            builder
                .Append("case ")
                .Append(Expression.ToVhdl())
                .Append(" is ");

            foreach (var when in Whens)
            {
                builder.Append(when.ToVhdl());
            }

            builder.Append("end case;");

            return builder.ToString();
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


        public string ToVhdl()
        {
            return 
                "when " +
                Expression.ToVhdl() +
                " => " +
                (Body.Count != 0 ? Body.ToVhdl() : "null;");
        }
    }
}
