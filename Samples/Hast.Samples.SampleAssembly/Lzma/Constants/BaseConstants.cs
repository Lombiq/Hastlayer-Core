namespace Hast.Samples.SampleAssembly.Services.Lzma.Constants
{
	internal static class BaseConstants
	{
		public const uint NumRepDistances = 4;
		public const uint NumStates = 12;
        public const int NumPosSlotBits = 6;
        public const int DicLogSizeMin = 0;
        public const int NumLenToPosStatesBits = 2; // it's for speed optimization
        public const uint NumLenToPosStates = 1 << NumLenToPosStatesBits;
        public const uint MatchMinLen = 2;
        public const int NumAlignBits = 4;
        public const uint AlignTableSize = 1 << NumAlignBits;
        public const uint AlignMask = (AlignTableSize - 1);
        public const uint StartPosModelIndex = 4;
        public const uint EndPosModelIndex = 14;
        public const uint NumPosModels = EndPosModelIndex - StartPosModelIndex;
        public const uint NumFullDistances = 1 << ((int)EndPosModelIndex / 2);
        public const uint NumLitPosStatesBitsEncodingMax = 4;
        public const uint NumLitContextBitsMax = 8;
        public const int NumPosStatesBitsMax = 4;
        public const uint NumPosStatesMax = (1 << NumPosStatesBitsMax);
        public const int NumPosStatesBitsEncodingMax = 4;
        public const uint NumPosStatesEncodingMax = (1 << NumPosStatesBitsEncodingMax);
        public const int NumLowLenBits = 3;
        public const int NumMidLenBits = 3;
        public const int NumHighLenBits = 8;
        public const uint NumLowLenSymbols = 1 << NumLowLenBits;
        public const uint NumMidLenSymbols = 1 << NumMidLenBits;
        public const uint NumLenSymbols = NumLowLenSymbols + NumMidLenSymbols + (1 << NumHighLenBits);
        public const uint MatchMaxLen = MatchMinLen + NumLenSymbols - 1;
        

		public static uint GetLenToPosState(uint len)
		{
			len -= MatchMinLen;

			if (len < NumLenToPosStates) return len;

			return NumLenToPosStates - 1;
        }
    }
}
