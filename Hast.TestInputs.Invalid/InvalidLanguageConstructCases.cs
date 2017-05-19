using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Invalid
{
    public class InvalidLanguageConstructCases
    {
        public void BreakStatements()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) break;
            }
        }
    }
}
