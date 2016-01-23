using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Example for a SimpleMemory-using algorithm.
    /// </summary>
    public class PrimeCalculator
    {
        // It's good to have externally interesting cell indices in constants like this, so they can be used from wrappers 
        // like below. Note the Hungarian notation-like prefixes. It's unfortunate but we need them here for clarity.
        public const int IsPrimeNumber_InputUInt32Index = 0;
        public const int IsPrimeNumber_OutputBooleanIndex = 0;
        public const int ArePrimeNumbers_InputUInt32CountIndex = 0;
        public const int ArePrimeNumbers_InputUInt32sStartIndex = 1;
        public const int ArePrimeNumbers_OutputUInt32sStartIndex = 1;

    
        /// <summary>
        /// Calculates whether a number is prime.
        /// </summary>
        /// <remarks>
        /// Note that the entry point of SimpleMemory-using algorithms should be void methods having a single 
        /// <see cref="SimpleMemory"/> argument. 
        /// </remarks>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void IsPrimeNumber(SimpleMemory memory)
        {
            // Reading out the input parameter.
            var number = memory.ReadUInt32(IsPrimeNumber_InputUInt32Index);
            // Writing back the output.
            memory.WriteBoolean(IsPrimeNumber_OutputBooleanIndex, IsPrimeNumber(number));
        }

        public virtual void ArePrimeNumbers(SimpleMemory memory)
        {
            // We need this information explicitly as we can't use arrays.
            uint numberCount = memory.ReadUInt32(ArePrimeNumbers_InputUInt32CountIndex);

            for (int i = 0; i < numberCount; i++)
            {
                uint number = memory.ReadUInt32(ArePrimeNumbers_InputUInt32sStartIndex + i);
                memory.WriteBoolean(ArePrimeNumbers_OutputUInt32sStartIndex + i, IsPrimeNumber(number));
            }
        }

        // Arrays not yet supported
        /*public virtual int[] PrimeFactors(int number)
        {
            var i = 0;
            var result = new int[50];

            int divisor = 2;

            while (divisor <= number)
            {
                if (number % divisor == 0)
                {
                    result[i++] = divisor;
                    number /= divisor;
                }
                else divisor++;
            }

            return result;
        }*/


        /// <summary>
        /// Internal implementation of prime number checking. This is here so we can use it simpler from two methods.
        /// Because when you want to pass data between methods you can freely use supported types as arguments, you 
        /// don't need to pass data through SimpleMemory.
        /// </summary>
        private bool IsPrimeNumber(uint number)
        {
            uint factor = number / 2;
            //var factor = Math.Sqrt(number); // Math.Sqrt() can't be processed yet because it needs object support.

            for (uint i = 2; i <= factor; i++)
            {
                if ((number % i) == 0) return false;
            }

            return true;
        }
    }


    /// <summary>
    /// Extension methods so the SimpleMemory-using PrimeCalculator is easier to consume.
    /// </summary>
    public static class PrimeCalculatorExtensions
    {
        public static bool IsPrimeNumber(this PrimeCalculator primeCalculator, uint number)
        {
            // One memory cell is enough for data exchange.
            var memory = new SimpleMemory(1);
            memory.WriteUInt32(PrimeCalculator.IsPrimeNumber_InputUInt32Index, number);
            primeCalculator.IsPrimeNumber(memory);
            return memory.ReadBoolean(PrimeCalculator.IsPrimeNumber_OutputBooleanIndex);
        }

        public static bool[] ArePrimeNumbers(this PrimeCalculator primeCalculator, uint[] numbers)
        {
            // We need to allocate more memory cells, enough for all the inputs and outputs.
            var memory = new SimpleMemory(numbers.Length + 1);

            memory.WriteUInt32(PrimeCalculator.ArePrimeNumbers_InputUInt32CountIndex, (uint)numbers.Length);

            for (int i = 0; i < numbers.Length; i++)
            {
                memory.WriteUInt32(PrimeCalculator.ArePrimeNumbers_InputUInt32sStartIndex + i, numbers[i]);
            }

            primeCalculator.ArePrimeNumbers(memory);


            var output = new bool[numbers.Length];
            for (int i = 0; i < numbers.Length; i++)
            {
                output[i] = memory.ReadBoolean(PrimeCalculator.ArePrimeNumbers_OutputUInt32sStartIndex + i);
            }
            return output;
        }
    }
}
