using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class StdLogicVector : SizedDataType
    {
        public StdLogicVector()
        {
            Name = "std_logic_vector";
            TypeCategory = DataTypeCategory.Array;
        }
    }
}
