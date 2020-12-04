using System;
using System.Collections.Immutable;

namespace Hast.TestInputs.Invalid
{
    public class InvalidArrayUsingCases
    {
        public void InvalidArrayAssignment()
        {
            // Since array size can only be statically defined using the same method (which has only one hardware array
            // "instance") invocations with different array sizes are invalid.

            var array1 = new[] { 1 };
            var value1 = GetItemValuePlusOne(array1, 0);
            var array2 = new[] { 1, 2, };
            var value2 = GetItemValuePlusOne(array2, 0);
        }

        public void ArraySizeIsNotStatic(int arraySize)
        {
            var array = new int[arraySize + 1];
        }

        public void ArrayCopyToIsNotStaticCopy(int input)
        {
            var array1 = new int[5];
            var array2 = new int[5];
            Array.Copy(array1, array2, input);
        }
        
        public void UnsupportedImmutableArrayCreateRangeUsage()
        {
            var immutableArray1 = ImmutableArray.CreateRange(new[] { 1 });
            var immutableArray2 = ImmutableArray.CreateRange(immutableArray1, item => item);
        }

        public void MultiDimensionalArray()
        {
            var array = new int[2, 3];
            array[0, 1] = 10;
        }

        public void NullAssignment()
        {
            var array1 = new int[5];
            array1 = null;
            var x = array1.Length;
            var y = -x;
        }

        private int GetItemValuePlusOne(int[] array, int itemIndex) => array[itemIndex] + 1;
    }
}
