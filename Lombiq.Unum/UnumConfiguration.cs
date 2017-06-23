namespace Lombiq.Unum
{
    public class UnumConfiguration
    {
        /// <summary>
        /// The number of bits describing the maximum amount of bits describing of the exponent in the Unum.
        /// ExponentSizeMax = 2 to the power of ExponentSizeSize;
        /// </summary>
        /// <example>
        /// Example 1: ExponentSizeSize = 0 --> Number of bits of ESize = 0 --> Exponent size = 1.
        /// Example 2: ExponentSizeSize = 3 --> Number of bits of ESize is in [0-3] --> Exponent size is in [1, 8].
        /// </example>
        public byte ExponentSizeSize { get; set; }

        /// <summary>
        /// The number of bits describing the maximum amount of bits describing of the fraction in the Unum.
        /// FractionSizeMax = 2 to the power of FractionSizeSize;
        /// </summary>
        /// <example>
        /// Example 1: FractionSizeSize = 0 --> Number of bits of FSize = 0 --> Fraction size = 1.
        /// Example 2: FractionSizeSize = 4 --> Number of bits of FSize is in [0-4] --> Fraction size is in [1, 16].
        /// </example>
        public byte FractionSizeSize { get; set; }
    }
}