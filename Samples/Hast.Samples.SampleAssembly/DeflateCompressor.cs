using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Sample demonstrating de/compression with the deflate algorithm.
    /// </summary>
    public class DeflateCompressor
    {
        public const int Deflate_InputOutputCountUInt32Index = 0;
        public const int Deflate_InputOutputStartIndex = 1;


        public virtual void Deflate(SimpleMemory memory)
        {
            var inputCount = memory.ReadInt32(Deflate_InputOutputCountUInt32Index);

        }
    }


    public static class DeflateCompressorExtensions
    {
        public static byte[] Deflate(this DeflateCompressor deflateCompressor, byte[] dataToCompress)
        {
            var inputCellCount = (int)Math.Ceiling((double)dataToCompress.Length / SimpleMemory.MemoryCellSizeBytes);
            var memory = new SimpleMemory(inputCellCount + 1);

            for (int i = 1; i <= inputCellCount; i++)
            {
                var inputIndex = i * SimpleMemory.MemoryCellSizeBytes;

                memory.Write4Bytes(
                    i,
                    new[]
                    {
                        dataToCompress[inputIndex],
                        dataToCompress[inputIndex + 1],
                        dataToCompress[inputIndex + 2],
                        dataToCompress[inputIndex + 3]
                    });
            }

            deflateCompressor.Deflate(memory);

            var outputCellCount = memory.ReadUInt32(DeflateCompressor.Deflate_InputOutputCountUInt32Index);
            var output = new byte[outputCellCount * SimpleMemory.MemoryCellSizeBytes];

            for (int i = 1; i <= outputCellCount; i++)
            {
                var outputCell = memory.Read4Bytes(i);
                var outputIndex = i * SimpleMemory.MemoryCellSizeBytes;
                output[outputIndex] = outputCell[0];
                output[outputIndex + 1] = outputCell[1];
                output[outputIndex + 2] = outputCell[2];
                output[outputIndex + 3] = outputCell[3];
            }

            return output;
        }
    }
}
