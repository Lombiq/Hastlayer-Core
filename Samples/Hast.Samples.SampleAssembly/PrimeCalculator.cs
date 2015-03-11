using System;
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
            var isPrime = true;
            int factor = num / 2;
            //var factor = Math.Sqrt(num); Math.Sqrt() can't be processed yet

            for (int i = 2; i <= factor; i++)
            {
                if ((num % i) == 0) isPrime = false;
            }

            return isPrime;
        }
    }
}
