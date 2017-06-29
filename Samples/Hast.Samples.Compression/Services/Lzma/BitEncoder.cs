namespace Hast.Samples.Compression.Services.Lzma
{
    struct BitEncoder
    {
        private const int KNumMoveBits = 5;
        private const int KNumMoveReducingBits = 2;
        private static uint[] ProbPrices = new uint[RangeEncoderConstants.KBitModelTotal >> KNumMoveReducingBits];


        private uint _prob;
        

        static BitEncoder()
        {
            const int kNumBits = (RangeEncoderConstants.KNumBitModelTotalBits - KNumMoveReducingBits);
            for (int i = kNumBits - 1; i >= 0; i--)
            {
                var start = (uint)1 << (kNumBits - i - 1);
                var end = (uint)1 << (kNumBits - i);
                for (var j = start; j < end; j++)
                {
                    ProbPrices[j] = ((uint)i << RangeEncoderConstants.KNumBitPriceShiftBits) +
                        (((end - j) << RangeEncoderConstants.KNumBitPriceShiftBits) >> (kNumBits - i - 1));
                }
            }
        }


        public void Init() => _prob = RangeEncoderConstants.KBitModelTotal >> 1;

        public void UpdateModel(uint symbol)
        {
            if (symbol == 0) _prob += (RangeEncoderConstants.KBitModelTotal - _prob) >> KNumMoveBits;
            else _prob -= (_prob) >> KNumMoveBits;
        }

        public void Encode(RangeEncoder encoder, uint symbol)
        {
            var newBound = (encoder.Range >> RangeEncoderConstants.KNumBitModelTotalBits) * _prob;

            if (symbol == 0)
            {
                encoder.Range = newBound;
                _prob += (RangeEncoderConstants.KBitModelTotal - _prob) >> KNumMoveBits;
            }
            else
            {
                encoder.Low += newBound;
                encoder.Range -= newBound;
                _prob -= (_prob) >> KNumMoveBits;
            }

            if (encoder.Range < RangeEncoderConstants.KTopValue)
            {
                encoder.Range <<= 8;
                encoder.ShiftLow();
            }
        }

        public uint GetPrice(uint symbol) =>
            ProbPrices[(((_prob - symbol) ^ ((-(int)symbol))) & (RangeEncoderConstants.KBitModelTotal - 1)) >> KNumMoveReducingBits];

        public uint GetPrice0() =>
            ProbPrices[_prob >> KNumMoveReducingBits];

        public uint GetPrice1() =>
            ProbPrices[(RangeEncoderConstants.KBitModelTotal - _prob) >> KNumMoveReducingBits];
    }
}
