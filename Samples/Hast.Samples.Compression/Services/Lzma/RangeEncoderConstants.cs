namespace Hast.Samples.Compression.Services.Lzma
{
    internal static class RangeEncoderConstants
    {
        public const uint KTopValue = (1 << 24);
        public const int KNumBitModelTotalBits = 11;
        public const uint KBitModelTotal = (1 << KNumBitModelTotalBits);
        public const int KNumBitPriceShiftBits = 6;
    }
}
