namespace Hast.Common.Numerics.Unum
{
    public class UnumMetadata
    {
        #region Unum structure
        public byte ExponentSizeSize { get; private set; } // "esizesize"
        public byte FractionSizeSize { get; private set; } // "fsizesize"

        public byte ExponentSizeMax { get; private set; } // "esizemax"
        public byte FractionSizeMax { get; private set; } // "fsizemax"

        public byte UnumTagSize { get; private set; } // "utagsize"
        public byte Size { get; private set; } // "maxubits"
        #endregion

        #region Unum masks
        public BitMask UncertaintyBitMask { get; private set; } // "ubitmask"
        public BitMask ExponentSizeMask { get; private set; } // "esizemask"
        public BitMask FractionSizeMask { get; private set; } // "fsizemask"
        public BitMask ExponentAndFractionSizeMask { get; private set; } // "efsizemask"
        public BitMask UnumTagMask { get; private set; } // "utagmask"

        //public uint SignBitMask { get; private set; } // "signbigu", a unum in which all bits are zero except the sign bit;
        #endregion

        #region Unum special values
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


        public UnumMetadata(byte exponentSizeSize, byte fractionSizeSize)
        {
            // Initializing structure.
            ExponentSizeSize = exponentSizeSize;
            FractionSizeSize = fractionSizeSize;

            ExponentSizeMax = (byte)(1 << ExponentSizeSize);
            FractionSizeMax = (byte)(1 << FractionSizeSize);

            UnumTagSize = (byte)(1 + ExponentSizeSize + FractionSizeSize);
            Size = (byte)(1 + ExponentSizeMax + FractionSizeMax + UnumTagSize);

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
    }
}
