using System;

namespace Lombiq.Unum
{
    public struct Unum
    {
        private readonly UnumMetadata _metadata;

        // Signbit Exponent Fraction Ubit ExponentSize FractionSize
        public BitMask UnumBits { get; private set; }

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

        public Unum(UnumMetadata metadata, BitMask wholeUnum)
        {
            _metadata = metadata;

            UnumBits = wholeUnum;
        }

        // This doesn't work for all cases yet.
        //public Unum(UnumMetadata metadata, float number)
        //{
        //    _metadata = metadata;

        //    // Handling special cases first.
        //    if (float.IsNaN(number))
        //    {
        //        UnumBits = _metadata.QuietNotANumber;
        //        return;
        //    }
        //    if (float.IsPositiveInfinity(number))
        //    {
        //        UnumBits = _metadata.PositiveInfinity;
        //        return;
        //    }
        //    if (float.IsNegativeInfinity(number))
        //    {
        //        UnumBits = _metadata.NegativeInfinity;
        //        return;
        //    }


        //    UnumBits = new BitMask(_metadata.Size);
        //    var floatExponentBits = (BitConverter.ToUInt32(BitConverter.GetBytes(number), 0) << 1) >> 24;

        //    // These are the only uncertain cases that we can safely handle without Ubounds.
        //    if (ExponentSizeMax < ExponentValueToExponentSize((int)floatExponentBits - 127))
        //    {
        //        // The exponent is too big, so we express the number as the largest possible signed value,
        //        // but the Unum is uncertain, meaning that it's finite, but too big to express.
        //        if (floatExponentBits - 127 > 0)
        //            UnumBits = IsPositive() ? LargestPositive : LargestNegative;
        //        else // If the exponent is too small, we will handle it as a signed uncertain zero.
        //        {
        //            UnumBits = new BitMask(Size);
        //            if (!IsPositive()) Negate();
        //        }

        //        SetUncertainityBit(true);

        //        return;
        //    }


        //    var floatFractionBits = (BitConverter.ToUInt32(BitConverter.GetBytes(number), 0) << 9) >> 9;
        //    uint resultFractionSize = 23;
        //    uint floatFractionBitsSize = 23;

        //    if (floatFractionBits == 0) resultFractionSize = 0;
        //    else
        //        while (floatFractionBits % 2 == 0)
        //        {
        //            resultFractionSize -= 1;
        //            floatFractionBits >>= 1;
        //            floatFractionBitsSize = resultFractionSize;
        //        }


        //    var uncertainty = false;

        //    if (FractionSizeMax + 1 < resultFractionSize)
        //    {
        //        resultFractionSize = ((uint)FractionSizeMax - 1);
        //        uncertainty = true;
        //    }
        //    else if (resultFractionSize > 0) resultFractionSize = (resultFractionSize - 1);

        //    var resultFraction = uncertainty ?
        //        new BitMask(new uint[] { floatFractionBits >> (int)floatFractionBitsSize - FractionSizeMax }, Size) :
        //        new BitMask(new uint[] { floatFractionBits }, Size);
        //    var resultExponent = ExponentValueToExponentBits((int)(floatExponentBits - 127), Size);
        //    var floatBits = BitConverter.ToUInt32(BitConverter.GetBytes(number), 0);
        //    var resultSignBit = (floatBits > uint.MaxValue / 2);
        //    var resultExponentSize = (ExponentValueToExponentSize((int)floatExponentBits - 127) - 1);


        //    SetUnumBits(resultSignBit, resultExponent, resultFraction,
        //        uncertainty, resultExponentSize, resultFractionSize);
        //}

        public Unum(UnumMetadata metadata, uint value)
        {
            _metadata = metadata;
            UnumBits = _metadata.EmptyBitMask;

            if (value == 0) return;


            UnumBits += value; // The Fraction will be stored here.
            var exponentValue = UnumBits.FindLeadingOne() - 1;
            var exponent = new BitMask(new uint[] { exponentValue }, _metadata.Size);
            var exponentSize = exponent.FindLeadingOne();
            if (exponentValue > (1 << (int)exponentSize - 1)) exponentSize++;
            var bias = (1 << (int)(exponentSize - 1)) - 1;
            exponent += (uint)bias;

            UnumBits = BitMask.ShiftToRightEnd(UnumBits);
            var fractionSize = UnumBits.FindLeadingOne() - 1;
            if (fractionSize > 0) fractionSize -= 1;
            if (exponentValue != 0) UnumBits = BitMask.SetZero(UnumBits, UnumBits.FindLeadingOne() - 1);


            UnumBits = SetUnumBits(false, exponent, UnumBits, false, exponentSize - 1, fractionSize);
        }

