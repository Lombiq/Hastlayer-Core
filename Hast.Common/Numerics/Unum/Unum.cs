namespace Hast.Common.Numerics.Unum
{
    public struct Unum// : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        #region Unum structure
        private byte _exponentSizeSize; // "esizesize"
        private byte _fractionSizeSize; // "fsizesize"

        private byte _exponentSizeMax; // "esizemax"
        private byte _fractionSizeMax; // "fsizemax"

        private byte _unumTagSize; // "utagsize"
        private byte _size; // "maxubits"
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
        private uint _uncertaintyBitMask; // "ubitmask"
        private uint _exponentSizeMask; // "esizemask"
        private uint _fractionSizeMask; // "fsizemask"
        private uint _exponentAndFractionSizeMask; // "efsizemask"
        private uint _unumTagMask; // "utagmask"
        #endregion

        #region Unum metadata
        // Bit masks should be at least of size "_unumTagSize + 1", instead of fixed 32 bits.
        // The precision where it could matter is ridiculously high though, so it shouldn't matter for now.

        private uint _uLP; // Unit in the Last Place or Unit of Least Precision.

        private uint _positiveInfinity; // "posinfu", the positive infinity for the given unum environment.
        private uint _negativeInfinity; // "neginfu", the negative infinity for the given unum environment.

        private uint _quietNotANumber; // "qNaNu"
        private uint _signalingNotANumber; // "sNaNu"

        private uint _largestPositive; // "maxrealu", the largest magnitude positive real number. One ULP less than infinity.
        private uint _smallestPositive; // "smallsubnormalu", the smallest magnitude positive real number. One ULP more than 0.

        private uint _largestNegative; // "negbigu", the largest maginude negative real number. One ULP more than negative infinity.
        private uint _minrealu; // "minrealu", looks like to be exactly the same as "negbigu".

        // uint is too small for a not particularly big unum, to be refactored.
        private uint _signBitOne; // "signbigu", a unum in which all bits are zero except the sign bit;

        //private uint _smallNormal; // "smallnormalu"
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
            _uncertaintyBitMask = ((uint)1 << (_unumTagSize - 1));
            _fractionSizeMask = (((uint)1 << _fractionSizeMax) - 1);
            _exponentSizeMask = (_uncertaintyBitMask - 1) - _fractionSizeMask;
            _exponentAndFractionSizeMask = _exponentSizeMask | _fractionSizeMask;
            _unumTagMask = _uncertaintyBitMask | _exponentAndFractionSizeMask;

            // Initializing metadata.
            _uLP = (uint)1 << _unumTagSize;

            _signBitOne = (uint)1 << _size - 1;

            _positiveInfinity = _signBitOne - 1 - _uncertaintyBitMask;
            _negativeInfinity = _positiveInfinity + _signBitOne;

            _largestPositive = _positiveInfinity - _uLP;
            _smallestPositive = _exponentAndFractionSizeMask + _uLP;

            _largestNegative = _negativeInfinity - _uLP;

            _minrealu = _largestPositive + _signBitOne;

            _quietNotANumber = _positiveInfinity + _uncertaintyBitMask;
            _signalingNotANumber = _negativeInfinity + _uncertaintyBitMask;

            //_smallNormal = _exponentAndFractionSizeMask + 1 << _size - 1 - _exponentSizeMax;
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
