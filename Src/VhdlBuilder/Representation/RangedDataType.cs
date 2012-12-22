using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class RangedDataType : DataType
    {
        public int RangeMin { get; set; }
        public int RangeMax { get; set; }

        public override string ToVhdl()
        {
            if (RangeMin == 0 || RangeMax == 0) return Name;
            return Name + " range " + RangeMin + " to " + RangeMax;
        }
    }
}
