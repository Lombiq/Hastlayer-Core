using Hast.Layer;
using Hast.Samples.Compression.Services.Lzma;
using Hast.Transformer.SimpleMemory;
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

            var inputFileBytes = File.ReadAllBytes(inputFilePath);
            var inputFileSize = inputFileBytes.Length;
            var inputFileCellCount = inputFileSize / 4 + (inputFileSize % 4 == 0 ? 0 : 1);
            var outputFileSize = inputFileSize + 13;
            var outputFileCellCount = outputFileSize / 4 + (outputFileSize % 4 == 0 ? 0 : 1);
            var memorySize = inputFileCellCount + outputFileCellCount;
            var simpleMemory = new SimpleMemory(memorySize);
            var inputStream = new SimpleMemoryStream(simpleMemory, 0, inputFileCellCount);
            inputStream.Write(inputFileBytes, 0, inputFileSize);
            inputStream.Reset();
            var outputStream = new SimpleMemoryStream(simpleMemory, inputFileCellCount, outputFileCellCount);

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
            encoder.WriteCoderProperties(outputStream);

            var fileSize = Eos || StdInMode ? -1 : inputStream.Length;

            for (int i = 0; i < 8; i++)
            {
                outputStream.WriteByte((byte)(fileSize >> (8 * i)));
            }

            encoder.Code(inputStream, outputStream);

            outputFileSize = (int)outputStream.Position + 1;
            var fileBytes = new byte[outputFileSize];
            outputStream.Reset();
            outputStream.Read(fileBytes, 0, outputFileSize);

            //Array.Copy(outputFileStream.GetBytes(), fileBytes, outputFileStream.Position);
            File.WriteAllBytes(outputFilePath, fileBytes);
        }
    }
}
