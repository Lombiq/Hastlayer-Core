using Hast.Layer;
using Hast.Samples.Compression.Services.Lzma;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Hast.Samples.Compression.Services
{
    public static class LzmaCompressionService
    {
        public const int LiteralContextBits = 3; // Set it to 0 for 32-bit data.
        public const string MatchFinder = "bt4";
        public const int DictionarySize = 1 << 23;
        public const int PositionStateBits = 2;
        public const int LiteralPositionBits = 0; // Set it to 2 for 32-bit data.
        public const int Algorithm = 2;
        public const int NumberOfFastBytes = 128;
        public const bool StdInMode = false;
        public const bool Eos = false;


        public static void CompressFile(string inputFilePath, string outputFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);

            if (!inputFile.Exists) throw new HastlayerException("Input file doesn't exist.");

            outputFilePath = string.IsNullOrEmpty(outputFilePath) ? inputFile.Name + ".lzma" : outputFilePath;
            if (File.Exists(outputFilePath)) File.Delete(outputFilePath);

            //using (var inputFileStream = inputFile.OpenRead())
            var inputFileStream = new HastlayerStream(File.ReadAllBytes(inputFilePath));
            //using (var outputFileStream = File.Create(outputFilePath))
            var outputFileStream = new HastlayerStream(new byte[inputFileStream.Length]);
            {
                CoderPropertyId[] propIDs =
                {
                    CoderPropertyId.DictionarySize,
                    CoderPropertyId.PosStateBits,
                    CoderPropertyId.LitContextBits,
                    CoderPropertyId.LitPosBits,
                    CoderPropertyId.Algorithm,
                    CoderPropertyId.NumFastbytes,
                    CoderPropertyId.MatchFinder,
                    CoderPropertyId.EndMarker
                };
                object[] properties =
                {
                    DictionarySize,
                    PositionStateBits,
                    LiteralContextBits,
                    LiteralPositionBits,
                    Algorithm,
                    NumberOfFastBytes,
                    MatchFinder,
                    Eos
                };

                var encoder = new LzmaEncoder();
                encoder.SetCoderProperties(propIDs, properties);
                encoder.WriteCoderProperties(outputFileStream);

                var fileSize = Eos || StdInMode ? -1 : inputFileStream.Length;

                for (int i = 0; i < 8; i++)
                {
                    outputFileStream.WriteByte((byte)(fileSize >> (8 * i)));
                }

                encoder.Code(inputFileStream, outputFileStream);

                var fileBytes = new byte[outputFileStream.Position];

                Array.Copy(outputFileStream.GetBytes(), fileBytes, outputFileStream.Position);
                File.WriteAllBytes(outputFilePath, fileBytes);
            }
        }
    }
}
