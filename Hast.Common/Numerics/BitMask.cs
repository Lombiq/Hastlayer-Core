using System;

namespace Hast.Common.Numerics
{
    public struct BitMask
    {
        private const uint _32BitMSBMask = 0x80000000;

        public uint Size { get; private set; }
        public uint SegmentCount { get; private set; }
        public uint[] Segments { get; private set; }
        

        public BitMask(uint value)
        {
            Size = 32;
            SegmentCount = 1;
            Segments = new uint[SegmentCount];

            Segments[0] = value;
        }

        public BitMask(int value)
        {
            if (value < 0)
            {
                Size = 0;
                SegmentCount = 0;
                Segments = new uint[SegmentCount];
            } 
            else
            {
                Size = 1;
                SegmentCount = 1;
                Segments = new uint[SegmentCount];

                Segments[0] = (uint)value;
            }
        }

        public BitMask(uint size, bool allOne)
        {
            Size = size;
            SegmentCount = size == 0 ? 0 : (size >> 5) + 1;
            Segments = new uint[SegmentCount];

            if (allOne) for (int i = 0; i < SegmentCount; i++) Segments[i] = uint.MaxValue;
        }

        public BitMask(BitMask source)
        {
            Size = source.Size;
            SegmentCount = source.SegmentCount;
            Segments = new uint[SegmentCount];

            for (int i = 0; i < SegmentCount; i++) Segments[i] = source.Segments[i];
        }


        public static BitMask SetOne(BitMask mask, uint position)
        {
            if (position > mask.Size) return new BitMask(mask.Size, false);

            // Integer conversion doesn't matter, because we only care about the bits,
            // not the actual value represented, but it's needed for the bit shift to work.
            var bitPosition = (int)position & 31;
            var segmentPosition = position >> 5;

            mask.Segments[segmentPosition] = (uint)1 << bitPosition;

            return mask;
        }


        public static bool operator ==(BitMask left, BitMask right)
        {
            if (left == null ^ right == null) return false;
            if (left == null && right == null) return true;
            if (left.Size != right.Size) return false;

            for (int i = 0; i < left.SegmentCount; i++)
                if (left.Segments[i] != right.Segments[i]) return false;

            return true;
        }

        public static bool operator !=(BitMask left, BitMask right)
        {
            if (left == null ^ right == null) return true;
            if (left == null && right == null) return false;
            if (left.Size != right.Size) return true;

            for (int i = 0; i < left.SegmentCount; i++)
                if (left.Segments[i] != right.Segments[i]) return true;

            return false;
        }

        public static BitMask operator +(BitMask left, uint right)
        {
            if (left.SegmentCount == 0) return left;

            var mask = new BitMask(left);

            var msbBefore = GetSegmentMSB(mask.Segments[0]);
            mask.Segments[0] += right;
            var msbAfter = GetSegmentMSB(mask.Segments[0]);

            if (msbBefore && !msbAfter) // Carry from the first segment.
                for (int i = 1; i < mask.SegmentCount; i++)
                {
                    // Does the carry bubble up through this segment?
                    if (mask.Segments[i] == uint.MaxValue) mask.Segments[i] = 0;
                    else
                    {
                        mask.Segments[i]++;
                        break;
                    }
                }

            return mask;
        }

        public static BitMask operator -(BitMask left, uint right)
        {
            if (left.SegmentCount == 0) return left;

            var mask = new BitMask(left);

            var msbBefore = GetSegmentMSB(mask.Segments[0]);
            mask.Segments[0] -= right;
            var msbAfter = GetSegmentMSB(mask.Segments[0]);

            if (!msbBefore && msbAfter) // Carry from the first segment.
                for (int i = 1; i < mask.SegmentCount; i++)
                {
                    // Does the carry bubble up through this segment?
                    if (mask.Segments[i] == 0) mask.Segments[i] = uint.MaxValue;
                    else
                    {
                        mask.Segments[i]--;
                        break;
                    }
                }

            return mask;
        }

        public static BitMask operator +(BitMask left, BitMask right)
        {
            if (left.Size != right.Size) return new BitMask(left.Size, false);

            var mask = new BitMask(left);
            bool carry = false, msbBefore, msbAfter;

            for (int i = 0; i < mask.SegmentCount; i++)
            {
                msbBefore = GetSegmentMSB(mask.Segments[i]);

                if (carry) mask.Segments[i]++;
                mask.Segments[i] += right.Segments[i];
                
                msbAfter = GetSegmentMSB(mask.Segments[i]);
                carry = msbBefore && !msbAfter;
            }

            return mask;
        }

        public static BitMask operator -(BitMask left, BitMask right)
        {
            if (left.Size != right.Size) return new BitMask(left.Size, false);

            var mask = new BitMask(left);
            bool carry = false, msbBefore, msbAfter;

            for (int i = 0; i < mask.SegmentCount; i++)
            {
                msbBefore = GetSegmentMSB(mask.Segments[i]);

                if (carry) mask.Segments[i]++;
                mask.Segments[i] -= right.Segments[i];

                msbAfter = GetSegmentMSB(mask.Segments[i]);
                carry = !msbBefore && msbAfter;
            }

            return mask;
        }

        public static BitMask operator ++(BitMask mask)
        {
            return mask + 1;
        }

        public static BitMask operator --(BitMask mask)
        {
            return mask - 1;
        }

        public static BitMask operator |(BitMask left, BitMask right)
        {
            if (left.Size != right.Size) return new BitMask(left.Size, false);

            var mask = new BitMask(left);

            for (int i = 0; i < mask.SegmentCount; i++)
                mask.Segments[i] = left.Segments[i] | right.Segments[i];

            return mask;
        }


        private static bool GetSegmentMSB(uint segment)
        {
           return segment >> 31 == 1;
        }
    }
}
