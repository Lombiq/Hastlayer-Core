namespace Hast.Samples.SampleAssembly.Services.Lzma
{
    internal class BitTreeEncoder
	{
		private BitEncoder[] _models;
		private int _numBitLevels;


		public BitTreeEncoder(int numBitLevels)
		{
			_numBitLevels = numBitLevels;
			_models = new BitEncoder[1 << numBitLevels];
		}


		public void Init(uint[] probPrices)
		{
            for (var i = 1; i < (1 << _numBitLevels); i++)
            {
                _models[i] = new BitEncoder();
                _models[i].Init(probPrices);
            }
		}

		public void Encode(RangeEncoder rangeEncoder, uint symbol)
		{
			uint m = 1;
            var bitIndex = _numBitLevels;
            while (bitIndex > 0)
			{
				bitIndex--;
				uint bit = (symbol >> bitIndex) & 1;
				_models[m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
			}
		}

		public void ReverseEncode(RangeEncoder rangeEncoder, uint symbol)
		{
			uint m = 1;
			for (var i = 0; i < _numBitLevels; i++)
			{
				var bit = symbol & 1;
				_models[m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
				symbol >>= 1;
			}
		}

		public uint GetPrice(uint symbol)
		{
			uint price = 0;
			uint m = 1;
            var bitIndex = _numBitLevels;
            while (bitIndex > 0)
            {
                bitIndex--;
				var bit = (symbol >> bitIndex) & 1;
				price += _models[m].GetPrice(bit);
				m = (m << 1) + bit;
			}

			return price;
		}

		public uint ReverseGetPrice(uint symbol)
		{
			uint price = 0;
			uint m = 1;
			for (var i = _numBitLevels; i > 0; i--)
			{
				var bit = symbol & 1;
				symbol >>= 1;
				price += _models[m].GetPrice(bit);
				m = (m << 1) | bit;
			}

			return price;
		}

		public static uint ReverseGetPrice(
            BitEncoder[] Models, 
            uint startIndex, 
            int numBitLevels, 
            uint symbol)
		{
			uint price = 0;
			uint m = 1;
			for (var i = numBitLevels; i > 0; i--)
			{
				uint bit = symbol & 1;
				symbol >>= 1;
				price += Models[startIndex + m].GetPrice(bit);
				m = (m << 1) | bit;
			}

			return price;
		}

		public static void ReverseEncode(
            BitEncoder[] Models, 
            uint startIndex,
			RangeEncoder rangeEncoder, 
            int numBitLevels, 
            uint symbol)
		{
			uint m = 1;
			for (var i = 0; i < numBitLevels; i++)
			{
				var bit = symbol & 1;
				Models[startIndex + m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
				symbol >>= 1;
			}
		}
	}
}
