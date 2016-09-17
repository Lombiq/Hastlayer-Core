using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly.Deflate
{
    public static class DeflateCompressorExtensions
    {
        public static byte[] Deflate(this DeflateCompressor deflateCompressor, byte[] dataToCompress)
        {
            ThrowIfConfigurationInvalid();

            var inputCellCount = (int)Math.Ceiling((double)dataToCompress.Length / SimpleMemory.MemoryCellSizeBytes);
            var memory = new SimpleMemory(inputCellCount + 1);

            memory.WriteInt32(DeflateCompressor.Deflate_InputOutputCountInt32Index, inputCellCount);

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

            var outputCellCount = memory.ReadInt32(DeflateCompressor.Deflate_InputOutputCountInt32Index);
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


        private static void ThrowIfConfigurationInvalid()
        {
            // Also taken over from the JS version.
            if (DeflateCompressor.zip_LIT_BUFSIZE > DeflateCompressor.zip_INBUFSIZ)
            {
                throw new InvalidOperationException("error: zip_INBUFSIZ is too small");
            }
            if ((DeflateCompressor.zip_WSIZE << 1) > (1 << DeflateCompressor.zip_BITS))
            {
                throw new InvalidOperationException("error: zip_WSIZE is too large");
            }
            if (DeflateCompressor.zip_HASH_BITS > DeflateCompressor.zip_BITS - 1)
            {
                throw new InvalidOperationException("error: zip_HASH_BITS is too large");
            }
            if (DeflateCompressor.zip_HASH_BITS < 8 || DeflateCompressor.zip_MAX_MATCH != 258)
            {
                throw new InvalidOperationException("error: Code too clever");
            }
        }
    }
}
