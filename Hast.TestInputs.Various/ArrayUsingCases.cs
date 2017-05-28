using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class ArrayUsingCases
    {
        public void PassArrayToConstructor()
        {
            var array = new int[5];
            var arrayHolder = new ArrayHolder(array);
            var arrayLength = arrayHolder.Array.Length;
        }


        private class ArrayHolder
        {
            public int[] Array { get; }


            public ArrayHolder(int[] array)
            {
                Array = array;
            }
        }
    }
}
