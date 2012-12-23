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
        staticant,
        Variable,
        Signal,
        File
    }

    public class DataObject : IDataObject
    {
        public ObjectType Type { get; set; }
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public Value Value { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder();

            builder
                .Append(Type)
                .Append(" ")
                .Append(Name.ToVhdlId())
                .Append(": ");

            if (DataType != null) builder.Append(DataType.ToVhdl());
            else if (Value != null) builder.Append(Value.DataType.ToVhdl());

            if (Value != null)
            {
                builder
                    .Append(" := ")
                    .Append(Value.ToVhdl());
            }

            builder.Append(";");

            return builder.ToString();
        }
    }
}
