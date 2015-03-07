using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Array : DataType
    {
        public int MaxLength { get; set; }
        public DataType StoredType { get; set; }


        public Array()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl()
        {
            return
                "type " +
                Name.ToVhdlId() +
                " is array (" +
                (MaxLength > 0 ? MaxLength + " downto 0" : "integer range <>") +
                ") of " +
                StoredType.Name +
                ";";
        }
    }
}
