﻿using System;
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

            // Since w only has constant values the condition can be evaluated compile-time and due to it being false
            // the whole if can be removed.
            var w = z + 5;
            if (w == 10)
            {
                w = x + 1;
            }
            w += 10;
        }

        public void ConstantPassingToMethod(int input)
        {
            var x = 15;
            var y = x * 10 - 5;
            // The result of StaticConstantUsingMethod() can even be inlined here.
            var z = ConstantUsingMethod(y, input) + StaticConstantUsingMethod(y);
        }

        public void ConstantPassingToObject()
        {
            var arraySize = 5;
            var array = new uint[arraySize];

            // These constructor parameters can be substituted.
            var arrayHolder = new ArrayHolder1(array);
            var array1 = arrayHolder.Array;
            var arrayLength1 = arrayHolder.ArrayLength;
            var arrayLengthCopy1 = arrayHolder.ArrayLengthCopy;
            var NonSubstitutableArrayLengthCopy1 = arrayHolder.NonSubstitutableArrayLengthCopy;
            var arrayHolder2 = new ArrayHolder1((uint)arraySize);
            var arrayHolder3 = new ArrayHolder1((uint)array.Length);

            // These constructor parameters can't be substituted because there are different ones.
            var arrayHolder4 = new ArrayHolder2((uint)arraySize);
            var arrayHolder5 = new ArrayHolder2((uint)arraySize + 8);
        }


        private int ConstantUsingMethod(int input1, int input2) => input1 * 2 + 8 - input2;


        private static int StaticConstantUsingMethod(int input) => input * 2 + 8;

        private class ArrayHolder1
        {
            public uint ArrayLength { get; }
            public uint ArrayLengthCopy { get; }
            public uint NonSubstitutableArrayLengthCopy { get; set; }
            public uint[] Array { get; }


            public ArrayHolder1(uint[] array)
            {
                ArrayLength = (uint)array.Length;
                ArrayLengthCopy = ArrayLength << 5;
                NonSubstitutableArrayLengthCopy = ArrayLengthCopy;
                Array = new uint[ArrayLength];
            }

            public ArrayHolder1(uint size)
            {
                ArrayLength = (size >> 5) + (size % 32 == 0 ? 0 : (uint)5);
                ArrayLengthCopy = ArrayLength << 5;
                NonSubstitutableArrayLengthCopy = ArrayLengthCopy;
                Array = new uint[ArrayLength];
            }
        }

        private class ArrayHolder2
        {
            public uint ArrayLength { get; }


            public ArrayHolder2(uint size)
            {
                ArrayLength = (size >> 5) + (size % 32 == 0 ? 0 : (uint)5);
            }
        }
    }
}