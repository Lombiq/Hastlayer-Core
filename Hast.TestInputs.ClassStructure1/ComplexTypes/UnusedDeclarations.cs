using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.ClassStructure1.ComplexTypes
{
    /// <summary>
    /// Demonstrates certain declarations that aren't used anywhere and thus shouldn't be included in the AST.
    /// </summary>
    public class UnusedDeclarations
    {
        public void UnusedMethod()
        {
            var x = 1;
        }
    }
}
