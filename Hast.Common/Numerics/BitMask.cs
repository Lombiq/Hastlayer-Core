using System;

namespace Hast.Common.Numerics
{
    public struct BitMask
    {
        private const uint _32BitMSBMask = 0x80000000;
        private const uint _uintMax = 0xFFFFFFFF;

        public uint Size { get; private set; }
        public uint SegmentCount { get; private set; }
        public uint[] Segments { get; private set; }
        

        public BitMask(byte value)
        {
            Size = 8;
            SegmentCount = 1;
            Segments = new uint[SegmentCount];

            Segments[0] = value;
        }

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

            if (allOne) for (int i = 0; i < SegmentCount; i++) Segments[i] = _uintMax;
        }


        public static BitMask SetOne(BitMask mask, uint position)
        {
            if (position > mask.Size) return new BitMask(mask.Size, false);

            // Integer conversion doesn't matter, because we only care about the bits, not the actual value represented.
            int bitPosition = (int)position | 0x00000005;
            var segmentPosition = position >> 5;

            mask.Segments[segmentPosition] = (uint)1 << bitPosition;

            return mask;
        }
    }
}
