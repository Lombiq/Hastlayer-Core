namespace Hast.Common.Numerics.Unum
{
    public class UnumConfiguration
    {
        /// <summary>
        /// The number of bits describing the maximum amount of bits describing of the exponent in the Unum.
        /// </summary>
        /// <example>
        /// Example 1: ESizeSize = 0 --> Number of bits of ESize = 0 --> Exponent size = 1 (there's always at least 1 bit).
        /// Example 2: ESizeSize = 3 --> Number of bits of ESize is in [0-3] --> Exponent size is in [1, 9].
        /// </example>
        public byte ESizeSize { get; set; }

        /// <summary>
        /// The number of bits describing the maximum amount of bits describing of the fraction in the Unum.
        /// </summary>
        /// <example>
        /// Example 1: FSizeSize = 0 --> Number of bits of FSize = 0 --> Fraction size = 1 (there's always at least 1 bit).
        /// Example 2: FSizeSize = 4 --> Number of bits of FSize is in [0-4] --> Fraction size is in [1, 17].
        /// </example>
        public byte FSizeSize { get; set; }
    }
}