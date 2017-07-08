namespace Lombiq.Unum
{
    public class UnumConfiguration
    {
        /// <summary>
        /// The number of bits in the exponent.
        /// </summary>
        public readonly byte ExponentSize;

        /// <summary>
        /// The number of bits in the fraction.
        /// </summary>
        public readonly byte FractionSize;


        public UnumConfiguration(IeeeConfiguration configuration)
        {
            switch (configuration)
            {
                case IeeeConfiguration.HalfPrecision:
                    ExponentSize = 5;
                    FractionSize = 10;
                    break;
                case IeeeConfiguration.SinglePrecision:
                    ExponentSize = 8;
                    FractionSize = 23;
                    break;
                case IeeeConfiguration.DoublePrecision:
                    ExponentSize = 11;
                    FractionSize = 52;
                    break;
                case IeeeConfiguration.ExtendedPrecision:
                    ExponentSize = 15;
                    FractionSize = 64;
                    break;
                case IeeeConfiguration.QuadPrecision:
                    ExponentSize = 15;
                    FractionSize = 112;
                    break;
            }
        }

        public UnumConfiguration(byte exponentSize, byte fractionSize)
        {
            ExponentSize = exponentSize;
            FractionSize = fractionSize;
        }
    }

    public enum IeeeConfiguration
    {
        HalfPrecision,     // 16-bit.
        SinglePrecision,   // 32-bit.
        DoublePrecision,   // 64-bit.
        ExtendedPrecision, // 80-bit (Intel x87).
        QuadPrecision      // 128-bit.
    }
}