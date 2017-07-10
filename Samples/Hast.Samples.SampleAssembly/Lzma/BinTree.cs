using Hast.Samples.SampleAssembly.Models;
using Hast.Samples.SampleAssembly.Lzma.Constants;
using Hast.Samples.SampleAssembly.Lzma.Helpers;
using System;

namespace Hast.Samples.SampleAssembly.Lzma
{
    // Part of the LZ compressor.
    public class BinTree
    {
        private const uint Hash2Size = 1 << 10;
        private const uint Hash3Size = 1 << 16;
        private const uint BT2HashSize = 1 << 16;
        private const uint StartMaxLen = 1;
        private const uint Hash3Offset = Hash2Size;
        private const uint EmptyHashValue = 0;
        private const uint MaxValForNormalize = ((uint)1 << 31) - 1;


        private uint _cyclicBufferPos;
        private uint _cyclicBufferSize = 0;
        private uint _matchMaxLen;
        private uint[] _son;
        private uint[] _hash;
        private uint _cutValue = 0xFF;
        private uint _hashMask;
        private uint _hashSizeSum = 0;
        private bool _hashArray = true;
        private uint _kNumHashDirectbytes = 0;
        private uint _kMinMatchCheck = 4;
        private uint _kFixHashSize = Hash2Size + Hash3Size;
        private CRC _crc;

        #region LzInWindow fields

        private SimpleMemoryStream _stream;
        private uint _posLimit; // offset (from _buffer) of first byte when new block reading must be done
        private bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream
        private uint _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        private uint _keepSizeAfter; // how many BYTEs must be kept buffer after _pos
        private uint _pointerToLastSafePosition;

        private byte[] _bufferBase = null; // pointer to buffer with data
        private uint _blockSize; // Size of Allocated memory block
        private uint _pos; // offset (from _buffer) of curent byte
        private uint _streamPos; // offset (from _buffer) of first not read byte from Stream
        private uint _bufferOffset;

        #endregion

        public void SetType(int numHashbytes)
        {
            _hashArray = (numHashbytes > 2);
            if (_hashArray)
            {
                _kNumHashDirectbytes = 0;
                _kMinMatchCheck = 4;
                _kFixHashSize = Hash2Size + Hash3Size;
            }
            else
            {
                _kNumHashDirectbytes = 2;
                _kMinMatchCheck = 2 + 1;
                _kFixHashSize = 0;
            }
        }
        
        public void SetStream(SimpleMemoryStream stream) =>
            _stream = stream;

        public void ReleaseStream() =>
            _stream = null;

        public void Init()
        {
            InitLzInWindow();

            _hash = new uint[BaseConstants.MaxHashSize];
            _son = new uint[(BaseConstants.MaxDictionarySize + 1) * 2];

            _crc = new CRC();
            for (var i = 0; i < _hashSizeSum; i++)
                _hash[i] = EmptyHashValue;
            _cyclicBufferPos = 0;
            ReduceOffsetsLzInWindow(-1);
        }

        public void MovePos()
        {
            if (++_cyclicBufferPos >= _cyclicBufferSize)
                _cyclicBufferPos = 0;
            MovePosLzInWindow();
            if (_pos == MaxValForNormalize)
                Normalize();
        }

        public byte GetIndexbyte(int index) =>
            _bufferBase[_bufferOffset + _pos + (uint)index];

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

