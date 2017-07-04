using Hast.Layer;
using Hast.Samples.Compression.Services.Lzma;
using Hast.Transformer.SimpleMemory;
using System.IO;

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
            var outputFileSize = inputFileSize + 23;
            var outputFileCellCount = outputFileSize / 4 + (outputFileSize % 4 == 0 ? 0 : 1);
            var memorySize = inputFileCellCount + outputFileCellCount;
            var simpleMemory = new SimpleMemory(memorySize);
            var inputStream = new SimpleMemoryStream(simpleMemory, 0, inputFileSize);
            inputStream.Write(inputFileBytes, 0, inputFileSize);
            inputStream.ResetPosition();
            var outputStream = new SimpleMemoryStream(simpleMemory, inputFileCellCount, outputFileSize);

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

            var fileSize = Eos || StdInMode ? -1 : inputFileSize;

            for (int i = 0; i < 8; i++)
            {
                var b = (byte)((long)fileSize >> (8 * i));
                outputStream.WriteByte(b);
            }

            encoder.Code(inputStream, outputStream);

            outputFileSize = (int)outputStream.Position;
            var fileBytes = new byte[outputFileSize];
            outputStream.ResetPosition();
            outputStream.Read(fileBytes, 0, outputFileSize);
            
            File.WriteAllBytes(outputFilePath, fileBytes);
        }
    }
}
