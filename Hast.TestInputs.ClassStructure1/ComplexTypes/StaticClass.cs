using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.ClassStructure1.ComplexTypes
{
    /// <summary>
    /// Demonstrates a static class in a separate assembly that should be usable from transformed methods.
    /// </summary>
    public static class StaticClass
    {
        public static bool StaticMethod()
        {
            return true;
        }
    }
}
