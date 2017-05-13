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
    }
}