using System;
using System.Runtime.Remoting.Messaging;

namespace Hast.Common.Numerics.Unum
{

    // Signbit Exponent Fraction Ubit ExponentSize FractionSize
    public struct Unum // : IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        private UnumMetadata _metadata { get; set; }

        #region Unum structure

        public byte ExponentSizeSize => _metadata.ExponentSizeSize; // "esizesize"
        public byte FractionSizeSize => _metadata.FractionSizeSize; // "fsizesize"

        public byte ExponentSizeMax => _metadata.ExponentSizeMax; // "esizemax"
        public ushort FractionSizeMax => _metadata.FractionSizeMax; // "fsizemax"

        public byte UnumTagSize => _metadata.UnumTagSize; // "utagsize"
        public ushort Size => _metadata.Size; // "maxubits"

        #endregion

        #region Unum blocks

        public BitMask UnumBits { get; set; }

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

        #region Unum constructors

        public Unum(byte exponentSizeSize, byte fractionSizeSize)
        {
            _metadata = new UnumMetadata(exponentSizeSize, fractionSizeSize);


            UnumBits = new BitMask(_metadata.Size);
        }

        public Unum(UnumMetadata metadata)
        {
            _metadata = metadata;


            UnumBits = new BitMask(_metadata.Size);

        }

        public Unum(BitMask wholeUnum, byte exponentSizeSize, byte fractionSizeSize)
        {
            _metadata = new UnumMetadata(exponentSizeSize, fractionSizeSize);


            UnumBits = wholeUnum;

        }

        public Unum(BitMask wholeUnum, UnumMetadata metadata)
        {
            _metadata = metadata;


            UnumBits = wholeUnum;

        }

        // This doesn't work for all cases yet.
        public Unum(UnumMetadata environment, float x)
        {
            _metadata = environment;
            //spec values
            if (float.IsNaN(x))
            {
                UnumBits = _metadata.QuietNotANumber;
                return;
            }

            if (float.IsPositiveInfinity(x))
            {
                UnumBits = _metadata.PositiveInfinity;
                return;
            }

            if (float.IsNegativeInfinity(x))
            {
                UnumBits = _metadata.NegativeInfinity;
                return;
            }

            UnumBits = new BitMask(_metadata.Size);


            var floatBits = BitConverter.ToUInt32(BitConverter.GetBytes(x), 0);
            var resultSignBit = (floatBits > uint.MaxValue / 2);
            var floatExponentBits = (BitConverter.ToUInt32(BitConverter.GetBytes(x), 0) << 1) >> 24;
            var floatFractionBits = (BitConverter.ToUInt32(BitConverter.GetBytes(x), 0) << 9) >> 9;
            uint resultFractionSize = 23;
            var uncertain = false;
            BitMask resultFraction;
            uint floatFractionBitsSize = 23;



            // These are the only Uncertain cases that we can safely handle without Ubounds.
            if (ExponentSizeMax < ExponentValueToExponentSize((int)floatExponentBits - 127))
            {
                // The exponent is too big, so we express the number as the largest possible signed value, but uncertain.
                // This means it's finite, but too big to express.
                if (floatExponentBits - 127 > 0)
                {
                    UnumBits = IsPositive() ? LargestPositive : LargestNegative;

                }
                else // If the exponent is too small, we will handle it as a signed uncertain zero.
                {
                    if (IsPositive())
                    {
                        UnumBits = new BitMask(Size);
                    }
                    else
                    {
                        UnumBits = new BitMask(Size);
                        Negate();
                    }


                }
                SetUncertainityBit(true);
                return;
            }


            if (floatFractionBits == 0)
            {
                resultFractionSize = 0;

            }
            else
            {
                while (floatFractionBits % 2 == 0)
                {

                    resultFractionSize -= 1;
                    floatFractionBits >>= 1;
                    floatFractionBitsSize = resultFractionSize;

                }

            }



            if (FractionSizeMax + 1 < resultFractionSize)
            {


                //resultFractionSize -= resultFractionSize - result.FractionSizeMax;


                resultFractionSize = ((uint)FractionSizeMax - 1);
                uncertain = true;
            }
            else
                if (resultFractionSize > 0) resultFractionSize = (resultFractionSize - 1);



            var resultExponentSize = (ExponentValueToExponentSize((int)floatExponentBits - 127) - 1);

            if (uncertain)
            {
                resultFraction = (new BitMask(
                    new uint[] { floatFractionBits >> (int)floatFractionBitsSize - FractionSizeMax }, Size)); //there is the problem

            }
            else
            {
                resultFraction = (new BitMask(new uint[] { floatFractionBits }, Size));
            }


            var resultExponent = ExponentValueToExponentBits((int)(floatExponentBits - 127), Size);

            SetUnumBits(resultSignBit, resultExponent, resultFraction, uncertain, resultExponentSize,
                 resultFractionSize);





        }

