using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class Case : IVhdlElement
    {
        public string Expression { get; set; }
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
                .Append(Expression)
                .Append(" is ");

            foreach (var when in Whens)
            {
                builder.Append(when.ToVhdl());
            }

            builder.Append("end case;");

            return builder.ToString();
        }
    }

    public class When : IVhdlElement
    {
        public string Expression { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public When()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return 
                "when " +
                Expression +
                " => " +
                (Body.Count != 0 ? Body.ToVhdl() : "null;");
        }
    }
}
