using Hast.Layer;
using Hast.Samples.Compression.Services.Lzma;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Hast.Samples.Compression.Services
{
    public static class LzmaCompressionService
    {
        public static async Task CompressFile(string inputFilePath, string outputFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);

            if (!inputFile.Exists) throw new HastlayerException("Input file doesn't exist.");

            outputFilePath = string.IsNullOrEmpty(outputFilePath) ? inputFile.Name + ".lzma" : outputFilePath;
            if (File.Exists(outputFilePath)) File.Delete(outputFilePath);

            using (var inputFileStream = inputFile.OpenRead())
            using (var outputFileStream = File.Create(outputFilePath))
            {
                string mf = "bt4";
                var dictionary = 1 << 23;
                var posStateBits = 2;
                int litContextBits = 3; // for normal files
                                        // UInt32 litContextBits = 0; // for 32-bit data
                int litPosBits = 0;
                // UInt32 litPosBits = 2; // for 32-bit data
                int algorithm = 2;
                int numFastBytes = 128;
                var stdInMode = false;
                bool eos = stdInMode;

                CoderPropertyId[] propIDs =
                {
                    CoderPropertyId.DictionarySize,
                    CoderPropertyId.PosStateBits,
                    CoderPropertyId.LitContextBits,
                    CoderPropertyId.LitPosBits,
                    CoderPropertyId.Algorithm,
                    CoderPropertyId.NumFastBytes,
                    CoderPropertyId.MatchFinder,
                    CoderPropertyId.EndMarker
                };
                object[] properties =
                {
                    (int)(dictionary),
                    (int)(posStateBits),
                    (int)(litContextBits),
                    (int)(litPosBits),
                    (int)(algorithm),
                    (int)(numFastBytes),
                    mf,
                    eos
                };
                var encoder = new LzmaEncoder();
                encoder.SetCoderProperties(propIDs, properties);
                encoder.WriteCoderProperties(outputFileStream);

                long fileSize;
                if (eos || stdInMode)
                    fileSize = -1;
                else
                    fileSize = inputFileStream.Length;
                for (int i = 0; i < 8; i++)
                    outputFileStream.WriteByte((byte)(fileSize >> (8 * i)));

                encoder.Code(inputFileStream, outputFileStream, -1, -1);
            }
        }
    }
}