        public Unum(UnumMetadata metadata, uint[] input)
        {
            _metadata = metadata;
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


            // Calcuating Exponent value and size.  
            var exponentValue = UnumBits.FindLeadingOne() - 1;
            var exponentSize = 0;
            var j = 1;

            while (j < exponentValue)
            {
                j <<= 1;
                exponentSize++;
            }

            if (exponentValue > (1 << exponentSize - 1)) exponentSize++;
            var bias = (1 << (exponentSize - 1)) - 1;
            exponentValue += (uint)bias;
            var exponentMask = _metadata.EmptyBitMask + exponentValue;
            if (exponentSize > 0) exponentSize -= 1; // Until now we needed the value, now we need the notation.


            // Calculating Fraction.
            UnumBits = BitMask.ShiftToRightEnd(UnumBits);
            var fractionSize = UnumBits.FindLeadingOne() - 1;
            if (fractionSize > 0) fractionSize -= 1;
            if (exponentValue > 0) UnumBits = BitMask.SetZero(UnumBits, UnumBits.FindLeadingOne() - 1);


            UnumBits = SetUnumBits(signBit, exponentMask, UnumBits, false, (uint)exponentSize, fractionSize);
        }

        public Unum(UnumMetadata metadata, int value)
        {
            _metadata = metadata;

            if (value >= 0) UnumBits = new Unum(metadata, (uint)value).UnumBits;
            else
            {
                UnumBits = new Unum(metadata, (uint)-value).UnumBits;
                UnumBits = Negate().UnumBits;
            }
        }

        // This doesn't work for all cases yet.
        //public Unum(UnumMetadata metadata, double x)
        //{
        //    _metadata = metadata;
        //    UnumBits = _metadata.EmptyBitMask;

        //    // Handling special cases first.
        //    if (double.IsNaN(x))
        //    {
        //        UnumBits = QuietNotANumber;
        //        return;
        //    }
        //    if (double.IsPositiveInfinity(x))
        //    {
        //        UnumBits = PositiveInfinity;
        //        return;
        //    }
        //    if (double.IsNegativeInfinity(x))
        //    {
        //        UnumBits = NegativeInfinity;
        //        return;
        //    }


        //    var doubleBits = BitConverter.ToUInt64(BitConverter.GetBytes(x), 0);
        //    SetSignBit((doubleBits > ulong.MaxValue / 2));


        //    var doubleFractionBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 12) >> 12;
        //    uint resultFractionSize = 52;

        //    if (doubleFractionBits == 0) resultFractionSize = 0;
        //    else
        //    {
        //        while (doubleFractionBits % 2 == 0)
        //        {
        //            resultFractionSize -= 1;
        //            doubleFractionBits >>= 1;
        //        }

        //    }


        //    var uncertainty = false;

        //    if (FractionSizeMax < resultFractionSize - 1)
        //    {
        //        SetFractionSizeBits((uint)(FractionSizeMax - 1));
        //        uncertainty = true;
        //    }
        //    else SetFractionSizeBits(resultFractionSize - 1);


        //    var doubleExponentBits = (BitConverter.ToUInt64(BitConverter.GetBytes(x), 0) << 1) >> 53;

        //    // These are the only uncertain cases that we can safely handle without Ubounds.
        //    if (ExponentSizeMax < ExponentValueToExponentSize((int)doubleExponentBits - 1023))
        //    {
        //        // The exponent is too big, so we express the number as the largest possible signed value,
        //        // but the Unum is uncertain, meaning that it's finite, but too big to express.
        //        if (doubleExponentBits - 1023 > 0)
        //            UnumBits = IsPositive() ? LargestPositive : LargestNegative;
        //        else // If the exponent is too small, we will handle it as a signed uncertain zero.
        //        {
        //            UnumBits = _metadata.EmptyBitMask;
        //            if (!IsPositive()) Negate();
        //        }

        //        SetUncertainityBit(true);

        //        return;
        //    }


        //    var exponentSizeBits = ExponentValueToExponentSize((int)doubleExponentBits - 1023) - 1;
        //    SetExponentSizeBits(exponentSizeBits);

