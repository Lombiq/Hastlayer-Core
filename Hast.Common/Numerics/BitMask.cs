using System;

namespace Hast.Common.Numerics
{
    public struct BitMask
    {
        public uint Size { get; private set; }
        public uint SegmentCount { get; private set; }
        public uint[] Segments { get; private set; }


        public BitMask(uint[] segments)
        {
            SegmentCount = (uint)segments.Length;
            Size = SegmentCount << 5;
            Segments = new uint[SegmentCount];
            Array.Copy(segments, Segments, SegmentCount);
        }

        public BitMask(uint[] segments, uint size = 0)
        {
            SegmentCount = (uint)segments.Length;
            Size = size;
            if (size > SegmentCount << 5)
            {

                SegmentCount = (size >> 5) + (size % 32 == 0 ? 0 : (uint)1);
            }
            Segments = new uint[SegmentCount];

            Array.Copy(segments, Segments, segments.Length);
            for (int i = 0; i < SegmentCount - segments.Length; i++)
                Array.Copy(new uint[] { 0 }, 0, Segments, segments.Length + i, 1);
        }

        public BitMask(uint size, bool allOne)
        {
            SegmentCount = (size >> 5) + (size % 32 == 0 ? 0 : (uint)1);
            Size = size;
            Segments = new uint[SegmentCount];

            if (allOne) for (int i = 0; i < SegmentCount; i++) Segments[i] = uint.MaxValue;
        }

        public BitMask(BitMask source)
        {
            Size = source.Size;
            SegmentCount = source.SegmentCount;
            Segments = new uint[SegmentCount];

            if (source.Segments != null) Array.Copy(source.Segments, Segments, SegmentCount);
        }


        public static BitMask SetOne(BitMask mask, uint index)
        {
            if (index > mask.Size) return mask;

            // Integer conversion doesn't matter, because we only care about the bits,
            // not the actual value represented, but it's needed for the bit shift to work.
            var bitPosition = index % 32;
            var segmentPosition = index >> 5;

            if ((mask.Segments[segmentPosition] >> (int)bitPosition) % 2 == 0)
                mask.Segments[segmentPosition] += (uint)1 << (int)bitPosition;

            return mask;
        }


        public static bool operator ==(BitMask left, BitMask right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            if (left.Size != right.Size) return false;

            for (int i = 0; i < left.SegmentCount; i++)
                if (left.Segments[i] != right.Segments[i]) return false;

            return true;
        }

        public static bool operator !=(BitMask left, BitMask right) => !(left == right);

        public static BitMask operator +(BitMask left, uint right) => left + new BitMask(new uint[] { right }, left.Size);

        public static BitMask operator -(BitMask left, uint right) => left - new BitMask(new uint[] { right }, left.Size);

        /// <summary>
        /// Bit-by-bit addition of two masks with "ripple-carry".
        /// </summary>
        /// <param name="left">Left operand BitMask.</param>
        /// <param name="right">Right operand BitMask.</param>
        /// <returns>The sum of the masks with respect to the size of the left operand.</returns>
        public static BitMask operator +(BitMask left, BitMask right)
        {
            if (left.SegmentCount == 0 || right.SegmentCount == 0) return left;

            var mask = new BitMask(left.Size, false);
            bool carry = false, leftBit, rightBit;
            int buffer, segment = 0, position = 0;

            for (int i = 0; i < mask.Size; i++)
            {
                leftBit = (left.Segments[segment] >> position) % 2 == 1;
                rightBit = i >= right.Size ? false : (right.Segments[segment] >> position) % 2 == 1;

                buffer = (leftBit ? 1 : 0) + (rightBit ? 1 : 0) + (carry ? 1 : 0);

                if (buffer % 2 == 1) mask.Segments[segment] += (uint)(1 << position);
                carry = buffer >> 1 == 1;

                position++;
                if (position >> 5 == 1)
                {
                    position = 0;
                    segment++;
                }
            }

            return mask;
        }

