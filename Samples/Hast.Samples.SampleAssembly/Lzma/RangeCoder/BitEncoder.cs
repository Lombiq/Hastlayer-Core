using Hast.Samples.SampleAssembly.Services.Lzma.Constants;

namespace Hast.Samples.SampleAssembly.Services.Lzma.RangeCoder
{
    internal class BitEncoder
    {
        private const int NumMoveBits = 5;
        public const int NumMoveReducingBits = 2;
        private uint[] ProbPrices;


        private uint _prob;


        public BitEncoder()
        {
        }


        public void Init(uint[] probPrices)
        {
            ProbPrices = probPrices;
            _prob = RangeEncoderConstants.BitModelTotal >> 1;
        }

        public void UpdateModel(uint symbol)
        {
            if (symbol == 0) _prob += (RangeEncoderConstants.BitModelTotal - _prob) >> NumMoveBits;
            else _prob -= (_prob) >> NumMoveBits;
        }

        public void Encode(RangeEncoder encoder, uint symbol)
        {
            var newBound = (encoder.Range >> RangeEncoderConstants.NumBitModelTotalBits) * _prob;

            if (symbol == 0)
            {
                encoder.Range = newBound;
                _prob += (RangeEncoderConstants.BitModelTotal - _prob) >> NumMoveBits;
            }
            else
            {
                encoder.Low += newBound;
                encoder.Range -= newBound;
                _prob -= (_prob) >> NumMoveBits;
            }

            if (encoder.Range < RangeEncoderConstants.TopValue)
            {
                encoder.Range <<= 8;
                encoder.ShiftLow();
            }
        }

        public uint GetPrice(uint symbol)
        {
            var a = _prob - symbol;
            var b = -1 * ((int)symbol);
            var c = RangeEncoderConstants.BitModelTotal - 1;
            var d = (a ^ b) & c;
            var e = (int)(d >> NumMoveReducingBits);

            return ProbPrices[e];
        }

        public uint GetPrice0() =>
            ProbPrices[_prob >> NumMoveReducingBits];

        public uint GetPrice1()
        {
            var i = (int)(RangeEncoderConstants.BitModelTotal - _prob);
            return ProbPrices[i >> NumMoveReducingBits];
        }
    }
}
