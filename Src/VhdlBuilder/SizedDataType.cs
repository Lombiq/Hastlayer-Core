using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    public class SizedDataType : DataType
    {
        public int Size { get; set; }

        public override string ToVhdl()
        {
            if (Size == 0) return Name;
            return Name + "(" + (Size - 1) + " downto 0)";
        }
    }
}