        public Unum(UnumMetadata environment, uint x)
        {

            _metadata = environment;
            UnumBits = _metadata.EmptyBitMask;
            if (x == 0) return;


            UnumBits += x; // The Fraction will be stored here.
            var exponentValue = UnumBits.FindLeadingOne() - 1;
            var exponent = new BitMask(new uint[] { exponentValue }, Size);
            var exponentSize = exponent.FindLeadingOne();
            if (exponentValue > (1 << (int)exponentSize - 1)) exponentSize++;
            var bias = (1 << (int)(exponentSize - 1)) - 1;
            exponent += (uint)bias;


            BitMask.ShiftToRightEnd(UnumBits);
            var fractionSize = UnumBits.FindLeadingOne() - 1;
            if (fractionSize > 0) fractionSize -= 1;
            if (exponentValue != 0) BitMask.SetZero(UnumBits, UnumBits.FindLeadingOne() - 1);


            SetUnumBits(false, exponent, UnumBits, false, exponentSize - 1, fractionSize);
        }
        public Unum(UnumMetadata environment, uint[] input)
        {

            _metadata = environment;
            UnumBits = _metadata.EmptyBitMask;

            // Copying input to UnumBits BitMask.
            for (var i = input.Length - 1; i > 0; i--)
            {
                UnumBits += input[i];
                UnumBits <<= 32;
            }
            UnumBits += input[0];
            if (UnumBits == _metadata.EmptyBitMask) return;


            // Handling Signbit.
            var signBit = (input[input.Length - 1] > uint.MaxValue / 2);
            UnumBits <<= 1;
            UnumBits >>= 1;


            //Calcuating exponent value and size.  
            var exponentValue = UnumBits.FindLeadingOne() - 1;
            var exponentSize = 0;
            var j = 1;
            while (j < exponentValue)
            {
                j <<= 1;
                exponentSize++;
            }
            if (exponentValue > (1 << (int)exponentSize - 1)) exponentSize++;
            var bias = (1 << (int)(exponentSize - 1)) - 1;
            exponentValue += (uint)bias;
            var exponentMask = _metadata.EmptyBitMask + exponentValue;
            if (exponentSize > 0) exponentSize -= 1; //Until now we needed the value, now we need the notation.



            // Calculating Fraction.
            BitMask.ShiftToRightEnd(UnumBits);
            var fractionSize = UnumBits.FindLeadingOne() - 1;
            if (fractionSize > 0) fractionSize -= 1;
            if (exponentValue >0) BitMask.SetZero(UnumBits, UnumBits.FindLeadingOne() - 1);


            SetUnumBits(signBit, exponentMask, UnumBits, false, (uint)exponentSize , fractionSize);
        }

        public Unum(UnumMetadata environment, int x)
        {
            if (x >= 0) this = new Unum(environment, (uint)x);
            else
            {
                this = new Unum(environment, (uint)-x);
                Negate();
            }

        }

