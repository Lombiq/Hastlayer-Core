using Hast.Samples.SampleAssembly.Models;
using Hast.Samples.SampleAssembly.Services.Lzma;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    public static class LzmaCompressorDefaultParameters
    {
        public const int LiteralContextBits = 3; // Set it to 0 for 32-bit data.
        public const bool MatchFinderIsBt4 = true;
        public const int DictionarySize = 1 << 7;
        public const int PositionStateBits = 2;
        public const int LiteralPositionBits = 0; // Set it to 2 for 32-bit data.
        public const int Algorithm = 2;
        public const int NumberOfFastBytes = 128;
        public const bool StdInMode = false;
        public const bool Eos = false;
    }


    public class LzmaCompressor
    {
        public const int LzmaCompressor_InputStartCellIndex = 0;
        public const int LzmaCompressor_InputByteCountIndex = 1;
        public const int LzmaCompressor_OutputStartCellIndex = 2;
        public const int LzmaCompressor_OutputByteCountIndex = 3;
        public const int LzmaCompressor_LiteralContextBitsIndex = 4;
        public const int LzmaCompressor_MatchFinderIsBt4Index = 5;
        public const int LzmaCompressor_DictionarySizeIndex = 6;
        public const int LzmaCompressor_PositionStateBitsIndex = 7;
        public const int LzmaCompressor_LiteralPositionBitsIndex = 8;
        public const int LzmaCompressor_AlgorithmIndex = 9;
        public const int LzmaCompressor_NumberOfFastBytesIndex = 10;
        public const int LzmaCompressor_StdInModeIndex = 11;
        public const int LzmaCompressor_EosIndex = 12;
        

        public virtual void Compress(SimpleMemory memory)
        {
            int inputStartCellIndex = memory.ReadInt32(LzmaCompressor_InputStartCellIndex);
            int inputByteCount = memory.ReadInt32(LzmaCompressor_InputByteCountIndex);
            int outputStartCellIndex = memory.ReadInt32(LzmaCompressor_OutputStartCellIndex);
            int outputByteCount = memory.ReadInt32(LzmaCompressor_OutputByteCountIndex);
            int literalContextBits = memory.ReadInt32(LzmaCompressor_LiteralContextBitsIndex);
            bool matchFinder = memory.ReadBoolean(LzmaCompressor_MatchFinderIsBt4Index);
            int dictionarySize = memory.ReadInt32(LzmaCompressor_DictionarySizeIndex);
            int positionStateBits = memory.ReadInt32(LzmaCompressor_PositionStateBitsIndex);
            int literalPositionBits = memory.ReadInt32(LzmaCompressor_LiteralPositionBitsIndex);
            int algorithm = memory.ReadInt32(LzmaCompressor_AlgorithmIndex);
            int numberOfFastBytes = memory.ReadInt32(LzmaCompressor_NumberOfFastBytesIndex);
            bool stdInMode = memory.ReadBoolean(LzmaCompressor_StdInModeIndex);
            bool eos = memory.ReadBoolean(LzmaCompressor_EosIndex);

            var inputStream = new SimpleMemoryStream(memory, inputStartCellIndex, inputByteCount);
            var outputStream = new SimpleMemoryStream(memory, outputStartCellIndex, outputByteCount);

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
                dictionarySize,
                positionStateBits,
                literalContextBits,
                literalPositionBits,
                algorithm,
                numberOfFastBytes,
                matchFinder,
                eos
            };

            var encoder = new LzmaEncoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outputStream);

            var fileSize = eos || stdInMode ? -1 : inputByteCount;

            for (int i = 0; i < 8; i++)
            {
                var b = (byte)((long)fileSize >> (8 * i));
                outputStream.WriteByte(b);
            }

            encoder.Code(inputStream, outputStream);

            memory.WriteInt32(LzmaCompressor_OutputByteCountIndex, (int)outputStream.Position);
        }
    }


    public static class LzmaCompressorExtensions
    {
        public static byte[] CompressBytes(this LzmaCompressor lzmaCompressor, byte[] inputBytes)
        {
            const int InitCellCount = 13;
            const int LzmaInitCellCount = 13;
            const int OutputExtraBytes = 10;

            var inputSize = inputBytes.Length;
            var inputCellCount = inputSize / 4 + (inputSize % 4 == 0 ? 0 : 1);
            var inputStartCell = InitCellCount;
            var outputSize = LzmaInitCellCount + inputSize + OutputExtraBytes;
            var outputCellCount = outputSize / 4 + (outputSize % 4 == 0 ? 0 : 1);
            var outputStartCell = InitCellCount + inputCellCount;
            
            var memorySize = InitCellCount + inputCellCount + outputCellCount;
            var simpleMemory = new SimpleMemory(memorySize);

            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_InputStartCellIndex, inputStartCell);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_InputByteCountIndex, inputSize);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_OutputStartCellIndex, outputStartCell);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_OutputByteCountIndex, outputSize);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_LiteralContextBitsIndex, LzmaCompressorDefaultParameters.LiteralContextBits);
            simpleMemory.WriteBoolean(LzmaCompressor.LzmaCompressor_MatchFinderIsBt4Index, LzmaCompressorDefaultParameters.MatchFinderIsBt4);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_DictionarySizeIndex, LzmaCompressorDefaultParameters.DictionarySize);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_PositionStateBitsIndex, LzmaCompressorDefaultParameters.PositionStateBits);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_LiteralPositionBitsIndex, LzmaCompressorDefaultParameters.LiteralPositionBits);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_AlgorithmIndex, LzmaCompressorDefaultParameters.Algorithm);
            simpleMemory.WriteInt32(LzmaCompressor.LzmaCompressor_NumberOfFastBytesIndex, LzmaCompressorDefaultParameters.NumberOfFastBytes);
            simpleMemory.WriteBoolean(LzmaCompressor.LzmaCompressor_StdInModeIndex, LzmaCompressorDefaultParameters.StdInMode);
            simpleMemory.WriteBoolean(LzmaCompressor.LzmaCompressor_EosIndex, LzmaCompressorDefaultParameters.Eos);

            var inputStream = new SimpleMemoryStream(simpleMemory, inputStartCell, inputSize);
            inputStream.Write(inputBytes, 0, inputSize);

            lzmaCompressor.Compress(simpleMemory);

            var outputStream = new SimpleMemoryStream(simpleMemory, outputStartCell, outputSize);
            outputSize = simpleMemory.ReadInt32(LzmaCompressor.LzmaCompressor_OutputByteCountIndex);
            var outputBytes = new byte[outputSize];
            outputStream.Read(outputBytes, 0, outputSize);

            return outputBytes;
        }
    }
}
