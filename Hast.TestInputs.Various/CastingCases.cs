using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class CastingCases
    {
        public void NumberCasting()
        {
            short a = 2;
            short b = 345;
            short c = (short)(a * b);
            int d = a * b;
        }
    }
}
