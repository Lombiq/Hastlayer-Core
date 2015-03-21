using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
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
