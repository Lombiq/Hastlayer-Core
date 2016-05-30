﻿using System;
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
        public const int ArePrimeNumbers_OutputBooleansStartIndex = 1;

        public const int MaxDegreeOfParallelism = 35;


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
            memory.WriteBoolean(IsPrimeNumber_OutputBooleanIndex, IsPrimeNumberInternal(number));
        }

        public virtual Task IsPrimeNumberAsync(SimpleMemory memory)
        {
            IsPrimeNumber(memory);

            // For efficient parallel execution with multiple connected FPGA boards you can make a non-parallelized
            // interface method async with Task.FromResult(). In .NET 4.6+ Task.CompletedTask can be used too.
            return Task.FromResult(true);
        }

        public virtual void ArePrimeNumbers(SimpleMemory memory)
        {
            // We need this information explicitly as we can't store arrays directly in memory.
            uint numberCount = memory.ReadUInt32(ArePrimeNumbers_InputUInt32CountIndex);

            for (int i = 0; i < numberCount; i++)
            {
                uint number = memory.ReadUInt32(ArePrimeNumbers_InputUInt32sStartIndex + i);
                memory.WriteBoolean(ArePrimeNumbers_OutputBooleansStartIndex + i, IsPrimeNumberInternal(number));
            }
        }

        public virtual void ParallelizedArePrimeNumbers(SimpleMemory memory)
        {
            // We need this information explicitly as we can't store arrays directly in memory.
            uint numberCount = memory.ReadUInt32(ArePrimeNumbers_InputUInt32CountIndex);

            // At the moment Hastlayer only supports a fixed degree of parallelism so we need to pad the input array
            // if necessary, see PrimeCalculatorExtensions.
            var numbers = new uint[MaxDegreeOfParallelism];
            var tasks = new Task<bool>[MaxDegreeOfParallelism];
            int i = 0;
            while (i < numberCount)
            {
                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    numbers[m] = memory.ReadUInt32(ArePrimeNumbers_InputUInt32sStartIndex + i + m);
                }

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    tasks[m] = Task.Factory.StartNew<bool>(
                        indexObject =>
                        {
                            // This is a copy of the body of IsPrimeNumberInternal(). We could also call that method
                            // from this lambda but it's more efficient to just do it directly, not adding indirection.
                            var number = numbers[(int)indexObject];
                            uint factor = number / 2;

                            for (uint x = 2; x <= factor; x++)
                            {
                                if ((number % x) == 0) return false;
                            }

                            return true;
                        },
                        m);
                }

                // Hastlayer doesn't support async code at the moment since ILSpy doesn't handle the new Roslyn-compiled
                // code. See: https://github.com/icsharpcode/ILSpy/issues/502
                Task.WhenAll(tasks).Wait();

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    memory.WriteBoolean(ArePrimeNumbers_OutputBooleansStartIndex + i + m, tasks[m].Result);
                }

                i += MaxDegreeOfParallelism;
            }
        }


        /// <summary>
        /// Internal implementation of prime number checking. This is here so we can use it simpler from two methods.
        /// Because when you want to pass data between methods you can freely use supported types as arguments, you 
        /// don't need to pass data through SimpleMemory.
        /// </summary>
        private bool IsPrimeNumberInternal(uint number)
        {
            uint factor = number / 2;
            //uint factor = Math.Sqrt(number); // Math.Sqrt() can't be processed because it's not a managed method.

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
            return RunIsPrimeNumber(number, memory => Task.Run(() => primeCalculator.IsPrimeNumber(memory))).Result;
        }

        public static Task<bool> IsPrimeNumberAsync(this PrimeCalculator primeCalculator, uint number)
        {
            return RunIsPrimeNumber(number, memory => primeCalculator.IsPrimeNumberAsync(memory));
        }

        public static bool[] ArePrimeNumbers(this PrimeCalculator primeCalculator, uint[] numbers)
        {
            return RunArePrimeNumbersMethod(numbers, memory => Task.Run(() => primeCalculator.ArePrimeNumbers(memory))).Result;
        }

        public static async Task<bool[]> ParallelizedArePrimeNumbers(this PrimeCalculator primeCalculator, uint[] numbers)
        {
            // Padding the input array as necessary to have a multiple of MaxDegreeOfParallelism. This is needed because
            // at the moment Hastlayer only supports a fixed degree of parallelism. This is the simplest way to overcome 
            // this.
            var originalNumberCount = numbers.Length;
            var remainderToMaxDegreeOfParallelism = numbers.Length % PrimeCalculator.MaxDegreeOfParallelism;
            if (remainderToMaxDegreeOfParallelism != 0)
            {
                numbers = numbers
                    .Concat(new uint[PrimeCalculator.MaxDegreeOfParallelism - remainderToMaxDegreeOfParallelism])
                    .ToArray();
            }

            var results = await RunArePrimeNumbersMethod(
                numbers,
                memory => Task.Run(() => primeCalculator.ParallelizedArePrimeNumbers(memory)));

            if (remainderToMaxDegreeOfParallelism != 0)
            {
                return results.Take(originalNumberCount).ToArray();
            }

            return results;
        }

        private static async Task<bool> RunIsPrimeNumber(uint number, Func<SimpleMemory, Task> methodRunner)
        {
            // One memory cell is enough for data exchange.
            var memory = new SimpleMemory(1);
            memory.WriteUInt32(PrimeCalculator.IsPrimeNumber_InputUInt32Index, number);

            await methodRunner(memory);

            return memory.ReadBoolean(PrimeCalculator.IsPrimeNumber_OutputBooleanIndex);
        }

        private static async Task<bool[]> RunArePrimeNumbersMethod(uint[] numbers, Func<SimpleMemory, Task> methodRunner)
        {
            // We need to allocate more memory cells, enough for all the inputs and outputs.
            var memory = new SimpleMemory(numbers.Length + 1);

            memory.WriteUInt32(PrimeCalculator.ArePrimeNumbers_InputUInt32CountIndex, (uint)numbers.Length);
            for (int i = 0; i < numbers.Length; i++)
            {
                memory.WriteUInt32(PrimeCalculator.ArePrimeNumbers_InputUInt32sStartIndex + i, numbers[i]);
            }


            await methodRunner(memory);


            var output = new bool[numbers.Length];
            for (int i = 0; i < numbers.Length; i++)
            {
                output[i] = memory.ReadBoolean(PrimeCalculator.ArePrimeNumbers_OutputBooleansStartIndex + i);
            }
            return output;
        }
    }
}
