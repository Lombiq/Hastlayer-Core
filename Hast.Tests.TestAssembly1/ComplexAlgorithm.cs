using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Tests.TestAssembly1
{
    /// <summary>
    /// A type implementing "complex" algorithms, demonstrating as many control structures and numeric features as supported.
    /// </summary>
    public class ComplexAlgorithm
    {
        public virtual bool IsPrimeNumber(int num)
        {
            int factor = num / 2;

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
