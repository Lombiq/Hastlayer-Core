using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Tests.TestAssembly1
{
    /// <summary>
    /// Demonstrates certain declarations that aren't used anywhere and thus shouldn't be included in the AST.
    /// </summary>
    public class UnusedDeclarations
    {
        public bool UnusedMethod()
        {
            return true;
        }
    }
}
