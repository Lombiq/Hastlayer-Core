﻿namespace Hast.Common.Numerics.Unum
{
    public struct Unum// : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        private UnumMetadata _metadata;

        #region Unum structure
        public byte ExponentSizeSize => _metadata.ExponentSizeSize; // "esizesize"
        public byte FractionSizeSize => _metadata.FractionSizeSize; // "fsizesize"

        public byte ExponentSizeMax => _metadata.ExponentSizeMax; // "esizemax"
        public byte FractionSizeMax => _metadata.FractionSizeMax; // "fsizemax"

        public byte UnumTagSize => _metadata.UnumTagSize; // "utagsize"
        public byte Size => _metadata.Size; // "maxubits"
        #endregion

        #region Unum blocks
        public bool SignBit { get; private set; }
        public bool[] ExponentBits { get; private set; }
        public bool[] FractionBits { get; private set; }

        public bool UncertaintyBit { get; private set; }
        public bool[] ExponentSizeBits { get; private set; }
        public bool[] FractionSizeBits { get; private set; }
        #endregion

        #region Unum masks
        public BitMask UncertaintyBitMask => _metadata.UncertaintyBitMask; // "ubitmask"
        public BitMask ExponentSizeMask => _metadata.ExponentSizeMask; // "esizemask"
        public BitMask FractionSizeMask => _metadata.FractionSizeMask; // "fsizemask"
        public BitMask ExponentAndFractionSizeMask => _metadata.ExponentAndFractionSizeMask; // "efsizemask"
        public BitMask UnumTagMask => _metadata.UnumTagMask; // "utagmask"
        public BitMask SignBitMask => _metadata.SignBitMask; // "signbigu"
        #endregion

        #region Unum metadata
        public BitMask ULP => _metadata.ULP; // Unit in the Last Place or Unit of Least Precision.

        public BitMask PositiveInfinity => _metadata.PositiveInfinity; // "posinfu"
        public BitMask NegativeInfinity => _metadata.NegativeInfinity; // "neginfu"

        public BitMask QuietNotANumber => _metadata.QuietNotANumber; // "qNaNu"
        public BitMask SignalingNotANumber => _metadata.SignalingNotANumber; // "sNaNu"

        public BitMask LargestPositive => _metadata.LargestPositive; // "maxrealu"
        public BitMask SmallestPositive => _metadata.SmallestPositive; // "smallsubnormalu"

        public BitMask LargestNegative => _metadata.LargestNegative; // "negbigu"
        public BitMask MinRealU => _metadata.MinRealU; // "minrealu"

        //private uint _smallNormal; // "smallnormalu"
        #endregion


        public Unum(byte exponentSizeSize, byte fractionSizeSize)
        {
            _metadata = new UnumMetadata(exponentSizeSize, fractionSizeSize);

            SignBit = false;
            UncertaintyBit = false;

            ExponentBits = new bool[_metadata.ExponentSizeMax];
            FractionBits = new bool[_metadata.FractionSizeMax];

            ExponentSizeBits = new bool[_metadata.ExponentSizeSize];
            FractionSizeBits = new bool[_metadata.FractionSizeSize];
        }

        public Unum(UnumMetadata metadata)
        {
            _metadata = metadata;

            SignBit = false;
            UncertaintyBit = false;

            ExponentBits = new bool[_metadata.ExponentSizeMax];
            FractionBits = new bool[_metadata.FractionSizeMax];

            ExponentSizeBits = new bool[_metadata.ExponentSizeSize];
            FractionSizeBits = new bool[_metadata.FractionSizeSize];
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
