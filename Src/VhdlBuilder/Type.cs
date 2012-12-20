using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    /// <summary>
    /// VHDL TYPE declaration, i.e. TYPE name IS (value1, value2);
    /// </summary>
    public class Type : IVhdlElement
    {
        public string Name { get; set; }
        public string[] Values { get; set; }

        public string ToVhdl()
        {
            throw new NotImplementedException();
        }
    }
}
