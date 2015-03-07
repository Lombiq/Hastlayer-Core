using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts an identifier to be used in VHDL to an extended identifier
        /// </summary>
        public static string ToVhdlId(this string id)
        {
            return @"\" + id + @"\";
        }
    }
}