        // This doesn't work for all cases yet.
        public Unum(UnumMetadata environment, double x)
        {
            _metadata = environment;
            UnumBits = _metadata.EmptyBitMask;
            //spec values
            if (double.IsNaN(x))
            {
                UnumBits = QuietNotANumber;
                return;
            }
            if (double.IsPositiveInfinity(x))
            {
                UnumBits = PositiveInfinity;
                return;
            }
            if (double.IsNegativeInfinity(x))
            {
                UnumBits = NegativeInfinity;
                return;
            }


            var doubleBits = BitConverter.ToUInt64(BitConverter.GetBytes(x), 0);
            SetSignBit((doubleBits > ulong.MaxValue / 2));
            var doubleExponentBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 1) >> 53;
            var doubleFractionBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 12) >> 12;
            uint resultFractionSize = 52;
            var uncertain = false;



            if (doubleFractionBits == 0) resultFractionSize = 0;
            else
            {
                while (doubleFractionBits % 2 == 0)
                {
                    resultFractionSize -= 1;
                    doubleFractionBits >>= 1;
                }

            }



            if (FractionSizeMax < resultFractionSize - 1)
            {
                SetFractionSizeBits((uint)(FractionSizeMax - 1));
                uncertain = true;
            }
            else SetFractionSizeBits(resultFractionSize - 1);



            // These are the only Uncertain cases that we can safely handle without Ubounds.
            if (ExponentSizeMax < ExponentValueToExponentSize((int)doubleExponentBits - 1023))
            {
                // The exponent is too big, so we express the number as the largest possible signed value, but uncertain.
                // This means it's finite, but too big to express.
                if (doubleExponentBits - 1023 > 0)
                {
                    UnumBits = IsPositive() ? LargestPositive : LargestNegative;

                }
                else // If the exponent is too small, we will handle it as a signed uncertain zero.
                {

                    UnumBits = _metadata.EmptyBitMask;
                    if (!IsPositive()) Negate();

                }

                SetUncertainityBit(true);
                return;
            }

            var exponentSizeBits = ExponentValueToExponentSize((int)doubleExponentBits - 1023) - 1;
            SetExponentSizeBits(exponentSizeBits);
            var doubleFraction = new uint[2];
            doubleFraction[0] = (uint)((doubleFractionBits << 32) >> 32);
            doubleFraction[1] = (uint)((doubleFractionBits >> 32));

            if (uncertain)
            {
                if (Size > 32) // This is necessary because Hastlayer enables only one size of BitMasks.
                {

                    var fBits = new BitMask(doubleFraction, Size) >> ((int)resultFractionSize - (int)FractionSize());
                    SetFractionBits(fBits);

                }
                else
                {   // The lower 32 bits wouldn't fit in anyway.
                    SetFractionBits(new BitMask(new uint[] { doubleFraction[1] }, Size) >>
                                           ((int)resultFractionSize - FractionSizeMax));
                }


                SetUncertainityBit(true);
            }
            else
            {   // This is necessary because Hastlayer enables only one size of BitMasks.
                SetFractionBits(Size > 32
                     ? new BitMask(doubleFraction, Size)
                     : new BitMask(new uint[] { doubleFraction[1] }, Size)); // The lower 32 bits wouldn't fit in anyway.
            }


            SetExponentBits(ExponentValueToExponentBits((int)(doubleExponentBits - 1023), Size));

        }



        #endregion

        #region Methods to set Unum parts

        public void SetUnumBits(bool signBit, BitMask exponent, BitMask fraction, bool ubit, uint exponentSize,
            uint fractionSize)
        {
            var wholeUnum = _metadata.EmptyBitMask;

            wholeUnum = FractionSizeMask & new BitMask(new uint[] { fractionSize }, Size);
            wholeUnum = wholeUnum | (new BitMask(new uint[] { exponentSize }, Size) << FractionSizeSize);

            if (ubit) wholeUnum = wholeUnum | UncertaintyBitMask;

            wholeUnum = wholeUnum | (fraction << FractionSizeSize + ExponentSizeSize + 1);
            wholeUnum = wholeUnum | (exponent << (int)(FractionSizeSize + ExponentSizeSize + 1 + fractionSize + 1));

            if (signBit) wholeUnum = wholeUnum | SignBitMask;


            UnumBits = wholeUnum;
        }


        public void SetSignBit(bool signBit)
        {

            UnumBits = signBit ? UnumBits | SignBitMask : UnumBits & (new BitMask(Size, true) ^ (SignBitMask));
        }

        public void SetUncertainityBit(bool uncertainityBit)
        {

            UnumBits = uncertainityBit ? UnumBits | UncertaintyBitMask : UnumBits & (~UncertaintyBitMask);
        }

        public void SetExponentBits(BitMask exponent)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^ ExponentMask())) |
                       (exponent << (int)(FractionSizeSize + ExponentSizeSize + 1 + FractionSize()));
        }

        public void SetFractionBits(BitMask fraction)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^ FractionMask())) | (fraction << FractionSizeSize + ExponentSizeSize + 1);
        }

        public void SetFractionSizeBits(uint fractionSize)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^ FractionSizeMask)) | new BitMask(new uint[] { fractionSize }, Size);
        }

        public void SetExponentSizeBits(uint exponentSize)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^ ExponentSizeMask) |
                       (new BitMask(new uint[] { exponentSize }, Size) << FractionSizeSize));
        }

        #endregion
  
        
        public uint[] FractionToUintArray()
        {
            var resultMask = FractionWithHiddenBit() << ExponentValueWithBias() - (int)FractionSize();
            var result = new uint[resultMask.SegmentCount];

            for (var i = 0; i < resultMask.SegmentCount; i++)
            {
                result[i] = resultMask.Segments[i];
            }
            return result;
        }

        public void Negate()
        {
            UnumBits ^= SignBitMask;
        }

        public bool IsExact()
        {
            return (UnumBits & UncertaintyBitMask) == _metadata.EmptyBitMask;
        }

        public bool IsPositive()
        {
            return (UnumBits & SignBitMask) == _metadata.EmptyBitMask;

        }

        public bool IsZero()
        {
            var zero = _metadata.EmptyBitMask;
            return ((UnumBits & UncertaintyBitMask) == zero) && ((UnumBits & FractionMask()) == zero) &&
                   ((UnumBits & ExponentMask()) == zero);
        }

        #region  Methods for Utag independent Masks and values

        public uint ExponentSize() //esize 
        {
            //This limits the ExponentSizeSize to 32, which is so enormous that it shouldn't be a problem
            return (((UnumBits & ExponentSizeMask) >> FractionSizeSize) + 1).GetLowest32Bits();
        }

        public uint FractionSize() //fsize
        {
            //This limits FractionSizeSize to 32 , which is so enormous that is shouldn't be a problem
            return ((UnumBits & FractionSizeMask) + 1).GetLowest32Bits();
        }

        public BitMask FractionMask() //fracmask
        {
            var fractionMask = new BitMask(new uint[] { 1 }, Size);
            return ((fractionMask << (int)FractionSize()) - 1) << UnumTagSize;
        }

        public BitMask ExponentMask() //expomask
        {
            BitMask exponentMask = new BitMask(new uint[] { 1 }, Size);
            return ((exponentMask << (int)ExponentSize()) - 1) << (int)(FractionSize() + UnumTagSize);
        }

        #endregion


        #region Methods for Utag dependent Masks and values

        public BitMask Exponent() //expo
        {
            return (ExponentMask() & UnumBits) >> (int)(UnumTagSize + FractionSize());
        }

        public BitMask Fraction() //frac
        {
            return (FractionMask() & UnumBits) >> (int)UnumTagSize;
        }

        public BitMask FractionWithHiddenBit()
        {
            return HiddenBitIsOne() ? BitMask.SetOne(Fraction(), FractionSize()) : Fraction();
        }

        public uint FractionSizeWithHiddenBit()
        {
            return HiddenBitIsOne() ? FractionSize() + 1 : FractionSize();
        }

        public int Bias()
        {
            return (1 << (int)(ExponentSize() - 1)) - 1;
        }

        public bool HiddenBitIsOne()
        {
            return (Exponent().GetLowest32Bits() > 0);
        }

        public int ExponentValueWithBias() //expovalue
        {

            var value = (int)Exponent().GetLowest32Bits() - Bias() + 1;
            return HiddenBitIsOne() ? value - 1 : value;
        }

        public bool IsNan()
        {
            return (UnumBits == SignalingNotANumber || UnumBits == QuietNotANumber);
        }

        public bool IsPositiveInfinity()
        {

            return (UnumBits == PositiveInfinity);
        }

        public bool IsNegativeInfinity()
        {

            return (UnumBits == NegativeInfinity);
        }

        #endregion


        #region Operations for exact Unums

        public static Unum AddExactUnums(Unum left, Unum right)
        {
            var scratchPad = left._metadata.EmptyBitMask; // It could be only FractionSizeMax +2 long if Hastlayer enabled it.


            // Handling special values.
            if (left.IsNan() || right.IsNan()) return new Unum(left.QuietNotANumber, left._metadata);

            if ((left.IsPositiveInfinity() && right.IsNegativeInfinity()) ||
                (left.IsNegativeInfinity() && right.IsPositiveInfinity()))
                return new Unum(left.QuietNotANumber, left._metadata);

            if (left.IsPositiveInfinity() || right.IsPositiveInfinity())
                return new Unum(left.PositiveInfinity, left._metadata);

            if (left.IsNegativeInfinity() || right.IsNegativeInfinity())
                return new Unum(left.NegativeInfinity, left._metadata);
            //if (left.IsZero()) return right;
            //if (right.IsZero()) return left;
            


            var resultExponentSizeSize = left.ExponentSizeSize;
            var resultFractionSizeSize = left.FractionSizeSize; 
            var resultUnum = new Unum(resultExponentSizeSize, resultFractionSizeSize);

            var exponentValueDifference = left.ExponentValueWithBias() - right.ExponentValueWithBias();
            var signBitsMatch = (left.IsPositive() == right.IsPositive());
            var resultSignBit = false;
            var resultUbit = false;
            var biggerBitsMovedToLeft = 0;
            var smallerBitsMovedToLeft = 0;
            var resultExponentValue = 0;
            uint resultFractionSize = 0;
            uint resultExponentSize = 0;
            

            

            if (exponentValueDifference == 0) // Exponents are equal.
            {
                resultExponentValue = left.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (left.FractionSize()+1));
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (right.FractionSize()+1));
                scratchPad = AddAlignedFractions(left.FractionWithHiddenBit() << biggerBitsMovedToLeft,
                    right.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);



                if (!signBitsMatch)
                {
                    if (left.FractionWithHiddenBit() >= right.FractionWithHiddenBit()) // Left Fraction is bigger.
                    {
                        resultSignBit = !left.IsPositive();

                    }
                    else // Right Fraction is bigger.
                    {
                        resultSignBit = !right.IsPositive();
                    }
                }

            }
            else if (exponentValueDifference >= 0) // Left exponent was bigger.
            {
                resultSignBit = !left.IsPositive();
                resultExponentValue = left.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (left.FractionSize()+1));
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (right.FractionSize()+1) -
                                                exponentValueDifference);


                scratchPad = left.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad, right.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);


            }
            else // Right exponent was bigger.
            {
                resultSignBit = !right.IsPositive();
                resultExponentValue = right.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (right.FractionSize()+1));
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - (left.FractionSize()+1) +
                                                exponentValueDifference);


                scratchPad = right.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad, left.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);

            }

            var exponentChange = (int)scratchPad.FindLeadingOne() - (int)(resultUnum.FractionSizeMax + 1);


            var resultExponent = left._metadata.EmptyBitMask + 
                                 ExponentValueToExponentBits(resultExponentValue + exponentChange, left.Size);
            resultExponentSize = ExponentValueToExponentSize(resultExponentValue + exponentChange) - 1;





            if (smallerBitsMovedToLeft < 0) resultUbit = true; // There are lost digits.
            else BitMask.ShiftToRightEnd(scratchPad);




            if (scratchPad.FindLeadingOne() == 0)
            {
                resultFractionSize = 0;
                resultExponent = scratchPad; // 0
                resultExponentSize = 0;
            }
            else resultFractionSize = scratchPad.FindLeadingOne() - 1;
            



            if (resultExponent.FindLeadingOne() != 0) // Erease hidden bit if there is one.
            {
                BitMask.SetZero(scratchPad, scratchPad.FindLeadingOne() - 1);
                resultFractionSize = (resultFractionSize == 0) ? 0 : resultFractionSize - 1;

            }

            // This is temporary, for imitation of float behaviour. Now the ubit works as a flag for rounded values.
            if ((!left.IsExact()) || (!right.IsExact())) resultUbit = true;


            resultUnum.SetUnumBits(resultSignBit, resultExponent, scratchPad, resultUbit, resultExponentSize,
                resultFractionSize);

            return resultUnum;
        }





        public static Unum SubtractExactUnums(Unum left, Unum right)
        {
            return AddExactUnums(left, NegateExactUnum(right));
        }


        public static Unum NegateExactUnum(Unum input)
        {
            input.Negate();
            return input;
        }


        //public static Unum MultiplyExactUnums(Unum left, Unum right)
        //{
        //    return new Unum();
        //}


        public static bool AreEqualExactUnums(Unum left, Unum right)
        {
            if (left.IsZero() && right.IsZero()) return true;
            return (left.UnumBits == right.UnumBits);
        }


        #endregion


        #region Helper methods for operations and conversions

        public static BitMask ExponentValueToExponentBits(int value, uint size)
        {
            if (value > 0)
            {
                var exponent = new BitMask(new uint[] { (uint)value }, size);
                var exponentSize = ExponentValueToExponentSize(value);
                var bias = (1 << (int)(exponentSize - 1)) - 1;
                exponent += (uint)bias;

                return exponent;
            }
            else
            {
                var exponent = new BitMask(new uint[] { (uint)-value }, size);
                var exponentSize = ExponentValueToExponentSize(value);
                var Bias = (1 << (int)(exponentSize - 1)) - 1;
                exponent += (uint)Bias;
                exponent -= (uint)(-2 * value);

                return exponent;
            }
        }


        public static uint ExponentValueToExponentSize(int value)
        {
            uint size = 1;
            if (value > 0)
            {
                while (value > 1 << (int)(size - 1)) size++;
            }
            else
            {
                while (-value >= 1 << (int)(size - 1)) size++;
            }
            return size;
        }





        public static BitMask AddAlignedFractions(BitMask left, BitMask right, bool SignBitsMatch)
        {
            if (SignBitsMatch) left += right;
            else
            {
                if (left > right) left -= right;
                else left = right - left;
            }
            return left;
        }



        #endregion



        #region Operators

        public static Unum operator +(Unum left, Unum right)
        {
            return AddExactUnums(left, right);


        }

        public static Unum operator -(Unum x)
        {
            return NegateExactUnum(x);

        }


        public static Unum operator -(Unum left, Unum right)
        {
            return SubtractExactUnums(left, right);

        }

        //public static Unum operator *(Unum left, Unum right)
        //{
        //    if (left.IsExact() && right.IsExact()) return MultiplyExactUnums(left, right);

        //    return new Unum();

        //}

        //public static Unum operator /(Unum left, Unum right)
        //{

        //}

        public static bool operator ==(Unum left, Unum right)
        {
            return AreEqualExactUnums(left, right);
        }

        public static bool operator !=(Unum left, Unum right)
        {
            return !(left == right);
        }

        //public static bool operator <(Unum left, Unum right)
        // {
        //     if (left.IsPositive() != right.IsPositive()) return left.IsPositive();
        //     if (left.ExponentValueWithBias() > right.ExponentValueWithBias()) return left.IsPositive();
        //     if (left.ExponentValueWithBias() < right.ExponentValueWithBias()) return right.IsPositive();
        //    // if (left.FractionWithHiddenBit())
        //     return false;

        // }

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

        //Converting from an Unum to int results in information loss, so only allowing it explicitly(with a cast).
        public static explicit operator int(Unum x)
        {

            uint result;
            if ((x.ExponentValueWithBias() + (int)x.FractionSizeWithHiddenBit()) < 31) //The Unum fits into the range.
            {
                result = (x.FractionWithHiddenBit() << x.ExponentValueWithBias() -
                              (int)x.FractionSize() ).GetLowest32Bits();

            }
            else return (x.IsPositive()) ? int.MaxValue : int.MinValue; // The absolute value of the Unum is too large.
          

            if (!x.IsPositive()) return -(int)result;

            return (int)result;
        }

        public static explicit operator uint(Unum x)
        {
            
            var result = (x.FractionWithHiddenBit()
                << x.ExponentValueWithBias() - ((int)x.FractionSize() )).GetLowest32Bits();

            return result;
        }

        // This is not well tested yet.
        public static explicit operator float(Unum x)
        {
            if (x.IsNan()) return float.NaN;
            if (x.IsNegativeInfinity()) return float.NegativeInfinity;
            if (x.IsPositiveInfinity()) return float.PositiveInfinity;
            if (x.ExponentValueWithBias() > 127) // Exponent is too big for float format.
            {
                return (x.IsPositive()) ? float.PositiveInfinity : float.NegativeInfinity;
            }
            if (x.ExponentValueWithBias() < -126) return (x.IsPositive()) ? 0 : -0; // Exponent is too small for float format.


            var result = (x.Fraction() << 23 - ((int)x.FractionSize())).GetLowest32Bits();
            result |= ((uint)(x.ExponentValueWithBias() + 127) << 23);



            return (x.IsPositive()) ? BitConverter.ToSingle(BitConverter.GetBytes(result), 0)
                : -BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
        }

        #endregion
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
    }
}
