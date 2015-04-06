using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Any VHDL code that's not implemented as a class.
    /// </summary>
    public class Raw : IVhdlElement
    {
        public string Source { get; set; }


        public Raw()
        {
        }

        public Raw(string source)
        {
            Source = source;
        }


        public string ToVhdl()
        {
            return Source;
        }
    }
}
