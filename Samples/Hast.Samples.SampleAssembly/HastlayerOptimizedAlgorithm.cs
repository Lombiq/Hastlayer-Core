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

        public const int MaxDegreeOfParallelism = 200;


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
                        uint result = input + (uint)index * 2;

                        for (int j = 0; j < 9999999; j++)
                        {
                            result += (uint)j;
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
