using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a no-op VHDL element.
    /// </summary>
    public class Empty : IVhdlElement
    {
        private static readonly Empty _instance = new Empty();
        public static Empty Instance { get { return _instance; } }

        private Empty()
        {
        }


        public string ToVhdl()
        {
            return string.Empty;
        }
    }
}
