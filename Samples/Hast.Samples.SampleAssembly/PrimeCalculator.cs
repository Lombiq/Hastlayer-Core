﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    public class PrimeCalculator
    {
        public virtual bool IsPrimeNumber(int num)
        {
            int factor = num / 2;
            //var z = Math.Max(10, 4);
            //var y = z;
            //var factor = Math.Sqrt(num); // Math.Sqrt() can't be processed yet

            for (int i = 2; i <= factor; i++)
            {
                if ((num % i) == 0) return false;
            }

            return true;
        }

        // Arrays not yet supported
        /*public virtual int[] PrimeFactors(int num)
        {
            var i = 0;
            var result = new int[50];

            int divisor = 2;

            while (divisor <= num)
            {
                if (num % divisor == 0)
                {
                    result[i++] = divisor;
                    num /= divisor;
                }
                else divisor++;
            }

            return result;
        }*/
    }
}
