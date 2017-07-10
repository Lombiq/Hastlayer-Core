namespace Hast.Samples.SampleAssembly.Lzma.Constants
{
    internal static class RangeEncoderConstants
    {
        public const uint TopValue = (1 << 24);
        public const int NumBitModelTotalBits = 11;
        public const uint BitModelTotal = (1 << NumBitModelTotalBits);
        public const int NumBitPriceShiftBits = 6;
    }
}
