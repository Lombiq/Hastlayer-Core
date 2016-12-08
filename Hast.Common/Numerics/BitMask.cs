namespace Hast.Common.Numerics
{
    public struct BitMask
    {
        private const uint _32BitMSB1Mask = 0x8000;

        public uint Size { get; private set; }
        public uint SegmentCount { get; private set; }
        public uint[] Bits { get; private set; }


        //public BitMask(uint size)
        //{
        //    Size = size;

        //    SegmentCount = Size / 32;
        //    SegmentCount = SegmentCount * 32 < Size ? ++SegmentCount : SegmentCount;

        //    Bits = new uint[SegmentCount];
        //}
        
        public BitMask(byte value)
        {
            Size = 8;
            SegmentCount = 1;
            Bits = new uint[1];

            Bits[0] = value;
        }

        public BitMask(uint value)
        {
            Size = 32;
            SegmentCount = 1;
            Bits = new uint[1];

            Bits[0] = value;
        }

        public BitMask(int value)
        {
            if (value < 0)
            {
                Size = 0;
                SegmentCount = 0;
                Bits = new uint[0];
            }
            else
            {
                Size = 1;
                SegmentCount = 1;
                Bits = new uint[1];

                Bits[0] = (uint)value;
            }
        }


        //public static BitMask operator <<(BitMask value, int shift) { }
    }
}
