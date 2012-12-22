using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Process : IVhdlElement
    {
        public string Name { get; set; }
        public List<string> SesitivityList { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Process()
        {
            SesitivityList = new List<string>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            var builder = new StringBuilder(11);

            if (!String.IsNullOrEmpty(Name))
            {
                builder
                    .Append(Name.ToVhdlId())
                    .Append(": ");
            }

            builder
                .Append("process (")
                .Append(String.Join(", ", SesitivityList.Select(signal => signal.ToVhdlId())))
                .Append(") ")
                .Append(Declarations.ToVhdl())
                .Append(" begin ")
                .Append(Body.ToVhdl())
                .Append(" end process;");

            return builder.ToString();
        }
    }
}
