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
        public byte FractionSizeMax => _metadata.FractionSizeMax; // "fsizemax"

        public byte UnumTagSize => _metadata.UnumTagSize; // "utagsize"
        public byte Size => _metadata.Size; // "maxubits"

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


            UnumBits = new BitMask(_metadata.Size, false);
        }

        public Unum(UnumMetadata metadata)
        {
            _metadata = metadata;


            UnumBits = new BitMask(_metadata.Size, false);

        }

        public Unum(BitMask WholeUnum, byte exponentSizeSize, byte fractionSizeSize)
        {
            _metadata = new UnumMetadata(exponentSizeSize, fractionSizeSize);


            UnumBits = WholeUnum;

        }

        public Unum(BitMask WholeUnum, UnumMetadata metadata)
        {
            _metadata = metadata;


            UnumBits = WholeUnum;

        }

        #endregion

        #region Methods to set Unum parts

        public void SetUnumBits(bool signBit, BitMask exponent, BitMask fraction, bool ubit, uint exponentSize,
            uint fractionSize)
        {
            var WholeUnum = new BitMask(_metadata.Size, false);
            WholeUnum = FractionSizeMask & new BitMask(new uint[] { fractionSize }, Size);
            WholeUnum = WholeUnum | (new BitMask(new uint[] { exponentSize }, Size) << FractionSizeSize);
            if (ubit) WholeUnum = WholeUnum | UncertaintyBitMask;
            WholeUnum = WholeUnum | (fraction << FractionSizeSize + ExponentSizeSize + 1);
            WholeUnum = WholeUnum | (exponent << (int)(FractionSizeSize + ExponentSizeSize + 1 + fractionSize + 1));
            if (signBit) WholeUnum = WholeUnum | SignBitMask;


            UnumBits = WholeUnum;
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


        public void Negate()
        {
            UnumBits ^= SignBitMask;
        }

        public bool IsExact()
        {
            return (UnumBits & UncertaintyBitMask) == new BitMask(Size, false);
        }

        public bool IsPositive()
        {
            return ((UnumBits & SignBitMask) == new BitMask(Size, false));

        }

        public bool IsZero()
        {
            var zero = new BitMask(Size, false);
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
            var FractionMask = new BitMask(new uint[] { 1 }, Size);
            return ((FractionMask << (int)FractionSize()) - 1) << UnumTagSize;
        }

        public BitMask ExponentMask() //expomask
        {
            BitMask ExponentMask = new BitMask(new uint[] { 1 }, Size);
            return ((ExponentMask << (int)ExponentSize()) - 1) << (int)(FractionSize() + UnumTagSize);
        }

        #endregion


        #region Methods for Utag dependent Masks and values

        public BitMask Exponent() //expo
        {
            return (ExponentMask() & UnumBits) >> (int)(UnumTagSize + FractionSize());
        }

        public BitMask Fraction() //frac
        {
            return (FractionMask() & UnumBits) >> (int)(UnumTagSize);
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
            return (int)((1 << (int)(ExponentSize() - 1)) - 1);
        }

        public bool HiddenBitIsOne()
        {
            return (Exponent().GetLowest32Bits() > 0);
        }

        public int ExponentValueWithBias() //expovalue
        {

            int Value = (int)Exponent().GetLowest32Bits() - Bias() + 1;
            return HiddenBitIsOne() ? Value - 1 : Value;
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
            var scratchPad =
                new BitMask(left.Size, false); // It could be only FractionSizeMax +2 long if Hastlayer enabled it.


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


            var resultExponentSizeSize = (left.ExponentSizeSize > right.ExponentSizeSize)
                ? left.ExponentSizeSize
                : right.ExponentSizeSize;
            var resultFractionSizeSize = (left.FractionSizeSize > right.FractionSizeSize)
                ? left.FractionSizeSize
                : right.FractionSizeSize;
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
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit());
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit());
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
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit());
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit() -
                                                exponentValueDifference);


                scratchPad = left.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad, right.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);


            }
            else // Right exponent was bigger.
            {
                resultSignBit = !right.IsPositive();
                resultExponentValue = right.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit());
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit() +
                                                exponentValueDifference);


                scratchPad = right.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad, left.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);

            }

            int exponentChange = (int)scratchPad.FindLeadingOne() - (int)(resultUnum.FractionSizeMax + 1);


            var resultExponent = new BitMask(left.Size, false) +
                                 ExponentValueToExponentBits(resultExponentValue + exponentChange);
            resultExponentSize = ExponentValueToExponentSize(resultExponentValue + exponentChange) - 1;





            if (smallerBitsMovedToLeft < 0) // There are lost digits.
            {
                resultUbit = true;
            }
            else
            {
                BitMask.ShiftToRightEnd(scratchPad);
            }





            if (scratchPad.FindLeadingOne() == 0)
            {
                resultFractionSize = 0;
                resultExponent = scratchPad; // 0
                resultExponentSize = 0;
            }
            else
            {
                resultFractionSize = scratchPad.FindLeadingOne() - 1;
            }






            if (resultExponent.FindLeadingOne() != 0) // Erease hidden bit if there is one.
            {
                BitMask.SetZero(scratchPad, scratchPad.FindLeadingOne() - 1);
                resultFractionSize = (resultFractionSize == 0) ? 0 : resultFractionSize - 1;

            }



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
            return (left.UnumBits == right.UnumBits) ? true : false;
        }


        #endregion

        #region Helper methods for operations and conversions

        public static BitMask ExponentValueToExponentBits(int value)
        {
            if (value > 0)
            {
                var exponent = new BitMask(new uint[] { (uint)value });
                var exponentSize = ExponentValueToExponentSize(value);
                var bias = (1 << (int)(exponentSize - 1)) - 1;
                exponent += (uint)bias;
                return exponent;
            }
            else
            {
                var exponent = new BitMask(new uint[] { (uint)-value });
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

        public static int FloatExponentBitsToExponentValue(uint floatExponentBits)
        {
            return (int)floatExponentBits - 127;
        }



        public static BitMask AddAlignedFractions(BitMask left, BitMask right, bool SignBitsMatch)
        {
            if (SignBitsMatch)
            {
                left += right;
            }
            else
            {
                if (left > right)
                {
                    left -= right;
                }
                else
                {
                    left = right - left;
                }

            }
            return left;
        }



        #endregion



        #region Operators

        public static Unum operator +(Unum left, Unum right)
        {
            if (left.IsExact() && right.IsExact()) return AddExactUnums(left, right);

            return new Unum();
        }

        public static Unum operator -(Unum x)
        {
            if (x.IsExact()) return NegateExactUnum(x);
            return new Unum();
        }

        public static Unum operator -(Unum left, Unum right)
        {
            if (left.IsExact() && right.IsExact()) return SubtractExactUnums(left, right);

            return new Unum();
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
            if (left.IsExact() && right.IsExact()) return AreEqualExactUnums(left, right);
            return false;
        }

        public static bool operator !=(Unum left, Unum right)
        {
            return !(left == right);
        }

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

        // There can be loss of information when converting from int to Unum, depending on the Unum environment and the particular int
        public static implicit operator Unum(int x)
        {

            var result = new Unum(3, 4);
            if (x < 0)
            {


                result = (uint)-x;
                result.Negate();
                return result;

            }
            else return result = (uint)x;
        }

        public static implicit operator Unum(uint x)
        {   //spec cases
            if (x == 0) return new Unum(new BitMask(33, false), 3, 4);

            var result = new Unum(3, 4); //TODO  get this dinamically from Environment
            var fraction = new BitMask(result.Size, false);
            fraction += x;
            var exponentValue = fraction.FindLeadingOne() - 1;
            var exponent = new BitMask(new uint[] { exponentValue }, result.Size);
            var exponentSize = exponent.FindLeadingOne();
            if (exponentValue > (1 << (int)exponentSize - 1)) exponentSize++;
            var bias = (1 << (int)(exponentSize - 1)) - 1;
            exponent += (uint)bias;

            BitMask.ShiftToRightEnd(fraction);
            var fractionSize = fraction.FindLeadingOne() - 2;
            BitMask.SetZero(fraction, fraction.FindLeadingOne() - 1);


            result.SetUnumBits(false, exponent, fraction, false, exponentSize - 1, fractionSize);
            return result;
        }

        public static implicit operator Unum(float x)
        {

            var result = new Unum(3, 4);
            //spec values
            if (float.IsNaN(x)) return new Unum(result.QuietNotANumber, result._metadata);
            if (float.IsPositiveInfinity(x)) return new Unum(result.PositiveInfinity, result._metadata);
            if (float.IsNegativeInfinity(x)) return new Unum(result.NegativeInfinity, result._metadata);


            var floatBits = BitConverter.ToUInt32(BitConverter.GetBytes(x), 0);
            result.SetSignBit((floatBits > uint.MaxValue / 2));
            var floatExponentBits = (BitConverter.ToUInt32(BitConverter.GetBytes(x), 0) << 1) >> 24;
            var floatFractionBits = (BitConverter.ToUInt32(BitConverter.GetBytes(x), 0) << 9) >> 9;
            uint resultFractionSize = 23;
            var uncertain = false;



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
                }

            }



            if (result.FractionSizeMax < resultFractionSize - 1)
            {


                //resultFractionSize -= resultFractionSize - result.FractionSizeMax;


                result.SetFractionSizeBits((uint)result.FractionSizeMax - 1);
                uncertain = true;
            }
            else
                result.SetFractionSizeBits(resultFractionSize - 1);



            // These are the only Uncertain cases that we can safely handle without Ubounds.
            if (result.ExponentSizeMax < ExponentValueToExponentSize((int)floatExponentBits - 127))
            {
                // The exponent is too big, so we express the number as the largest possible signed value, but uncertain.
                // This means it's finite, but too big to express.
                if (floatExponentBits - 127 > 0)
                {
                    result.UnumBits = result.IsPositive() ? result.LargestPositive : result.LargestNegative;

                }
                else // If the exponent is too small, we will handle it as a signed uncertain zero.
                {
                    if (result.IsPositive())
                    {
                        result.UnumBits = new BitMask(result.Size, false);
                    }
                    else
                    {
                        result.UnumBits = new BitMask(result.Size, false);
                        result.Negate();
                    }


                }
                result.SetUncertainityBit(true);
            }

            result.SetExponentSizeBits(ExponentValueToExponentSize((int)floatExponentBits - 127) - 1);

            if (uncertain)
            {
                result.SetFractionBits(new BitMask(
                    new uint[] { floatFractionBits >> (int)resultFractionSize - result.FractionSizeMax }, result.Size));
                result.SetUncertainityBit(true);
            }
            else
            {
                result.SetFractionBits(new BitMask(new uint[] { floatFractionBits }, result.Size));
            }


            result.SetExponentBits(new BitMask(result.Size, false) + ExponentValueToExponentBits((int)(floatExponentBits - 127)));



            return result;
        }


        public static implicit operator Unum(double x)
        {
            var result = new Unum(3, 4);
            //spec values
            if (double.IsNaN(x)) return new Unum(result.QuietNotANumber, result._metadata);
            if (double.IsPositiveInfinity(x)) return new Unum(result.PositiveInfinity, result._metadata);
            if (double.IsNegativeInfinity(x)) return new Unum(result.NegativeInfinity, result._metadata);


            var doubleBits = BitConverter.ToUInt64(BitConverter.GetBytes(x), 0);
            result.SetSignBit((doubleBits > ulong.MaxValue / 2));
            var doubleExponentBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 1) >> 53;
            var doubleFractionBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 12) >> 12;
            uint resultFractionSize = 52;
            var uncertain = false;



            if (doubleFractionBits == 0)
            {
                resultFractionSize = 0;

            }
            else
            {
                while (doubleFractionBits % 2 == 0)
                {

                    resultFractionSize -= 1;
                    doubleFractionBits >>= 1;
                }

            }



            if (result.FractionSizeMax < resultFractionSize - 1)
            {
                result.SetFractionSizeBits((uint)result.FractionSizeMax - 1);
                uncertain = true;
            }
            else
                result.SetFractionSizeBits(resultFractionSize - 1);



            // These are the only Uncertain cases that we can safely handle without Ubounds.
            if (result.ExponentSizeMax < ExponentValueToExponentSize((int)doubleExponentBits - 1023))
            {
                // The exponent is too big, so we express the number as the largest possible signed value, but uncertain.
                // This means it's finite, but too big to express.
                if (doubleExponentBits - 1023 > 0)
                {
                    result.UnumBits = result.IsPositive() ? result.LargestPositive : result.LargestNegative;

                }
                else // If the exponent is too small, we will handle it as a signed uncertain zero.
                {
                    if (result.IsPositive())
                    {
                        result.UnumBits = new BitMask(result.Size, false);
                    }
                    else
                    {
                        result.UnumBits = new BitMask(result.Size, false);
                        result.Negate();
                    }


                }
                result.SetUncertainityBit(true);
            }

            result.SetExponentSizeBits(ExponentValueToExponentSize((int)doubleExponentBits - 1023) - 1);
            var doubleFraction = new uint[2];
            doubleFraction[0] = (uint)((doubleFractionBits << 32) >> 32);
            doubleFraction[1] = (uint)((doubleFractionBits >> 32));

            if (uncertain)
            {
                if (result.Size > 32) // This is necessary because Hastlayer enables only one size of BitMasks.
                {
                    result.SetFractionBits(new BitMask(doubleFraction, result.Size) >>
                        ((int)resultFractionSize - result.FractionSizeMax));

                }
                else
                {   // The lower 32 bits wouldn't fit in anyway.
                    result.SetFractionBits(new BitMask(new uint[] { doubleFraction[1] }, result.Size) >>
                        ((int)resultFractionSize - result.FractionSizeMax));
                }


                result.SetUncertainityBit(true);
            }
            else
            {   // This is necessary because Hastlayer enables only one size of BitMasks.
                result.SetFractionBits(result.Size > 32
                    ? new BitMask(doubleFraction, result.Size)
                    : new BitMask(new uint[] {doubleFraction[1]}, result.Size)); // The lower 32 bits wouldn't fit in anyway.
            }


            result.SetExponentBits(new BitMask(result.Size, false) + ExponentValueToExponentBits((int)(doubleExponentBits - 1023)));



            return result;
        }

        

        //Converting from an Unum to int results in information loss, so only allowing it explicitly(with a cast).
        public static explicit operator int(Unum x)
        {

            uint result;

            
            if ((x.ExponentValueWithBias() + (int)x.FractionSizeWithHiddenBit()) < 31) //The Unum fits into the range.
            {
                result = (x.FractionWithHiddenBit() << x.ExponentValueWithBias() -
                              ((int)x.FractionSizeWithHiddenBit() - 1)).GetLowest32Bits();

            }
            else // The absolute value of the Unum is too large.
            {
                return (x.IsPositive()) ? int.MaxValue : int.MinValue;
            }
            if (!x.IsPositive())
            {
                return -BitConverter.ToInt32(BitConverter.GetBytes(result), 0);
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(result), 0);
        }

        public static explicit operator uint(Unum x)
        {
            var result = (x.FractionWithHiddenBit() << x.ExponentValueWithBias() - ((int)x.FractionSizeWithHiddenBit() - 1)).GetLowest32Bits();
            return result;
        }

        public static explicit operator float(Unum x)
        {
            if (x.IsNan()) return float.NaN;
            if (x.IsNegativeInfinity()) return float.NegativeInfinity;
            if (x.IsPositiveInfinity()) return float.PositiveInfinity;
            if (x.ExponentValueWithBias() > 127) // Exponent is too big for float format.
            {
                return (x.IsPositive()) ? float.PositiveInfinity : float.NegativeInfinity;
            }
            if (x.ExponentValueWithBias() < -126) // Exponent is too small for float format.
            {
                return (x.IsPositive()) ? 0 : -0;
            }


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