        //    var doubleFraction = new uint[2];
        //    doubleFraction[0] = (uint)((doubleFractionBits << 32) >> 32);
        //    doubleFraction[1] = (uint)((doubleFractionBits >> 32));

        //    if (uncertainty)
        //    {
        //        SetFractionBits(Size > 32 ?
        //            // This is necessary because Hastlayer enables only one size of BitMasks.
        //            new BitMask(doubleFraction, Size) >> ((int)resultFractionSize - (int)FractionSize()) :
        //            // The lower 32 bits wouldn't fit in anyway.
        //            new BitMask(new uint[] { doubleFraction[1] }, Size) >> ((int)resultFractionSize - FractionSizeMax));

        //        SetUncertainityBit(true);
        //    }
        //    else
        //        SetFractionBits(Size > 32 ?
        //            // This is necessary because Hastlayer enables only one size of BitMasks.
        //            new BitMask(doubleFraction, Size) :
        //            // The lower 32 bits wouldn't fit in anyway.
        //            new BitMask(new uint[] { doubleFraction[1] }, Size));


        //    SetExponentBits(ExponentValueToExponentBits((int)(doubleExponentBits - 1023), Size));
        //}

        #endregion

        #region Methods to set the values of individual Unum structure elements

        public BitMask SetUnumBits(bool signBit, BitMask exponent, BitMask fraction,
            bool uncertainityBit, uint exponentSize, uint fractionSize)
        {
            var wholeUnum = _metadata.EmptyBitMask;

            wholeUnum = _metadata.FractionSizeMask & new BitMask(new uint[] { fractionSize }, _metadata.Size);
            wholeUnum = wholeUnum | (new BitMask(new uint[] { exponentSize }, _metadata.Size) << _metadata.FractionSizeSize);

            if (uncertainityBit) wholeUnum = wholeUnum | _metadata.UncertaintyBitMask;

            wholeUnum = wholeUnum | (fraction << _metadata.FractionSizeSize + _metadata.ExponentSizeSize + 1);
            wholeUnum = wholeUnum | (exponent << (int)(_metadata.FractionSizeSize + _metadata.ExponentSizeSize + 1 + fractionSize + 1));

            if (signBit) wholeUnum = wholeUnum | _metadata.SignBitMask;

            return UnumBits = wholeUnum;
        }


        public BitMask SetSignBit(bool signBit)
        {
            return UnumBits = signBit ? UnumBits | _metadata.SignBitMask : UnumBits & (new BitMask(_metadata.Size, true) ^ (_metadata.SignBitMask));
        }

        public BitMask SetUncertainityBit(bool uncertainityBit)
        {
            return UnumBits = uncertainityBit ? UnumBits | _metadata.UncertaintyBitMask : UnumBits & (~_metadata.UncertaintyBitMask);
        }

        public BitMask SetExponentBits(BitMask exponent)
        {
            return UnumBits = (UnumBits & (new BitMask(_metadata.Size, true) ^ ExponentMask())) |
                        (exponent << (int)(_metadata.FractionSizeSize + _metadata.ExponentSizeSize + 1 + FractionSize()));
        }

        public BitMask SetFractionBits(BitMask fraction)
        {
            return UnumBits = (UnumBits & (new BitMask(_metadata.Size, true) ^ FractionMask())) | (fraction << _metadata.FractionSizeSize + _metadata.ExponentSizeSize + 1);
        }

        public BitMask SetFractionSizeBits(uint fractionSize)
        {
            return UnumBits = (UnumBits & (new BitMask(_metadata.Size, true) ^ _metadata.FractionSizeMask)) | new BitMask(new uint[] { fractionSize }, _metadata.Size);
        }

        public BitMask SetExponentSizeBits(uint exponentSize)
        {
           return UnumBits = (UnumBits & (new BitMask(_metadata.Size, true) ^ _metadata.ExponentSizeMask) |
                       (new BitMask(new uint[] { exponentSize }, _metadata.Size) << _metadata.FractionSizeSize));
        }

        #endregion

        #region Binary data extraction

        public uint[] FractionToUintArray()
        {
            var resultMask = FractionWithHiddenBit() << ExponentValueWithBias() - (int)FractionSize();
            var result = new uint[resultMask.SegmentCount];

            for (var i = 0; i < resultMask.SegmentCount; i++) result[i] = resultMask.Segments[i];
            if (!IsPositive()) result[resultMask.SegmentCount - 1] |= 0x80000000;
            else
            {
                result[resultMask.SegmentCount - 1] <<= 1;
                result[resultMask.SegmentCount - 1] >>= 1;
            }

            return result;
        }

