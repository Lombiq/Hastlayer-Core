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
        public virtual bool IsPrimeNumber(uint number)
        {
            uint factor = number / 2;

            for (uint i = 2; i <= factor; i++)
            {
                if ((number % i) == 0) return false;
            }

            return true;
        }
    }
}
