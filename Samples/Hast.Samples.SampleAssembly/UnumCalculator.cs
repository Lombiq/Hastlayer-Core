using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;
using Hast.Common.Numerics.Unum;

namespace Hast.Samples.SampleAssembly
{
    public class UnumCalculator
    {
        public const int AddToUnum_InputInt32Index = 0;
        public const int AddToUnum_OutputInt32Index = 0;

        public virtual void AddToUnum(SimpleMemory memory)
        {
            var number = memory.ReadInt32(AddToUnum_InputInt32Index);

            Unum unum = 10;
            // Adding this line only because otherwise the whole data structure would be cleaned up as unused.
            var sign = unum.IsPositive();

            int x = (int)unum;
            Unum y = number;

            var result = number + unum;

            memory.WriteInt32(AddToUnum_OutputInt32Index, (int)result);
        }
    }


    public static class UnumCalculatorExtensions
    {
        public static int AddToUnum(this UnumCalculator unumCalculator, int number)
        {
            var memory = new SimpleMemory(1);
            memory.WriteInt32(UnumCalculator.AddToUnum_InputInt32Index, number);
            unumCalculator.AddToUnum(memory);
            return memory.ReadInt32(UnumCalculator.AddToUnum_OutputInt32Index);
        }
    }
}
