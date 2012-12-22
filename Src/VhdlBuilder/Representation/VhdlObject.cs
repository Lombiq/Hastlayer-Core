using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public enum ObjectType
    {
        Constant,
        Variable,
        Signal,
        File
    }

    public class VhdlObject : IVhdlElement
    {
        public ObjectType Type { get; set; }
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string Value { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder(8);

            builder
                .Append(Type)
                .Append(" ")
                .Append(Name.ToVhdlId())
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
