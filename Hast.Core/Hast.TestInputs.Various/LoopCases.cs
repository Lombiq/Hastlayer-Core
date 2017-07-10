using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class LoopCases
    {
        public void BreakInLoop(int input)
        {
            var sum = input;

            for (int i = 0; i < input; i++)
            {
                sum += i;

                if (sum > 10)
                {
                    break;
                }
            }
        }

        public void BreakInLoopInLoop(int input)
        {
            var sum = input;

            for (int i = 0; i < input; i++)
            {
                for (int x = 0; x < i; x++)
                {
                    sum += i;

                    if (sum > 10)
                    {
                        break;
                    }
                }
            }
        }
    }
}
