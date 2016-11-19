using System.Linq;

namespace Hast.Common.Numerics.Unum
{
    public struct Unum// : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        #region Unum structure
        private byte _exponentSizeSize;
        private byte _fractionSizeSize;

        private byte _exponentSizeMax;
        private byte _fractionSizeMax;

        private byte _unumTagSize;
        private byte _size;
        #endregion

        #region Unum blocks
        private bool _signBit;
        private bool[] _exponentBits;
        private bool[] _fractionBits;

        private bool _uncertaintyBit;
        private bool[] _exponentSizeBits;
        private bool[] _fractionSizeBits;
        #endregion

        #region Unum masks
        private int _uncertaintyBitMask;
        private int _exponentSizeMask;
        private int _fractionSizeMask;
        private int _exponentAndFractionSizeMask;
        private int _unumTagMask;
        #endregion

        #region Unum metadata
        private int _uLP; // Unit in the Last Place or Unit of Least Precision.

        //private int _positiveInfinityUnum;
        //private int _negativeInfinityUnum;

        //private int _quietNotANumberUnum;
        //private int _signalingNotANumberUnum;

        //private int _maxRealUnum; // Largest representable positive number. One ULP less than infinity.
        //private int _smallSubNormalUnum; // Smallest representable positive number. One ULP more than 0.

        //private int _negativeBiggestUnum; // Smallest representable negative number. One ULP more than negative infinity.

        #endregion


        public Unum(byte exponentSizeSize, byte fractionSizeSize)
        {
            // Initializing structure.
            _exponentSizeSize = exponentSizeSize;
            _fractionSizeSize = fractionSizeSize;

            _exponentSizeMax = (byte)(1 << _exponentSizeSize);
            _fractionSizeMax = (byte)(1 << _fractionSizeSize);

            _unumTagSize = (byte)(1 + _exponentSizeSize + _fractionSizeSize);
            _size = (byte)(1 + _exponentSizeMax + _fractionSizeMax + _unumTagSize);

            // Initializing blocks.
            _signBit = false;
            _uncertaintyBit = false;

            _exponentBits = new bool[_exponentSizeMax];
            _fractionBits = new bool[_fractionSizeMax];

            _exponentSizeBits = new bool[_exponentSizeSize];
            _fractionSizeBits = new bool[_fractionSizeSize];

            // Initializing masks.
            _uncertaintyBitMask = 1 << (_unumTagSize - 1);
            _fractionSizeMask = (1 << _fractionSizeMax) - 1;
            _exponentSizeMask = (_uncertaintyBitMask - 1) - _fractionSizeMask;
            _exponentAndFractionSizeMask = _exponentSizeMask | _fractionSizeMask;
            _unumTagMask = _uncertaintyBitMask | _exponentAndFractionSizeMask;

            // Initializing metadata.
            _uLP = 1 << _unumTagSize;

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
