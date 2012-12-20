using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    public class Entity : IVhdlElement
    {
        public string Name { get; set; }
        public Port[] Ports { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            builder
                .Append("entity ")
                .Append(Name)
                .Append(" is ")
                .Append("port(")
                .Append(Ports.ToVhdl())
                .Append(");")
                .Append("end ")
                .Append(Name)
                .Append(";");

            return builder.ToString();
        }
    }
}
