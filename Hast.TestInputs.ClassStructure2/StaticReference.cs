using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.TestInputs.ClassStructure1.ComplexTypes;

namespace Hast.TestInputs.ClassStructure2
{
    /// <summary>
    /// Demonstrates access to a static class (similar how e.g. the Math class would be used).
    /// </summary>
    public class StaticReference
    {
        public virtual int StaticClassUsingMethod()
        {
            if (!StaticClass.StaticMethod())
            {
                return 2;
            }

            return 3 + 4;
        }
    }
}
