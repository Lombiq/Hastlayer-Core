using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public enum PortMode
    {
        In,
        Out,
        Buffer,
        InOut
    }

    public class Port : IVhdlElement
    {
        public string Name { get; set; }
        public PortMode Mode { get; set; }
        public DataType DataType { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder();
            builder
                .Append(Name)
                .Append(": ")
                .Append(Mode)
                .Append(" ")
                .Append(DataType.ToVhdl())
                .Append(";");

            return builder.ToString();
        }
    }
}
