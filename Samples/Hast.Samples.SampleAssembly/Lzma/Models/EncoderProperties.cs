using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly.Lzma.Models
{
    public enum MatchFinder
    {
        BT2,
        BT4,
    };


    public class EncoderProperties
    {
        /*
         *            int literalContextBits = memory.ReadInt32(LzmaCompressor_LiteralContextBitsIndex);
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
         */

        public int LiteralContextBits { get; set; }
        public uint DictionarySize { get; set; }
        public int PositionStateBits { get; set; }
        public int LiteralPositionBits { get; set; }
        public int Algorithm { get; set; }
        public uint NumberOfFastBytes { get; set; }
        public bool WriteEndMarker { get; set; }
        public MatchFinder MatchFinder { get; set; }
    }
}
