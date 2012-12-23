using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class Identifier : DataType
    {
        public Identifier()
        {
            TypeCategory = DataTypeCategory.Identifier;
        }

        public override string ToVhdl()
        {
            return string.Empty;
        }
    }
}
