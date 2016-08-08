using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    /// <summary>
    /// Represents a VHDL null statement (mostly used in case constructs).
    /// </summary>
    public class Null : IVhdlElement
    {
        private static readonly Null _instance = new Null();
        public static Null Instance { get { return _instance; } }


        private Null()
        {
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            // It shouldn't be always terminated, so not terminating it here. See: 
            // http://www.vhdl.renerta.com/mobile/source/vhd00045.htm
            return "null";
        }
    }
}
