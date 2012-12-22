using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    /// <summary>
    /// Any VHDL entity that's not implemented as a class
    /// </summary>
    public class Raw : IVhdlElement
    {
        public string Source { get; set; }

        public string ToVhdl()
        {
            return Source;
        }
    }
}