        /// <summary>
        /// Bit-by-bit subtraction of two masks with "ripple-carry".
        /// </summary>
        /// <param name="left">Left operand BitMask.</param>
        /// <param name="right">Right operand BitMask.</param>
        /// <returns>The difference between the masks with respect to the size of the left operand.</returns>
        public static BitMask operator -(BitMask left, BitMask right)
        {
            if (left.SegmentCount == 0 || right.SegmentCount == 0) return left;

            var mask = new BitMask(left.Size, false);
            bool carry = false, leftBit, rightBit;
            int buffer, segment = 0, position = 0;

            for (int i = 0; i < mask.Size; i++)
            {
                leftBit = (left.Segments[segment] >> position) % 2 == 1;
                rightBit = i >= right.Size ? false : (right.Segments[segment] >> position) % 2 == 1;

                buffer = 2 + (leftBit ? 1 : 0) - (rightBit ? 1 : 0) - (carry ? 1 : 0);

                if (buffer % 2 == 1) mask.Segments[segment] += (uint)(1 << position);
                carry = buffer >> 1 == 0;

                position++;
                if (position >> 5 == 1)
                {
                    position = 0;
                    segment++;
                }
            }

            return mask;
        }

        public static BitMask operator ++(BitMask mask) => mask + 1;

        public static BitMask operator --(BitMask mask) => mask - 1;

        public static BitMask operator |(BitMask left, BitMask right)
        {
            if (left.Size != right.Size) return new BitMask(left.Size, false);

            var mask = new BitMask(left);

            for (int i = 0; i < mask.SegmentCount; i++)
                mask.Segments[i] = left.Segments[i] | right.Segments[i];

            return mask;
        }

        public static BitMask operator &(BitMask left, BitMask right)
        {
            if (left.Size != right.Size) return new BitMask(left.Size, false);

            var mask = new BitMask(left);

            for (int i = 0; i < mask.SegmentCount; i++)
                mask.Segments[i] = left.Segments[i] & right.Segments[i];

            return mask;
        }

        /// <summary>
        /// Bitshifting of a BitMask to the right by an integer. Shifts left if negative value is given.
        /// </summary>
        /// <param name="left">Left operand BitMask to shift.</param>
        /// <param name="right">Right operand int tells how many bits to shift by.</param>
        /// <returns>BitMask of size of left BitMask, shifted left by number of bits given in the right integer. </returns>
        public static BitMask operator >>(BitMask left, int right)
        {
            if (right < 0) return left << -right;

            bool carryOld, carryNew;
            uint segmentMaskWithLeadingOne = 0x80000000; //1000 0000 0000 0000 0000 0000 0000 0000 

            for (int i = 0; i < right; i++)
            {

                carryOld = false;


                for (int j = 0; j < left.SegmentCount; j++)
                {
                    carryNew = left.Segments[j] % 2 == 1;
                    left.Segments[j] = (left.Segments[j] >> 1);
                    if (carryOld) left.Segments[j] = left.Segments[j] | segmentMaskWithLeadingOne;
                    carryOld = carryNew;


                }
            }
            return left;
        }

        /// <summary>
        /// Bitshifting of a BitMask to the left by an integer. Shifts right if negative value is given.
        /// </summary>
        /// <param name="left">Left operand BitMask.</param>
        /// <param name="right">Right operand int tells how many bits to shift by.</param>
        /// <returns>BitMask of size of left BitMask, shifted right by number of bits given in the right integer. </returns>
        public static BitMask operator <<(BitMask left, int right)
        {
            if (right < 0) return left >> -right;

            bool carryOld, carryNew;
            uint segmentMaskWithLeadingOne = 0x80000000; //1000 0000 0000 0000 0000 0000 0000 0000 
            uint segmentMaskWithClosingOne = 1;          //0000 0000 0000 0000 0000 0000 0000 0001 

            for (int i = 0; i < right; i++)
            {

                carryOld = false;


                for (int j = (int)left.SegmentCount - 1; j >= 0; j--)
                {
                    carryNew = ((left.Segments[j] & segmentMaskWithLeadingOne) == segmentMaskWithLeadingOne);
                    left.Segments[j] = (left.Segments[j] << 1);
                    if (carryOld) left.Segments[j] = left.Segments[j] | segmentMaskWithClosingOne;
                    carryOld = carryNew;


                }
            }
            return left;


        }


        public uint getLowest32Bits()
        {
            return Segments[0];
        }


        public override bool Equals(object obj) => this == (BitMask)obj;

        public override int GetHashCode()
        {
            var segmentHashCodes = new int[SegmentCount];
            for (int i = 0; i < SegmentCount; i++) segmentHashCodes[i] = Segments[i].GetHashCode();

            return segmentHashCodes.GetHashCode();
        }

        public override string ToString() => string.Join(", ", Segments);
    }
}
