using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class ArrayUsingCases
    {
        public void InvalidArrayUsage()
        {
            // Since array size can only be statically defined using the same method (which has only one hardware array
            // "instance") invocations with different array sizes are invalid.

            var array1 = new[] { 1 };
            var value1 = GetItemValuePlusOne(array1, 0);
            var array2 = new[] { 1, 2, };
            var value2 = GetItemValuePlusOne(array2, 0);
        }

        private int GetItemValuePlusOne(int[] array, int itemIndex) => array[itemIndex] + 1;
    }
}
