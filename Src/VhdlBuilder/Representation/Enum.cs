using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Enum : DataType
    {
        public List<Value> Values { get; set; }


        public Enum()
        {
            TypeCategory = DataTypeCategory.Composite;
            Values = new List<Value>();
        }


        public override string ToVhdl()
        {
            return
                "type " +
                Name.ToVhdlId() +
                " is (" +
                string.Join(", ", Values.Select(value => value.ToVhdl())) +
                ");";
        }
    }
}
