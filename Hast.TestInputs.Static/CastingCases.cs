namespace Hast.TestInputs.Static
{
    public class CastingCases
    {
        public void NumberCasting(short a, short b)
        {
            short c = (short)(a * b);
            int d = a * b;
            var e = (byte)a;
            var f = (sbyte)a;
            var g = (ushort)a;
            var h = (uint)a;
            var i = (int)a;
            var x = h * i;
        }
    }
}