        public void Create(
            uint dictionarySize,
            uint keepAddBufferBefore,
            uint matchMaxLen,
            uint keepAddBufferAfter)
        {
            // Should throw an Exception when it becomes supported.
            //if (historySize > MaxValForNormalize - 256) throw new Exception();

            _cutValue = 16 + (matchMaxLen >> 1);

            var windowReservSize = (dictionarySize + keepAddBufferBefore + matchMaxLen + keepAddBufferAfter) / 2 + 256;

            CreateLzInWindow(dictionarySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

            _matchMaxLen = matchMaxLen;

            // Dictionary size can be maximum 128 bytes!
            _cyclicBufferSize = dictionarySize + 1;

            _hashSizeSum = BT2HashSize;

            if (_hashArray)
            {
                _hashSizeSum = dictionarySize - 1;
                _hashSizeSum |= (_hashSizeSum >> 1);
                _hashSizeSum |= (_hashSizeSum >> 2);
                _hashSizeSum |= (_hashSizeSum >> 4);
                _hashSizeSum |= (_hashSizeSum >> 8);
                _hashSizeSum >>= 1;
                _hashSizeSum |= 0xFFFF;
                if (_hashSizeSum > (1 << 24))
                    _hashSizeSum >>= 1;
                _hashMask = _hashSizeSum;
                _hashSizeSum++;
                _hashSizeSum += _kFixHashSize;
            }
        }

        public uint GetMatches(uint[] distances)
        {
            uint lenLimit;
            if (_pos + _matchMaxLen <= _streamPos)
                lenLimit = _matchMaxLen;
            else
            {
                lenLimit = _streamPos - _pos;
                if (lenLimit < _kMinMatchCheck)
                {
                    MovePos();

                    return 0;
                }
            }

            uint offset = 0;
            var matchMinPos = (_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0;
            var cur = _bufferOffset + _pos;
            var maxLen = StartMaxLen; // to avoid items for len < hashSize;
            uint hashValue, hash2Value = 0, hash3Value = 0;

            if (_hashArray)
            {
                uint temp = _crc.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                hash2Value = temp & (Hash2Size - 1);
                temp ^= ((uint)(_bufferBase[cur + 2]) << 8);
                hash3Value = temp & (Hash3Size - 1);
                hashValue = (temp ^ (_crc.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
            }
            else
                hashValue = _bufferBase[cur] ^ ((uint)(_bufferBase[cur + 1]) << 8);

            var curMatch = _hash[_kFixHashSize + hashValue];

            if (_hashArray)
            {
                uint curMatch2 = _hash[hash2Value];
                uint curMatch3 = _hash[Hash3Offset + hash3Value];
                _hash[hash2Value] = _pos;
                _hash[Hash3Offset + hash3Value] = _pos;
                if (curMatch2 > matchMinPos)
                    if (_bufferBase[_bufferOffset + curMatch2] == _bufferBase[cur])
                    {
                        distances[offset++] = maxLen = 2;
                        distances[offset++] = _pos - curMatch2 - 1;
                    }
                if (curMatch3 > matchMinPos)
                    if (_bufferBase[_bufferOffset + curMatch3] == _bufferBase[cur])
                    {
                        if (curMatch3 == curMatch2)
                            offset -= 2;
                        distances[offset++] = maxLen = 3;
                        distances[offset++] = _pos - curMatch3 - 1;
                        curMatch2 = curMatch3;
                    }
                if (offset != 0 && curMatch2 == curMatch)
                {
                    offset -= 2;
                    maxLen = StartMaxLen;
                }
            }

            _hash[_kFixHashSize + hashValue] = _pos;

            var ptr0 = (_cyclicBufferPos << 1) + 1;
            var ptr1 = (_cyclicBufferPos << 1);

            uint len0, len1;
            len0 = len1 = _kNumHashDirectbytes;

            if (_kNumHashDirectbytes != 0)
            {
                if (curMatch > matchMinPos)
                {
                    if (_bufferBase[_bufferOffset + curMatch + _kNumHashDirectbytes] !=
                            _bufferBase[cur + _kNumHashDirectbytes])
                    {
                        distances[offset++] = maxLen = _kNumHashDirectbytes;
                        distances[offset++] = _pos - curMatch - 1;
                    }
                }
            }

            var count = _cutValue;

            var run = true;
            while (run)
            {
                if (curMatch <= matchMinPos || count == 0)
                {
                    _son[ptr0] = _son[ptr1] = EmptyHashValue;
                    run = false;
                }
                else
                {
                    var delta = _pos - curMatch;
                    var cyclicPos = ((delta <= _cyclicBufferPos) ?
                        (_cyclicBufferPos - delta) :
                        (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                    var pby1 = _bufferOffset + curMatch;
                    var len = LzmaHelpers.GetMinValue(len0, len1);
                    if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                    {
                        var run2 = true;
                        while (run2 && ++len != lenLimit)
                        {
                            if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                                run2 = false;
                        }

                        if (maxLen < len)
                        {
                            distances[offset++] = maxLen = len;
                            distances[offset++] = delta - 1;
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];

                                run = false;
                            }
                        }
                    }

                    if (run)
                    {
                        if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
                        {
                            _son[ptr1] = curMatch;
                            ptr1 = cyclicPos + 1;
                            curMatch = _son[ptr1];
                            len1 = len;
                        }
                        else
                        {
                            _son[ptr0] = curMatch;
                            ptr0 = cyclicPos;
                            curMatch = _son[ptr0];
                            len0 = len;
                        }
                    }
                }

                count--;
            }

            MovePos();

            return offset;
        }

        public void Skip(uint num)
        {
            do
            {
                var continueLoop = false;
                uint lenLimit;
                if (_pos + _matchMaxLen <= _streamPos)
                    lenLimit = _matchMaxLen;
                else
                {
                    lenLimit = _streamPos - _pos;
                    if (lenLimit < _kMinMatchCheck)
                    {
                        MovePos();
                        continueLoop = true;
                    }
                }

                if (!continueLoop)
                {
                    var matchMinPos = (_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0;
                    var cur = _bufferOffset + _pos;

                    uint hashValue;

                    if (_hashArray)
                    {
                        var temp = _crc.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                        var hash2Value = temp & (Hash2Size - 1);
                        _hash[hash2Value] = _pos;
                        temp ^= ((uint)(_bufferBase[cur + 2]) << 8);
                        var hash3Value = temp & (Hash3Size - 1);
                        _hash[Hash3Offset + hash3Value] = _pos;
                        hashValue = (temp ^ (_crc.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    }
                    else
                        hashValue = _bufferBase[cur] ^ ((uint)(_bufferBase[cur + 1]) << 8);

                    var curMatch = _hash[_kFixHashSize + hashValue];
                    _hash[_kFixHashSize + hashValue] = _pos;

                    var ptr0 = (_cyclicBufferPos << 1) + 1;
                    var ptr1 = (_cyclicBufferPos << 1);

                    uint len0, len1;
                    len0 = len1 = _kNumHashDirectbytes;

                    var count = _cutValue;
                    var run = true;
                    while (run)
                    {
                        if (curMatch <= matchMinPos || count == 0)
                        {
                            _son[ptr0] = _son[ptr1] = EmptyHashValue;

                            run = false;
                        }
                        else
                        {
                            var delta = _pos - curMatch;
                            var cyclicPos = ((delta <= _cyclicBufferPos) ?
                                (_cyclicBufferPos - delta) :
                                (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                            var pby1 = _bufferOffset + curMatch;
                            var len = LzmaHelpers.GetMinValue(len0, len1);
                            if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                            {
                                var run2 = true;
                                while (run2 && ++len != lenLimit)
                                {
                                    if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                                        run2 = false;
                                }

                                if (len == lenLimit)
                                {
                                    _son[ptr1] = _son[cyclicPos];
                                    _son[ptr0] = _son[cyclicPos + 1];

                                    run = false;
                                }
                            }

                            if (run)
                            {
                                if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
                                {
                                    _son[ptr1] = curMatch;
                                    ptr1 = cyclicPos + 1;
                                    curMatch = _son[ptr1];
                                    len1 = len;
                                }
                                else
                                {
                                    _son[ptr0] = curMatch;
                                    ptr0 = cyclicPos;
                                    curMatch = _son[ptr0];
                                    len0 = len;
                                }
                            }
                        }
                    }

                    count--;

                    MovePos();
                }
            }
            while (--num != 0);
        }

        public void SetCutValue(uint cutValue) =>
            _cutValue = cutValue;


        private void NormalizeSon(uint subValue)
        {
            for (uint i = 0; i < _cyclicBufferSize * 2; i++)
            {
                var value = _son[i];
                if (value <= subValue) value = EmptyHashValue;
                else value -= subValue;
                _son[i] = value;
            }
        }

        private void NormalizeHash(uint subValue)
        {
            for (uint i = 0; i < _hashSizeSum; i++)
            {
                var value = _hash[i];
                if (value <= subValue) value = EmptyHashValue;
                else value -= subValue;
                _hash[i] = value;
            }
        }

        private void Normalize()
        {
            uint subValue = _pos - _cyclicBufferSize;

            NormalizeSon(subValue);
            NormalizeHash(subValue);

            ReduceOffsetsLzInWindow((int)subValue);
        }

        #region LzInWindow Methods

        private void CreateLzInWindow(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
        {
            _keepSizeBefore = keepSizeBefore;
            _keepSizeAfter = keepSizeAfter;

            var blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;

            // Suppose that the _bufferBase has already been initialized (not null).
            _blockSize = blockSize;

            _pointerToLastSafePosition = _blockSize - keepSizeAfter;
        }

        private void InitLzInWindow()
        {
            _bufferBase = new byte[BaseConstants.MaxBlockLength];

            _bufferOffset = 0;
            _pos = 0;
            _streamPos = 0;
            _streamEndWasReached = false;

            ReadBlockLzInWindow();
        }

        private void ReadBlockLzInWindow()
        {
            if (!_streamEndWasReached)
            {
                var run = true;

                while (run)
                {
                    var size = (int)((0 - _bufferOffset) + _blockSize - _streamPos);
                    if (size == 0) run = false;
                    else
                    {
                        var numReadbytes = _stream.Read(_bufferBase, (int)(_bufferOffset + _streamPos), size);
                        if (numReadbytes == 0)
                        {
                            _posLimit = _streamPos;
                            var pointerToPostion = _bufferOffset + _posLimit;
                            if (pointerToPostion > _pointerToLastSafePosition)
                                _posLimit = _pointerToLastSafePosition - _bufferOffset;

                            _streamEndWasReached = true;

                            run = false;
                        }
                        else
                        {
                            _streamPos += (uint)numReadbytes;

                            if (_streamPos >= _pos + _keepSizeAfter)
                                _posLimit = _streamPos - _keepSizeAfter;
                        }
                    }
                }
            }
        }

        private void ReduceOffsetsLzInWindow(int subValue)
        {
            _bufferOffset += (uint)subValue;
            _posLimit -= (uint)subValue;
            _pos -= (uint)subValue;
            _streamPos -= (uint)subValue;
        }

        private void MovePosLzInWindow()
        {
            _pos++;
            if (_pos > _posLimit)
            {
                var pointerToPostion = _bufferOffset + _pos;

                if (pointerToPostion > _pointerToLastSafePosition)
                    MoveBlockLzInWindow();

                ReadBlockLzInWindow();
            }
        }

        private void MoveBlockLzInWindow()
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

        #endregion
    }
}
