using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayTypeNameHelper
    {
        public static string CreateArrayTypeName(string elementTypeName)
        {
            return elementTypeName + "_Array";
        }
    }
}
