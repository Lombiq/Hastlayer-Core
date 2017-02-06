namespace Hast.Common.Numerics.Unum
{
    public struct Unum// : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        #region Unum structure
        public byte ExponentSizeSize { get; private set; } // "esizesize"
        public byte FractionSizeSize { get; private set; } // "fsizesize"

        public byte ExponentSizeMax { get; private set; } // "esizemax"
        public byte FractionSizeMax { get; private set; } // "fsizemax"

        public byte UnumTagSize { get; private set; } // "utagsize"
        public byte Size { get; private set; } // "maxubits"
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
        public BitMask UncertaintyBitMask { get; private set; } // "ubitmask"
        public BitMask ExponentSizeMask { get; private set; } // "esizemask"
        public BitMask FractionSizeMask { get; private set; }// "fsizemask"
        public BitMask ExponentAndFractionSizeMask { get; private set; } // "efsizemask"
        public BitMask UnumTagMask { get; private set; } // "utagmask"

        // uint is too small for a not particularly big unum, to be refactored.
        //public uint SignBitMask { get; private set; } // "signbigu", a unum in which all bits are zero except the sign bit;
        #endregion

        #region Unum metadata
        // Bit masks should be at least of size "_unumTagSize + 1", instead of fixed 32 bits.
        // The precision where it could matter is ridiculously high though, so it shouldn't matter for now.

        public BitMask ULP { get; private set; } // Unit in the Last Place or Unit of Least Precision.

        public BitMask PositiveInfinity { get; private set; } // "posinfu", the positive infinity for the given unum environment.
        public BitMask NegativeInfinity { get; private set; } // "neginfu", the negative infinity for the given unum environment.

        public BitMask QuietNotANumber { get; private set; } // "qNaNu"
        public BitMask SignalingNotANumber { get; private set; } // "sNaNu"

        public BitMask LargestPositive { get; private set; } // "maxrealu", the largest magnitude positive real number. One ULP less than infinity.
        public BitMask SmallestPositive { get; private set; } // "smallsubnormalu", the smallest magnitude positive real number. One ULP more than 0.

        public BitMask LargestNegative { get; private set; } // "negbigu", the largest maginude negative real number. One ULP more than negative infinity.
        public BitMask MinRealU { get; private set; } // "minrealu", looks like to be exactly the same as "negbigu".

        //private uint _smallNormal; // "smallnormalu"
        #endregion


        public Unum(byte exponentSizeSize, byte fractionSizeSize)
        {
            // Initializing structure.
            ExponentSizeSize = exponentSizeSize;
            FractionSizeSize = fractionSizeSize;

            ExponentSizeMax = (byte)(1 << ExponentSizeSize);
            FractionSizeMax = (byte)(1 << FractionSizeSize);

            UnumTagSize = (byte)(1 + ExponentSizeSize + FractionSizeSize);
            Size = (byte)(1 + ExponentSizeMax + FractionSizeMax + UnumTagSize);

            // Initializing blocks.
            SignBit = false;
            UncertaintyBit = false;

            ExponentBits = new bool[ExponentSizeMax];
            FractionBits = new bool[FractionSizeMax];

            ExponentSizeBits = new bool[ExponentSizeSize];
            FractionSizeBits = new bool[FractionSizeSize];

            // Initializing masks.
            UncertaintyBitMask = new BitMask(Size, false);
            BitMask.SetOne(UncertaintyBitMask, (uint)UnumTagSize - 1);

            FractionSizeMask = new BitMask(Size, false);
            BitMask.SetOne(FractionSizeMask, FractionSizeSize);
            FractionSizeMask--;


            ExponentSizeMask = (UncertaintyBitMask - 1) - FractionSizeMask;
            ExponentAndFractionSizeMask = ExponentSizeMask | FractionSizeMask;
            UnumTagMask = UncertaintyBitMask | ExponentAndFractionSizeMask;

            // Initializing metadata.
            ULP = new BitMask(Size, false);
            BitMask.SetOne(ULP, UnumTagSize);

            PositiveInfinity = new BitMask(Size, false);
            BitMask.SetOne(PositiveInfinity, (uint)Size - 1);
            PositiveInfinity -= 1;
            PositiveInfinity -= UncertaintyBitMask;

            NegativeInfinity = new BitMask(Size, false);
            BitMask.SetOne(NegativeInfinity, (uint)Size - 1);
            NegativeInfinity += PositiveInfinity;

            LargestPositive = PositiveInfinity - ULP;
            SmallestPositive = ExponentAndFractionSizeMask + ULP;

            LargestNegative = NegativeInfinity - ULP;

            MinRealU = LargestPositive + ((uint)1 << Size - 1);

            QuietNotANumber = PositiveInfinity + UncertaintyBitMask;
            SignalingNotANumber = NegativeInfinity + UncertaintyBitMask;

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
