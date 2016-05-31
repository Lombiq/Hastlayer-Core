using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// An algorithm that doesn't do anything useful but showcases what kind of algorithms are best suited to
    /// accelerate with Hastlayer.
    /// </summary>
    public class HastlayerOptimizedAlgorithm
    {
        public const int Run_InputUInt32Index = 0;
        public const int Run_OutputUInt32Index = 0;

        public const int MaxDegreeOfParallelism = 50;


        public virtual void Run(SimpleMemory memory)
        {
            var input = memory.ReadUInt32(Run_InputUInt32Index);
            var tasks = new Task<uint>[MaxDegreeOfParallelism];

            for (int i = 0; i < MaxDegreeOfParallelism; i++)
            {
                tasks[i] = Task.Factory.StartNew<uint>(
                    indexObject =>
                    {
                        var index = (int)indexObject;
                        uint result = (uint)index * 2;

                        for (int j = 0; j < 99999; j++)
                        {
                            var a = result + j;
                            var b = result + j + 23;
                            var c = j - 23;
                            var d = -3 + j;
                            var e = result + 2399 + result;
                            var f = 942 - result - j;
                            var g = result + j - 144;
                            var h = result - j - 23449;
                            var ii = result + j + 999;
                            var jj = result - j + 1175;

                            var sum = a + b + c + d + e + f + g + h + ii + jj;

                            result += (uint)sum;
                        }

                        return result;
                    },
                    i);
            }

            Task.WhenAll(tasks).Wait();

            uint output = 0;
            for (int i = 0; i < MaxDegreeOfParallelism; i++)
            {
                output += tasks[i].Result;
            }

            memory.WriteUInt32(Run_OutputUInt32Index, output);
        }
    }


    public static class HastlayerOptimizedAlgorithmExtensions
    {
        public static uint Run(this HastlayerOptimizedAlgorithm algorithm, uint input)
        {
            var memory = new SimpleMemory(1);
            memory.WriteUInt32(HastlayerOptimizedAlgorithm.Run_InputUInt32Index, input);
            algorithm.Run(memory);
            return memory.ReadUInt32(HastlayerOptimizedAlgorithm.Run_OutputUInt32Index);
        }
    }
}
