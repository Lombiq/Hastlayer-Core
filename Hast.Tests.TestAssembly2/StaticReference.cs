using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Tests.TestAssembly1;

namespace Hast.Tests.TestAssembly2
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
