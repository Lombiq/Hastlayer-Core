using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class Variable : IVhdlElement
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string Value { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            builder
                .Append("variable ")
                .Append(Name)
                .Append(": ")
                .Append(DataType.ToVhdl());

            if (!String.IsNullOrEmpty(Value))
            {
                builder
                    .Append(" := ")
                    .Append(Value);
            }

            builder.Append(";");

            return builder.ToString();
        }
    }
}
