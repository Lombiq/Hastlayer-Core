using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Showcasing some simple recursive algorithms, since recursive method calls are also supported by Hastlayer.
    /// </summary>
    public class RecursiveAlgorithms
    {
        public const int CalculateFibonacchiSeries_InputShortIndex = 0;
        public const int CalculateFibonacchiSeries_OutputUInt32Index = 0;
        public const int CalculateFactorial_InputShortIndex = 0;
        public const int CalculateFactorial_OutputUInt32Index = 0;


        public virtual void CalculateFibonacchiSeries(SimpleMemory memory)
        {
            var number = (short)memory.ReadInt32(CalculateFibonacchiSeries_InputShortIndex);
            memory.WriteUInt32(CalculateFibonacchiSeries_OutputUInt32Index, CalculateFibonacchiSeries(number));
        }

        public virtual void CalculateFactorial(SimpleMemory memory)
        {
            var number = (short)memory.ReadInt32(CalculateFactorial_InputShortIndex);
            memory.WriteUInt32(CalculateFactorial_OutputUInt32Index, CalculateFactorial(number));
        }


        // The return value should be a type with a bigger range than the input. But we can't use larger than 32b numbers
        // yet so the input needs to be a short.
        private uint CalculateFibonacchiSeries(short number)
        {
            if (number == 0 || number == 1) return (uint)number;
            return CalculateFibonacchiSeries((short)(number - 2)) + CalculateFibonacchiSeries((short)(number - 1));
        }

        private uint CalculateFactorial(short number)
        {
            if (number == 0)  return 1;
            return (uint)(number * CalculateFactorial((short)(number - 1)));
        }
    }


    public static class RecursiveAlgorithmsExtensions
    {
        public static uint CalculateFibonacchiSeries(this RecursiveAlgorithms recursiveAlgorithms, short number)
        {
            var memory = new SimpleMemory(1);
            memory.WriteInt32(RecursiveAlgorithms.CalculateFibonacchiSeries_InputShortIndex, number);
            recursiveAlgorithms.CalculateFibonacchiSeries(memory);
            return memory.ReadUInt32(RecursiveAlgorithms.CalculateFibonacchiSeries_OutputUInt32Index);
        }

        public static uint CalculateFactorial(this RecursiveAlgorithms recursiveAlgorithms, short number)
        {
            var memory = new SimpleMemory(1);
            memory.WriteInt32(RecursiveAlgorithms.CalculateFactorial_InputShortIndex, number);
            recursiveAlgorithms.CalculateFactorial(memory);
            return memory.ReadUInt32(RecursiveAlgorithms.CalculateFactorial_OutputUInt32Index);
        }
    }
}
