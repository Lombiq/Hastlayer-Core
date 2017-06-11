namespace Hast.Algorithms
{
    public class MWC64X
    {
        ulong state = 7215152093156152310UL; //random seed

        public uint GetNextRandom()
        {
            uint c = (uint)(state >> 32);
            uint x = (uint)(state & 0xFFFFFFFFUL);
            state = x * ((ulong)4294883355UL) + c;
            return x ^ c;
        }

        ulong randomState1 = 7215152093156152310UL; //random seed
        public uint GetNextRandom1()
        {
            unchecked
            {
                uint c = (uint)(this.randomState1 >> 32);
                ulong xl = this.randomState1 & (ulong)-1;
                uint x = (uint)xl;
                this.randomState1 = (ulong)x * (ulong)-83941 + (ulong)c;
                return x ^ c;
            }
        }
    }
}