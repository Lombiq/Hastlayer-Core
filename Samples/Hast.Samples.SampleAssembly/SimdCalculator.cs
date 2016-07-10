using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SpecialOperations;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Sample to showcase SIMD (Simple Instruction Multiple Data) processing usage, i.e. operations executed in parallel
    /// on multiple elements of vectors.
    /// </summary>
    /// <remarks>
    /// System.Numerics.Vectors (including the NuGet package version of it: http://www.nuget.org/packages/System.Numerics.Vectors)
    /// could be used for SIMD processing on x64 systems. However Vector<T> can only contain that many elements that can
    /// fit into the processor's SIMD register and thus is quite inconvenient to use. So using a custom implementation.
    /// </remarks>
    public class SimdCalculator
    {
        public const int AddVectors_VectorsElementCountInt32Index = 0;
        public const int AddVectors_VectorElementsStartInt32Index = 1;
        public const int AddVectors_SumVectorElementsStartInt32Index = 1;

        public const int MaxDegreeOfParallelism = 500;


        public virtual void AddVectors(SimpleMemory memory)
        {
            var elementCount = memory.ReadInt32(AddVectors_VectorsElementCountInt32Index);

            int i = 0;

            while (i < elementCount)
            {
                var vector1 = new int[MaxDegreeOfParallelism];
                var vector2 = new int[MaxDegreeOfParallelism];
                var resultVector = new int[MaxDegreeOfParallelism];

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    vector1[m] = memory.ReadInt32(AddVectors_VectorElementsStartInt32Index + i + m);
                }

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    vector2[m] = memory.ReadInt32(AddVectors_VectorElementsStartInt32Index + i + m + elementCount);
                }

                resultVector = SimdOperations.AddVectors(vector1, vector2, MaxDegreeOfParallelism);

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    memory.WriteInt32(AddVectors_SumVectorElementsStartInt32Index + i + m, resultVector[m]);
                }

                i += MaxDegreeOfParallelism;
            }
        }
    }


    public static class SimdCalculatorExtensions
    {
        public static int[] AddVectors(this SimdCalculator algorithm, int[] vector1, int[] vector2)
        {
            SimdOperations.ThrowIfVectorsNotEquallyLong(vector1, vector2);

            var originalElementCount = vector1.Length;

            vector1 = vector1.PadToMultipleOf(SimdCalculator.MaxDegreeOfParallelism);
            vector2 = vector2.PadToMultipleOf(SimdCalculator.MaxDegreeOfParallelism);

            var elementCount = vector1.Length;
            var memory = new SimpleMemory(1 + elementCount * 2);

            memory.WriteInt32(SimdCalculator.AddVectors_VectorsElementCountInt32Index, 15);

            for (int i = 0; i < elementCount; i++)
            {
                memory.WriteInt32(SimdCalculator.AddVectors_VectorElementsStartInt32Index + i, vector1[i]);
                memory.WriteInt32(SimdCalculator.AddVectors_VectorElementsStartInt32Index + elementCount + i, vector2[i]);
            }

            algorithm.AddVectors(memory);

            var result = new int[elementCount];

            for (int i = 0; i < elementCount; i++)
            {
                result[i] = memory.ReadInt32(SimdCalculator.AddVectors_SumVectorElementsStartInt32Index + i);
            }

            return result.CutToLength(originalElementCount);
        }
    }
}
