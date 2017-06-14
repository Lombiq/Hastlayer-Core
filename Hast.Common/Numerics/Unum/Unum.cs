using System;

namespace Hast.Common.Numerics.Unum
{

    // Signbit Exponent Fraction Ubit Exponentsize FractionSize
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

        public void SetUnumBits(bool SignBit, BitMask Exponent, BitMask Fraction, bool Ubit, uint ExponentSize, uint FractionSize)
        {
            var WholeUnum = new BitMask(_metadata.Size, false);
            WholeUnum = FractionSizeMask & new BitMask(new uint[] { FractionSize }, Size);
            WholeUnum = WholeUnum | (new BitMask(new uint[] { ExponentSize }, Size) << FractionSizeSize);
            if (Ubit) WholeUnum = WholeUnum | UncertaintyBitMask;
            WholeUnum = WholeUnum | (Fraction << FractionSizeSize + ExponentSizeSize + 1);
            WholeUnum = WholeUnum | (Exponent << (int)(FractionSizeSize + ExponentSizeSize + 1 + FractionSize + 1));
            if (SignBit) WholeUnum = WholeUnum | SignBitMask;


            UnumBits = WholeUnum;
        }

        public void Negate()
        {
            UnumBits ^= SignBitMask;
        }

        public bool IsExact()
        {
            if ((UnumBits & UncertaintyBitMask) == new BitMask(Size, false))
                return true;

            return false;
        }
        public bool IsPositive()
        {
            return ((UnumBits & SignBitMask) == new BitMask(Size, false));

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

        #region methods for Utag dependent Masks and values

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
        public bool IsZero()
        {
            var zero = new BitMask(Size, false);
            return ((UnumBits & UncertaintyBitMask) == zero) && ((UnumBits & FractionMask()) == zero) && ((UnumBits & ExponentMask()) == zero);
        }

        #region Operations for exact Unums

        public static Unum AddExactUnums(Unum left, Unum right)
        {
            BitMask ScratchPad1 = new BitMask(left.Size, false);  // It could be only FractionSizeMax +2 long if Hastlayer enabled it.
            BitMask ScratchPad2 = new BitMask(left.Size, false);

            // Handling special values.
            if (left.IsNan() || right.IsNan()) return new Unum(left.QuietNotANumber, left._metadata);
            if ((left.IsPositiveInfinity() && right.IsNegativeInfinity()) || (left.IsNegativeInfinity() && right.IsPositiveInfinity())) return new Unum(left.QuietNotANumber, left._metadata);
            if (left.IsPositiveInfinity() || right.IsPositiveInfinity()) return new Unum(left.PositiveInfinity, left._metadata);
            if (left.IsNegativeInfinity() || right.IsNegativeInfinity()) return new Unum(left.NegativeInfinity, left._metadata);
            if (left.IsZero()) return right;
            if (right.IsZero()) return left;


            var ResultExponentSizeSize = (left.ExponentSizeSize > right.ExponentSizeSize) ? left.ExponentSizeSize : right.ExponentSizeSize;
            var ResultFractionSizeSize = (left.FractionSizeSize > right.FractionSizeSize) ? left.FractionSizeSize : right.FractionSizeSize;
            var ResultUnum = new Unum(ResultExponentSizeSize, ResultFractionSizeSize);

            var ExponentValueDifference = left.ExponentValueWithBias() - right.ExponentValueWithBias();
            var SignBitsMatch = (left.IsPositive() == right.IsPositive());
            var ResultSignBit = false;
            var ResultUbit = false;
            var BiggerBitsMovedToLeft = 0;
            var SmallerBitsMovedToLeft = 0;
            var ResultExponentValue = 0;
            uint ResultFractionSize = 0;
            uint ResultExponentSize = 0;


            if (ExponentValueDifference == 0)// Exponents are equal.
            {
                ResultExponentValue = left.ExponentValueWithBias();
                BiggerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit());
                SmallerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit());
                ScratchPad1 = AddAlignedFractions(left.FractionWithHiddenBit() << BiggerBitsMovedToLeft,
                                                  right.FractionWithHiddenBit() << SmallerBitsMovedToLeft,
                                                  SignBitsMatch);



                if (!SignBitsMatch)
                {
                    if (left.FractionWithHiddenBit() >= right.FractionWithHiddenBit()) // Left Fraction is bigger.
                    {
                        ResultSignBit = !left.IsPositive();

                    }
                    else // Right Fraction is bigger.
                    {
                        ResultSignBit = !right.IsPositive();
                    }
                }

            }
            else if (ExponentValueDifference >= 0) // Left exponent was bigger.
            {
                ResultSignBit = !left.IsPositive();
                ResultExponentValue = left.ExponentValueWithBias();
                BiggerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit());
                SmallerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit() - ExponentValueDifference);


                ScratchPad1 = left.FractionWithHiddenBit() << BiggerBitsMovedToLeft;
                ScratchPad2 = right.FractionWithHiddenBit() << SmallerBitsMovedToLeft;
                ScratchPad1 = AddAlignedFractions(ScratchPad1, ScratchPad2, SignBitsMatch);


            }
            else // Right exponent was bigger.
            {
                ResultSignBit = !right.IsPositive();
                ResultExponentValue = right.ExponentValueWithBias();
                BiggerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - right.FractionSizeWithHiddenBit());
                SmallerBitsMovedToLeft = (int)(ResultUnum.FractionSizeMax + 1 - left.FractionSizeWithHiddenBit() + ExponentValueDifference);


                ScratchPad1 = right.FractionWithHiddenBit() << BiggerBitsMovedToLeft;
                ScratchPad2 = left.FractionWithHiddenBit() << SmallerBitsMovedToLeft;
                ScratchPad1 = AddAlignedFractions(ScratchPad1, ScratchPad2, SignBitsMatch);

            }

            int ExponentChange = (int)ScratchPad1.FindLeadingOne() - (int)(ResultUnum.FractionSizeMax + 1);

          
            var ResultExponent = new BitMask(left.Size, false)+ ExponentValueToExponentBits(ResultExponentValue + ExponentChange);
            ResultExponentSize = ExponentValueToExponentSize(ResultExponentValue + ExponentChange)-1;

       



            if (SmallerBitsMovedToLeft < 0) // There are lost digits.
            {
                ResultUbit = true;
            }
            else
            {
                BitMask.ShiftToRightEnd(ScratchPad1);
            }





            if (ScratchPad1.FindLeadingOne() == 0)
            {
                ResultFractionSize = 0;
                ResultExponent = ScratchPad1; // 0
                ResultExponentSize = 0;
            }
            else
            {
                ResultFractionSize = ScratchPad1.FindLeadingOne() - 1;
            }


           



            if (ResultExponent.FindLeadingOne() != 0) // Erease hidden bit if there is one.
            {
                BitMask.SetZero(ScratchPad1, ScratchPad1.FindLeadingOne() - 1);
                ResultFractionSize = (ResultFractionSize == 0) ? 0 : ResultFractionSize - 1;

            }



            ResultUnum.SetUnumBits(ResultSignBit, ResultExponent, ScratchPad1, ResultUbit, ResultExponentSize, ResultFractionSize);
            
            return ResultUnum;
        }



        #region Helper methods for addition
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

        public static Unum SubtractExactUnums(Unum left, Unum right)
        {
            return AddExactUnums(left, NegateExactUnum(right));
        }


        public static Unum NegateExactUnum(Unum input)
        {
            input.Negate();
            return input;
        }


        public static Unum MultiplyExactUnums(Unum left, Unum right)
        {
            return new Unum();
        }


        public static bool AreEqualExactUnums(Unum left, Unum right)
        {
            if (left.IsZero() && right.IsZero()) return true;
            return (left.UnumBits == right.UnumBits) ? true : false;
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

        #region operators

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

        public static Unum operator *(Unum left, Unum right)
        {
            if (left.IsExact() && right.IsExact()) return MultiplyExactUnums(left, right);

            return new Unum();

        }

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

            Unum result = new Unum(3, 4);
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
            var ResultSign = false;
            var Fraction = new BitMask(result.Size, false);
            Fraction += x;
            var ExponentValue = Fraction.FindLeadingOne() - 1;
            BitMask Exponent = new BitMask(new uint[] { ExponentValue }, result.Size);
            var ExponentSize = Exponent.FindLeadingOne();
            if (ExponentValue > (1 << (int)ExponentSize - 1)) ExponentSize++;
            var Bias = (1 << (int)(ExponentSize - 1)) - 1;
            Exponent += (uint)Bias;

            BitMask.ShiftToRightEnd(Fraction);
            var FractionSize = Fraction.FindLeadingOne() - 2;
            BitMask.SetZero(Fraction, Fraction.FindLeadingOne() - 1);


            result.SetUnumBits(ResultSign, Exponent, Fraction, false, ExponentSize - 1, FractionSize);
            return result;
        }

        public static implicit operator Unum(float x)
        {


            return new Unum();
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
    }
}