        #endregion

        #region Binary data manipulation

        public Unum Negate()
        {
            UnumBits ^= _metadata.SignBitMask;
            return this;
        }

        #endregion

        #region Unum numeric states

        public bool IsExact() => (UnumBits & _metadata.UncertaintyBitMask) == _metadata.EmptyBitMask;

        public bool IsPositive() => (UnumBits & _metadata.SignBitMask) == _metadata.EmptyBitMask;

        public bool IsZero() =>
            (UnumBits & _metadata.UncertaintyBitMask) == _metadata.EmptyBitMask &&
            (UnumBits & FractionMask()) == _metadata.EmptyBitMask &&
            (UnumBits & ExponentMask()) == _metadata.EmptyBitMask;

        #endregion

        #region  Methods for Utag independent Masks and values

        // This limits the ExponentSizeSize to 32, which is so enormous that it shouldn't be a problem.
        public uint ExponentSize() => (((UnumBits & _metadata.ExponentSizeMask) >> _metadata.FractionSizeSize) + 1).GetLowest32Bits();

        // This limits the FractionSizeSize to 32, which is so enormous that it shouldn't be a problem.
        public uint FractionSize() => ((UnumBits & _metadata.FractionSizeMask) + 1).GetLowest32Bits();

        public BitMask FractionMask()
        {
            var fractionMask = new BitMask(new uint[] { 1 }, _metadata.Size);
            return (fractionMask << (int)FractionSize());
        }

        public BitMask ExponentMask()
        {
            var exponentMask = new BitMask(new uint[] { 1 }, _metadata.Size);
            return ((exponentMask << (int)ExponentSize()) - 1) << (int)(FractionSize() + _metadata.UnumTagSize);
        }

        #endregion

        #region Methods for Utag dependent Masks and values

        public BitMask Exponent() => (ExponentMask() & UnumBits) >> (int)(_metadata.UnumTagSize + FractionSize());

        public BitMask Fraction() => (FractionMask() & UnumBits) >> _metadata.UnumTagSize;

        public BitMask FractionWithHiddenBit() =>
            HiddenBitIsOne() ? BitMask.SetOne(Fraction(), FractionSize()) : Fraction();

        public uint FractionSizeWithHiddenBit() => HiddenBitIsOne() ? FractionSize() + 1 : FractionSize();

        public int Bias() => (1 << (int)(ExponentSize() - 1)) - 1;

        public bool HiddenBitIsOne() => Exponent().GetLowest32Bits() > 0;

        public int ExponentValueWithBias() => (int)Exponent().GetLowest32Bits() - Bias() + (HiddenBitIsOne() ? 0 : 1);

        public bool IsNan() => UnumBits == _metadata.SignalingNotANumber || UnumBits == _metadata.QuietNotANumber;

        public bool IsPositiveInfinity() => UnumBits == _metadata.PositiveInfinity;

        public bool IsNegativeInfinity() => UnumBits == _metadata.NegativeInfinity;

        #endregion

        #region Operations for exact Unums

