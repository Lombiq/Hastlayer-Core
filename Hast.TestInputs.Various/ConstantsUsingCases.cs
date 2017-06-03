using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class ConstantsUsingCases
    {
        public void ConstantValuedVariables(int input)
        {
            // x, y and z are constant but u isn't.
            var x = 4;

            var y = 9 << 2;
            y++;

            var z = x * y;

            var u = 8;
            u = z + input;

            // The array creation should also use the substituted constants.
            var array = new int[y];

            // While v only has constant assignments due to the dynamic condition it doesn't have a constant value.
            var v = 5;
            if (input < 5)
            {
                v += 8;
            }
            else
            {
                v = 10;
            }

            // While w only has constant values it's not just assigned to but also read between assignments so can only
            // be partially substituted.
            var w = z + 5;
            if (w == 10)
            {
                w = x + 1;
            }
            w += 10;
        }
    }
}
