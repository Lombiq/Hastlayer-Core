using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Numerics.Unum
{
    public struct Unum// : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        #region Unum structure
        private bool _signBit;
        private bool[] _exponentBits;
        private bool[] _fractionBits;
        private bool _uBit;
        private bool[] _exponentSizeBits;
        private bool[] _fractionSizeBits;
        #endregion

        private byte _size;
        private byte _uTagSize;
        private bool[] _uTag => new[] { _uBit }.Union(_exponentSizeBits).Union(_fractionSizeBits).ToArray();


        public Unum(byte eSize, byte fSize)
        {
            var eSizeSize = UnumHelper.SegmentSizeToSegmentSizeSize(eSize);
            var fSizeSize = UnumHelper.SegmentSizeToSegmentSizeSize(fSize);

            _signBit = false;
            _exponentBits = new bool[eSize];
            _fractionBits = new bool[fSize];
            _uBit = false;
            _exponentSizeBits = new bool[eSizeSize];
            _fractionSizeBits = new bool[fSizeSize];

            _uTagSize = (byte)(1 + eSizeSize + fSizeSize);
            _size = (byte)(1 + eSizeSize + fSizeSize + _uTagSize);
        }



        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }


        public static Unum operator +(Unum left, Unum right)
        {
            return new Unum();
        }

        //public static Unum operator -(Unum left, Unum right)
        //{

        //}

        //public static Unum operator *(Unum left, Unum right)
        //{

        //}

        //public static Unum operator /(Unum left, Unum right)
        //{

        //}

        //public static bool operator ==(Unum left, Unum right)
        //{

        //}

        //public static bool operator !=(Unum left, Unum right)
        //{

        //}

        //public static bool operator <(Unum left, Unum right)
        //{

        //}

        //public static bool operator >(Unum left, Unum right)
        //{

        //}

        //public static bool operator <=(Unum left, Unum right)
        //{

        //}

        //public static bool operator >=(Unum left, Unum right)
        //{

        //}

        //public static implicit operator Unum(short x)
        //{
        //}

        //public static implicit operator Unum(ushort x)
        //{
        //}

        // Since there is no loss of information when converting from an int to an Unum we allow it implicitly.
        public static implicit operator Unum(int x)
        {
            return new Unum();
        }

        //public static implicit operator Unum(uint x)
        //{
        //}

        //public static implicit operator Unum(float x)
        //{
        //}

        //public static implicit operator Unum(double x)
        //{
        //}

        //public static implicit operator Unum(long x)
        //{
        //}

        //public static implicit operator Unum(ulong x)
        //{
        //}

        //public static implicit operator Unum(decimal x)
        //{
        //}

        // Converting from an Unum to int results in information loss, so only allowing it explicitly (with a cast).
        public static explicit operator int(Unum x)
        {
            return 1;
        }
    }
}