        public static Unum AddExactUnums(Unum left, Unum right)
        {
            var scratchPad = left._metadata.EmptyBitMask; // It could be only FractionSizeMax +2 long if Hastlayer enabled it.

            // Handling special cases first.
            if (left.IsNan() || right.IsNan())
                return new Unum(left._metadata, left._metadata.QuietNotANumber);

            if ((left.IsPositiveInfinity() && right.IsNegativeInfinity()) ||
                (left.IsNegativeInfinity() && right.IsPositiveInfinity()))
                return new Unum(left._metadata, left._metadata.QuietNotANumber);

            if (left.IsPositiveInfinity() || right.IsPositiveInfinity())
                return new Unum(left._metadata, left._metadata.PositiveInfinity);

            if (left.IsNegativeInfinity() || right.IsNegativeInfinity())
                return new Unum(left._metadata, left._metadata.NegativeInfinity);


            // Using the metadata properties directly for now, as custom properties like left.ExponentSizeSize are not
            // yet supported.
            var resultExponentSizeSize = left._metadata.ExponentSizeSize;
            var resultFractionSizeSize = left._metadata.FractionSizeSize;
            var resultUnum = new Unum(resultExponentSizeSize, resultFractionSizeSize);

            var exponentValueDifference = left.ExponentValueWithBias() - right.ExponentValueWithBias();
            var signBitsMatch = left.IsPositive() == right.IsPositive();
            var resultSignBit = false;
            var biggerBitsMovedToLeft = 0;
            var smallerBitsMovedToLeft = 0;
            var resultExponentValue = 0;


            if (exponentValueDifference == 0) // Exponents are equal.
            {
                resultExponentValue = left.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum._metadata.FractionSizeMax + 1 - (left.FractionSize() + 1));
                smallerBitsMovedToLeft = (int)(resultUnum._metadata.FractionSizeMax + 1 - (right.FractionSize() + 1));
                scratchPad = AddAlignedFractions(
                    left.FractionWithHiddenBit() << biggerBitsMovedToLeft,
                    right.FractionWithHiddenBit() << smallerBitsMovedToLeft,
                    signBitsMatch);

                if (!signBitsMatch)
                    resultSignBit = left.FractionWithHiddenBit() >= right.FractionWithHiddenBit() ?
                        !left.IsPositive() : // Left Fraction is bigger.
                        !right.IsPositive(); // Right Fraction is bigger.

            }
            else if (exponentValueDifference > 0) // Left Exponent is bigger.
            {
                resultSignBit = !left.IsPositive();
                resultExponentValue = left.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum._metadata.FractionSizeMax + 1 - (left.FractionSize() + 1));
                smallerBitsMovedToLeft = (int)
                    (resultUnum._metadata.FractionSizeMax + 1 - (right.FractionSize() + 1) - exponentValueDifference);

