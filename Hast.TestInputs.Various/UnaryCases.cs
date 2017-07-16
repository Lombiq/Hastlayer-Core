using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class UnaryCases
    {
        public void IncrementDecrement(int input)
        {
            var array = new int[5];
            if (input < 10)
            {
                // These unary expressions will remain in the AST and thus needs to be handled.
                array[input++] = 3;
                array[input--] = 3;
            }
        }
    }
}
