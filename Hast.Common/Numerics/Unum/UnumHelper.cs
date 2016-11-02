namespace Hast.Common.Numerics.Unum
{
    public static class UnumHelper
    {
        public static byte SegmentSizeToSegmentSizeSize(byte segmentSize)
        {
            if (segmentSize == 0) throw new UnumException("Segment size must be greater than zero.");

            byte firstSignificantBit = 0;
            while (firstSignificantBit < 7 && (segmentSize & (1 << firstSignificantBit)) != 0) firstSignificantBit++;

            return firstSignificantBit;
        }

        /// <summary>
        /// Calculates whether a Unum with the given configuration of exponent and fraction size can fit
        /// into the given number of bits.
        /// </summary>
        /// <param name="eSize">The size of the exponent.</param>
        /// <param name="fSize">The size of the fraction.</param>
        /// <param name="maximumSize">The maximum size allowed for the Unum.</param>
        /// <returns>The number of bits required to store the Unum with the given configuration
        /// if it fits into the given maximum, 0 otherwise.</returns>
        public static byte UnumConfigurationFitsNBits(byte eSize, byte fSize, byte maximumSize)
        {
            // One of the segments can be 24 bits long if the other one is only 1 bit.
            if (eSize >= maximumSize || fSize >= maximumSize || eSize > 24 || fSize > 24) return 0;

            var size = MaximumUnumBits(eSize, fSize);

            // Sign bit + exponent size + fraction size + uncertainty bit + exponent size size + fraction size size.
            return size > maximumSize ? (byte)0 : size;
        }

        public static byte MaximumUnumBits(byte eSize, byte fSize) =>
            (byte)(1 + eSize + fSize + 1 + SegmentSizeToSegmentSizeSize(eSize) + SegmentSizeToSegmentSizeSize(fSize));
    }
}
