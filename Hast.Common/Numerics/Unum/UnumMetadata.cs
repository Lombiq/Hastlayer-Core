namespace Hast.Common.Numerics.Unum
{
    public class UnumMetadata
    {
        #region Unum structure
        public byte ExponentSizeSize { get; } // "esizesize"
        public byte FractionSizeSize { get; } // "fsizesize"

        public byte ExponentSizeMax { get; } // "esizemax"
        public ushort FractionSizeMax { get; } // "fsizemax"

        public byte UnumTagSize { get; } // "utagsize"
        public ushort Size { get; } // "maxubits"
        #endregion

        #region Unum masks
        public BitMask EmptyBitMask { get; }

        public BitMask UncertaintyBitMask { get; } // "ubitmask"
        public BitMask ExponentSizeMask { get; } // "esizemask"
        public BitMask FractionSizeMask { get; } // "fsizemask"
        public BitMask ExponentAndFractionSizeMask { get; } // "efsizemask"
        public BitMask UnumTagMask { get; } // "utagmask"
        public BitMask SignBitMask { get; } // "signbigu", a unum in which all bits are zero except the sign bit;
        #endregion

        #region Unum special values
        public BitMask ULP { get; } // Unit in the Last Place or Unit of Least Precision.

        public BitMask PositiveInfinity { get; } // "posinfu", the positive infinity for the given unum environment.
        public BitMask NegativeInfinity { get; } // "neginfu", the negative infinity for the given unum environment.

        public BitMask QuietNotANumber { get; } // "qNaNu"
        public BitMask SignalingNotANumber { get; } // "sNaNu"

        public BitMask LargestPositive { get; } // "maxrealu", the largest magnitude positive real number. One ULP less than infinity.
        public BitMask SmallestPositive { get; } // "smallsubnormalu", the smallest magnitude positive real number. One ULP more than 0.

        public BitMask LargestNegative { get; } // "negbigu", the largest maginude negative real number. One ULP more than negative infinity.
        public BitMask MinRealU { get; } // "minrealu", looks like to be exactly the same as "negbigu".

        //private uint _smallNormal; // "smallnormalu"
        #endregion


        public UnumMetadata(byte exponentSizeSize, byte fractionSizeSize)
        {
            // Initializing structure.
            ExponentSizeSize = exponentSizeSize;
            FractionSizeSize = fractionSizeSize;

            ExponentSizeMax = (byte)(1 << ExponentSizeSize);
            FractionSizeMax = (ushort)(1 << FractionSizeSize);

            UnumTagSize = (byte)(1 + ExponentSizeSize + FractionSizeSize);
            Size = (ushort)(1 + ExponentSizeMax + FractionSizeMax + UnumTagSize);

            // Initializing masks.
            UncertaintyBitMask = new BitMask(Size);
            BitMask.SetOne(UncertaintyBitMask, (uint)UnumTagSize - 1);

            FractionSizeMask = new BitMask(Size);
            BitMask.SetOne(FractionSizeMask, FractionSizeSize);
            FractionSizeMask--;

            ExponentSizeMask = (UncertaintyBitMask - 1) - FractionSizeMask;
            ExponentAndFractionSizeMask = ExponentSizeMask | FractionSizeMask;
            UnumTagMask = UncertaintyBitMask | ExponentAndFractionSizeMask;

            SignBitMask = new BitMask(Size);
            BitMask.SetOne(SignBitMask, (uint)Size - 1);

            // Initializing metadata.
            EmptyBitMask = new BitMask(Size);
            ULP = new BitMask(Size);
            BitMask.SetOne(ULP, UnumTagSize);

            PositiveInfinity = new BitMask(Size);
            BitMask.SetOne(PositiveInfinity, (uint)Size - 1);
            PositiveInfinity -= 1;
            PositiveInfinity -= UncertaintyBitMask;

            NegativeInfinity = new BitMask(Size);
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
    }
}
