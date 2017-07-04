using Hast.Samples.SampleAssembly.Models;

namespace Hast.Samples.SampleAssembly.Services.Lzma
{
    public class LzInWindow
    {
        private SimpleMemoryStream _stream;
        private uint _posLimit; // offset (from _buffer) of first byte when new block reading must be done
        private bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream
        private uint _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        private uint _keepSizeAfter; // how many BYTEs must be kept buffer after _pos
        private uint _pointerToLastSafePosition;


        protected byte[] _bufferBase = null; // pointer to buffer with data
        protected uint _blockSize; // Size of Allocated memory block
        protected uint _pos; // offset (from _buffer) of curent byte
        protected uint _streamPos; // offset (from _buffer) of first not read byte from Stream
        protected uint _bufferOffset;
        

		public void MoveBlock()
		{
			var offset = _bufferOffset + _pos - _keepSizeBefore;
			// we need one additional byte, since MovePos moves on 1 byte.

			if (offset > 0) offset--;
			
			var numbytes = _bufferOffset + _streamPos - offset;

			// check negative offset ????
			for (uint i = 0; i < numbytes; i++)
				_bufferBase[i] = _bufferBase[offset + i];

			_bufferOffset -= offset;
		}

		public virtual void ReadBlock()
		{
			if (_streamEndWasReached) return;

			while (true)
			{
				var size = (int)((0 - _bufferOffset) + _blockSize - _streamPos);
				if (size == 0) return;

				var numReadbytes = _stream.Read(_bufferBase, (int)(_bufferOffset + _streamPos), size);
				if (numReadbytes == 0)
				{
					_posLimit = _streamPos;
					var pointerToPostion = _bufferOffset + _posLimit;
					if (pointerToPostion > _pointerToLastSafePosition)
						_posLimit = _pointerToLastSafePosition - _bufferOffset;

					_streamEndWasReached = true;

					return;
				}

				_streamPos += (uint)numReadbytes;

				if (_streamPos >= _pos + _keepSizeAfter)
					_posLimit = _streamPos - _keepSizeAfter;
			}
		}

		public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
		{
			_keepSizeBefore = keepSizeBefore;
			_keepSizeAfter = keepSizeAfter;

			var blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;

			if (_bufferBase == null || _blockSize != blockSize)
			{
				Free();
				_blockSize = blockSize;
				_bufferBase = new byte[_blockSize];
			}

			_pointerToLastSafePosition = _blockSize - keepSizeAfter;
		}

		public void SetStream(SimpleMemoryStream stream) => 
            _stream = stream;

		public void ReleaseStream() =>
            _stream = null;

		public void Init()
		{
			_bufferOffset = 0;
			_pos = 0;
			_streamPos = 0;
			_streamEndWasReached = false;

			ReadBlock();
		}

		public void MovePos()
		{
			_pos++;
			if (_pos > _posLimit)
			{
				var pointerToPostion = _bufferOffset + _pos;

				if (pointerToPostion > _pointerToLastSafePosition)
					MoveBlock();

				ReadBlock();
			}
		}

		public byte GetIndexbyte(int index) => 
            _bufferBase[_bufferOffset + _pos + index];

		// index + limit have not to exceed _keepSizeAfter;
		public uint GetMatchLen(int index, uint distance, uint limit)
		{
            if (_streamEndWasReached)
            {
                if ((_pos + index) + limit > _streamPos)
                    limit = _streamPos - (uint)(_pos + index);
            }

			distance++;
			// byte *pby = _buffer + (size_t)_pos + index;
			uint pby = _bufferOffset + _pos + (uint)index;

			uint i;
            for (i = 0; i < limit && _bufferBase[pby + i] == _bufferBase[pby + i - distance]; i++) ;
            return i;
		}

		public uint GetNumAvailablebytes() =>
            _streamPos - _pos;

		public void ReduceOffsets(int subValue)
		{
			_bufferOffset += (uint)subValue;
			_posLimit -= (uint)subValue;
			_pos -= (uint)subValue;
			_streamPos -= (uint)subValue;
        }


        private void Free() =>
            _bufferBase = null;
    }
}
