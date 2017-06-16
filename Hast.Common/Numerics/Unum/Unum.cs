using System;

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
            
            UnumBits = (UnumBits & (new BitMask(Size, true) ^ExponentMask())) |
                       (exponent << (int)(FractionSizeSize + ExponentSizeSize + 1 + FractionSize()));
        }

        public void SetFractionBits(BitMask fraction)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^FractionMask())) | (fraction << FractionSizeSize + ExponentSizeSize + 1);
        }

        public void SetFractionSizeBits(uint fractionSize)
        {
            
            UnumBits = (UnumBits & (new BitMask(Size, true) ^FractionSizeMask)) | new BitMask(new uint[] { fractionSize }, Size);
        }

        public void SetExponentSizeBits(uint exponentSize)
        {

            UnumBits = (UnumBits & (new BitMask(Size, true) ^ExponentSizeMask) |
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
            BitMask scratchPad1 =
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
                scratchPad1 = AddAlignedFractions(left.FractionWithHiddenBit() << biggerBitsMovedToLeft,
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


                scratchPad1 = left.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad1 = AddAlignedFractions(scratchPad1, right.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);


            }
            else // Right exponent was bigger.
            {
                resultSignBit = !right.IsPositive();
                resultExponentValue = right.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit());
                smallerBitsMovedToLeft = (int)(resultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit() +
                                                exponentValueDifference);


                scratchPad1 = right.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad1 = AddAlignedFractions(scratchPad1, left.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);

            }

            int exponentChange = (int)scratchPad1.FindLeadingOne() - (int)(resultUnum.FractionSizeMax + 1);


            var resultExponent = new BitMask(left.Size, false) +
                                 ExponentValueToExponentBits(resultExponentValue + exponentChange);
            resultExponentSize = ExponentValueToExponentSize(resultExponentValue + exponentChange) - 1;





            if (smallerBitsMovedToLeft < 0) // There are lost digits.
            {
                resultUbit = true;
            }
            else
            {
                BitMask.ShiftToRightEnd(scratchPad1);
            }





            if (scratchPad1.FindLeadingOne() == 0)
            {
                resultFractionSize = 0;
                resultExponent = scratchPad1; // 0
                resultExponentSize = 0;
            }
            else
            {
                resultFractionSize = scratchPad1.FindLeadingOne() - 1;
            }






            if (resultExponent.FindLeadingOne() != 0) // Erease hidden bit if there is one.
            {
                BitMask.SetZero(scratchPad1, scratchPad1.FindLeadingOne() - 1);
                resultFractionSize = (resultFractionSize == 0) ? 0 : resultFractionSize - 1;

            }



            resultUnum.SetUnumBits(resultSignBit, resultExponent, scratchPad1, resultUbit, resultExponentSize,
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

        public static BitMask ExponentValueToExponentBits(int Value)
        {
            if (Value > 0)
            {
                BitMask Exponent = new BitMask(new uint[] { (uint)Value });
                var ExponentSize = ExponentValueToExponentSize(Value);
                var Bias = (1 << (int)(ExponentSize - 1)) - 1;
                Exponent += (uint)Bias;
                return Exponent;
            }
            else
            {
                BitMask Exponent = new BitMask(new uint[] { (uint)-Value });
                var ExponentSize = ExponentValueToExponentSize(Value);
                var Bias = (1 << (int)(ExponentSize - 1)) - 1;
                Exponent += (uint)Bias;
                Exponent -= (uint)(-2 * Value);
                return Exponent;
            }
        }


        public static uint ExponentValueToExponentSize(int Value)
        {
            uint Size = 1;
            if (Value > 0)
            {
                while (Value > 1 << (int)(Size - 1)) Size++;
            }
            else
            {
                while (-Value >= 1 << (int)(Size - 1)) Size++;
            }
            return Size;
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



            if (result.FractionSizeMax < resultFractionSize-1)
            {
               
               
                //resultFractionSize -= resultFractionSize - result.FractionSizeMax;
                

                result.SetFractionSizeBits(result.FractionSizeMax);
                uncertain = true;
            }
            else
                result.SetFractionSizeBits(resultFractionSize-1);




            if (result.ExponentSizeMax < ExponentValueToExponentSize((int)floatExponentBits - 127))
            {
                throw new OverflowException("The dynamic range of given Unum environment is too small.");
            }
            result.SetExponentSizeBits(ExponentValueToExponentSize((int)floatExponentBits - 127)-1);

            if (uncertain)
            {
                result.SetFractionBits(new BitMask(
                    new uint[] { floatFractionBits >> (int)resultFractionSize - result.FractionSizeMax }, result.Size));
            }
            else
            {
                result.SetFractionBits(new BitMask(new uint[] { floatFractionBits },result.Size)); //
            }

            
            result.SetExponentBits(new BitMask( result.Size,false) + ExponentValueToExponentBits((int)(floatExponentBits - 127)));

          

            return result;
        }


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

        public static explicit operator float(Unum x)
        {
            return 1;
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
