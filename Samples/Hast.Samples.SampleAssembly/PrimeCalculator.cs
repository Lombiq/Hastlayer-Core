using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    public class PrimeCalculator
    {
        public const int IsPrimeNumber_InputUInt32Index = 0;
        public const int IsPrimeNumber_OutputBooleanIndex = 0;
        public const int ArePrimeNumbers_InputUInt32CountIndex = 0;
        public const int ArePrimeNumbers_InputUInt32sStartIndex = 1;
        public const int ArePrimeNumbers_OutputBooleansStartIndex = 1;

        public const int MaxDegreeOfParallelism = 5;


        public virtual void IsPrimeNumber(SimpleMemory memory)
        {
            var number = memory.ReadUInt32(IsPrimeNumber_InputUInt32Index);
            memory.WriteBoolean(IsPrimeNumber_OutputBooleanIndex, IsPrimeNumberInternal(number));
        }

        public virtual Task IsPrimeNumberAsync(SimpleMemory memory)
        {
            IsPrimeNumber(memory);

            return Task.CompletedTask;
        }

        public virtual void ArePrimeNumbers(SimpleMemory memory)
        {
            uint numberCount = memory.ReadUInt32(ArePrimeNumbers_InputUInt32CountIndex);

            for (int i = 0; i < numberCount; i++)
            {
                uint number = memory.ReadUInt32(ArePrimeNumbers_InputUInt32sStartIndex + i);
                memory.WriteBoolean(ArePrimeNumbers_OutputBooleansStartIndex + i, IsPrimeNumberInternal(number));
            }
        }

        public virtual void ParallelizedArePrimeNumbers(SimpleMemory memory)
        {
            uint numberCount = memory.ReadUInt32(ArePrimeNumbers_InputUInt32CountIndex);

            var tasks = new Task<bool>[MaxDegreeOfParallelism];
            int i = 0;
            while (i < numberCount)
            {
                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    var currentNumber = memory.ReadUInt32(ArePrimeNumbers_InputUInt32sStartIndex + i + m);

                    tasks[m] = Task.Factory.StartNew<bool>(
                        numberObject =>
                        {
                            var number = (uint)numberObject;
                            uint factor = number / 2;

                            for (uint x = 2; x <= factor; x++)
                            {
                                if ((number % x) == 0) return false;
                            }

                            return true;
                        },
                        currentNumber);
                }

                Task.WhenAll(tasks).Wait();

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    memory.WriteBoolean(ArePrimeNumbers_OutputBooleansStartIndex + i + m, tasks[m].Result);
                }

                i += MaxDegreeOfParallelism;
            }
        }


        private bool IsPrimeNumberInternal(uint number)
        {
            uint factor = number / 2;

            for (uint i = 2; i <= factor; i++)
            {
                if ((number % i) == 0) return false;
            }

            return true;
        }
    }


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