                scratchPad = left.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad,
                    right.FractionWithHiddenBit() << smallerBitsMovedToLeft, signBitsMatch);
            }
            else // Right Exponent is bigger.
            {
                resultSignBit = !right.IsPositive();
                resultExponentValue = right.ExponentValueWithBias();
                biggerBitsMovedToLeft = (int)(resultUnum._metadata.FractionSizeMax + 1 - (right.FractionSize() + 1));
                smallerBitsMovedToLeft = (int)
                    (resultUnum._metadata.FractionSizeMax + 1 - (left.FractionSize() + 1) + exponentValueDifference);

                scratchPad = right.FractionWithHiddenBit() << biggerBitsMovedToLeft;
                scratchPad = AddAlignedFractions(scratchPad,
                    left.FractionWithHiddenBit() << smallerBitsMovedToLeft, signBitsMatch);
            }


            var exponentChange = (int)scratchPad.FindLeadingOne() - (resultUnum._metadata.FractionSizeMax + 1);
            var resultExponent = left._metadata.EmptyBitMask +
                ExponentValueToExponentBits(resultExponentValue + exponentChange, left._metadata.Size);
            uint resultExponentSize = ExponentValueToExponentSize(resultExponentValue + exponentChange) - 1;

            var resultUbit = false;
            if (smallerBitsMovedToLeft < 0) resultUbit = true; // There are lost digits.
            else scratchPad = BitMask.ShiftToRightEnd(scratchPad);

            uint resultFractionSize = 0;

            if (scratchPad.FindLeadingOne() == 0)
            {
                resultExponent = scratchPad; // 0
                resultExponentSize = 0;
            }
            else resultFractionSize = scratchPad.FindLeadingOne() - 1;


            if (resultExponent.FindLeadingOne() != 0) // Erease hidden bit if it exists.
            {
                scratchPad = BitMask.SetZero(scratchPad, scratchPad.FindLeadingOne() - 1);
                resultFractionSize = resultFractionSize == 0 ? 0 : resultFractionSize - 1;
            }

            // This is temporary, for the imitation of float behaviour. Now the ubit works as a flag for rounded values.
            if ((!left.IsExact()) || (!right.IsExact())) resultUbit = true;

            resultUnum.UnumBits = resultUnum.SetUnumBits(resultSignBit, resultExponent, scratchPad, resultUbit, resultExponentSize,
                resultFractionSize);

            return resultUnum;
        }

        public static Unum SubtractExactUnums(Unum left, Unum right) => AddExactUnums(left, NegateExactUnum(right));

        public static Unum NegateExactUnum(Unum input)
        {
            input = input.Negate();
            return input;
        }

        public static bool AreEqualExactUnums(Unum left, Unum right) =>
            left.IsZero() && right.IsZero() ? true : left.UnumBits == right.UnumBits;


        #endregion

        #region Helper methods for operations and conversions

        public static BitMask ExponentValueToExponentBits(int value, uint size)
        {
            if (value > 0)
            {
                var exponent = new BitMask(new uint[] { (uint)value }, size);
                var exponentSize = ExponentValueToExponentSize(value);
                exponent += (uint)(1 << (int)(exponentSize - 1)) - 1; // Applying bias.

                return exponent;
            }
            else
            {
                var exponent = new BitMask(new uint[] { (uint)-value }, size);
                var exponentSize = ExponentValueToExponentSize(value);
                exponent += (uint)(1 << (int)(exponentSize - 1)) - 1; // Applying bias.
                exponent -= (uint)(-2 * value);

                return exponent;
            }
        }

        public static uint ExponentValueToExponentSize(int value)
        {
            uint size = 1;

            if (value > 0) while (value > 1 << (int)(size - 1)) size++;
            else while (-value >= 1 << (int)(size - 1)) size++;

            return size;
        }

        public static BitMask AddAlignedFractions(BitMask left, BitMask right, bool signBitsMatch)
        {
            if (signBitsMatch) left += right;
            else left = left > right ? left - right : right - left;

            return left;
        }

        #endregion

        #region Operators

        public static Unum operator +(Unum left, Unum right) => AddExactUnums(left, right);

        public static Unum operator -(Unum x) => NegateExactUnum(x);

        public static Unum operator -(Unum left, Unum right) => SubtractExactUnums(left, right);

        //public static Unum operator *(Unum left, Unum right)
        //{
        //    if (left.IsExact() && right.IsExact()) return MultiplyExactUnums(left, right);

        //    return new Unum();
        //}

        //public static Unum operator /(Unum left, Unum right)
        //{

        //}

        public static bool operator ==(Unum left, Unum right) => AreEqualExactUnums(left, right);

        public static bool operator !=(Unum left, Unum right) => !(left == right);

        //public static bool operator <(Unum left, Unum right)
        // {
        //     if (left.IsPositive() != right.IsPositive()) return left.IsPositive();
        //     if (left.ExponentValueWithBias() > right.ExponentValueWithBias()) return left.IsPositive();
        //     if (left.ExponentValueWithBias() < right.ExponentValueWithBias()) return right.IsPositive();
        //     // if (left.FractionWithHiddenBit())

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

        //Converting from an Unum to int results in information loss, so only allowing it explicitly (with a cast).
        public static explicit operator int(Unum x)
        {
            uint result;

            if ((x.ExponentValueWithBias() + (int)x.FractionSizeWithHiddenBit()) < 31) //The Unum fits into the range.
                result = (x.FractionWithHiddenBit() << x.ExponentValueWithBias() - (int)x.FractionSize()).GetLowest32Bits();
            else return (x.IsPositive()) ? int.MaxValue : int.MinValue; // The absolute value of the Unum is too large.

            return x.IsPositive() ? (int)result : -(int)result;
        }

        public static explicit operator uint(Unum x) =>
            (x.FractionWithHiddenBit() << x.ExponentValueWithBias() - ((int)x.FractionSize())).GetLowest32Bits();

        // This is not well tested yet.
        public static explicit operator float(Unum x)
        {
            // Handling special cases first.
            if (x.IsNan()) return float.NaN;
            if (x.IsNegativeInfinity()) return float.NegativeInfinity;
            if (x.IsPositiveInfinity()) return float.PositiveInfinity;
            if (x.ExponentValueWithBias() > 127) // Exponent is too big for float format.
                return (x.IsPositive()) ? float.PositiveInfinity : float.NegativeInfinity;
            if (x.ExponentValueWithBias() < -126) return (x.IsPositive()) ? 0 : -0; // Exponent is too small for float format.

            var result = (x.Fraction() << 23 - ((int)x.FractionSize())).GetLowest32Bits();
            result |= (uint)(x.ExponentValueWithBias() + 127) << 23;

            return x.IsPositive() ?
                BitConverter.ToSingle(BitConverter.GetBytes(result), 0) :
                -BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
        }

        #endregion

        #region Overrides
        public override bool Equals(object obj) => base.Equals(obj);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => base.ToString();

        #endregion
    }
}
