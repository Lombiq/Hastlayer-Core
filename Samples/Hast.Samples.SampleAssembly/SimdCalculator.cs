using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public const int MaxDegreeOfParallelism = 35;


        public virtual void AddVectors(SimpleMemory memory)
        {
            var elementCount = memory.ReadInt32(AddVectors_VectorsElementCountInt32Index);

            int i = 0;
            while (i < elementCount)
            {
                Vector<int> resultVector;

                var intermediaryArray = new int[MaxDegreeOfParallelism];

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    intermediaryArray[m] = memory.ReadInt32(AddVectors_VectorElementsStartInt32Index + i + m);
                }

                var vector1 = new Vector<int>(intermediaryArray);

                for (int m = 0; m < MaxDegreeOfParallelism; m++)
                {
                    intermediaryArray[m] = memory.ReadInt32(AddVectors_VectorElementsStartInt32Index + i + m + elementCount);
                }

                var vector2 = new Vector<int>(intermediaryArray);

                resultVector = vector1 + vector2;

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
        public static int[] AddVectors(this SimdCalculator algorithm, int[] array1, int[] array2)
        {
            if (!Vector.IsHardwareAccelerated)
            {
                throw new InvalidOperationException("Vector operations would happen without hardware acceleration. You need to target the assembly to x64.");
            }

            if (array1.Length != array2.Length)
            {
                throw new NotSupportedException("The two vectors must have the same number of elements.");
            }

            var originalElementCount = array1.Length;

            array1 = array1.PadToMultipleOf(SimdCalculator.MaxDegreeOfParallelism);
            array2 = array2.PadToMultipleOf(SimdCalculator.MaxDegreeOfParallelism);

            var elementCount = array1.Length;
            var memory = new SimpleMemory(1 + elementCount * 2);

            memory.WriteInt32(SimdCalculator.AddVectors_VectorsElementCountInt32Index, 15);

            for (int i = 0; i < elementCount; i++)
            {
                memory.WriteInt32(SimdCalculator.AddVectors_VectorElementsStartInt32Index + i, array1[i]);
                memory.WriteInt32(SimdCalculator.AddVectors_VectorElementsStartInt32Index + elementCount + i, array2[i]);
            }

            algorithm.AddVectors(memory);

            var result = new int[elementCount];

            for (int i = 0; i < elementCount; i++)
            {
                result[i] = memory.ReadInt32(SimdCalculator.AddVectors_SumVectorElementsStartInt32Index);
            }

            return result.CutToLength(originalElementCount);
        }
    }
}
