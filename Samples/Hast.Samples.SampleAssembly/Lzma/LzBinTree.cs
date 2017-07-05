using Hast.Samples.SampleAssembly.Models;
using System;

namespace Hast.Samples.SampleAssembly.Services.Lzma
{
    public class BinTree : LzInWindow
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

        public new void SetStream(SimpleMemoryStream stream) =>
            base.SetStream(stream);

        public new void ReleaseStream() =>
            base.ReleaseStream();

        public new void Init()
        {
            base.Init();
            _crc = new CRC();
            for (var i = 0; i < _hashSizeSum; i++)
                _hash[i] = EmptyHashValue;
            _cyclicBufferPos = 0;
            ReduceOffsets(-1);
        }

        public new void MovePos()
        {
            if (++_cyclicBufferPos >= _cyclicBufferSize)
                _cyclicBufferPos = 0;
            base.MovePos();
            if (_pos == MaxValForNormalize)
                Normalize();
        }

        public new byte GetIndexbyte(int index) =>
            base.GetIndexbyte(index);

        public new uint GetMatchLen(int index, uint distance, uint limit) =>
            base.GetMatchLen(index, distance, limit);

        public new uint GetNumAvailablebytes() =>
            base.GetNumAvailablebytes();

        public void Create(
            uint historySize,
            uint keepAddBufferBefore,
            uint matchMaxLen,
            uint keepAddBufferAfter)
        {
            // Should throw an Exception when it becomes supported.
            //if (historySize > MaxValForNormalize - 256) throw new Exception();

            _cutValue = 16 + (matchMaxLen >> 1);

            var windowReservSize = (historySize + keepAddBufferBefore + matchMaxLen + keepAddBufferAfter) / 2 + 256;

            Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

            _matchMaxLen = matchMaxLen;

            var cyclicBufferSize = historySize + 1;
            if (_cyclicBufferSize != cyclicBufferSize)
                _son = new uint[(_cyclicBufferSize = cyclicBufferSize) * 2];

            var hs = BT2HashSize;

            if (_hashArray)
            {
                hs = historySize - 1;
                hs |= (hs >> 1);
                hs |= (hs >> 2);
                hs |= (hs >> 4);
                hs |= (hs >> 8);
                hs >>= 1;
                hs |= 0xFFFF;
                if (hs > (1 << 24))
                    hs >>= 1;
                _hashMask = hs;
                hs++;
                hs += _kFixHashSize;
            }

            if (hs != _hashSizeSum)
                _hash = new uint[_hashSizeSum = hs];
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

            while (true)
            {
                if (curMatch <= matchMinPos || count-- == 0)
                {
                    _son[ptr0] = _son[ptr1] = EmptyHashValue;
                    break;
                }
                var delta = _pos - curMatch;
                var cyclicPos = ((delta <= _cyclicBufferPos) ? 
                    (_cyclicBufferPos - delta) :
                    (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                var pby1 = _bufferOffset + curMatch;
                var len = Math.Min(len0, len1);
                if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                {
                    while (++len != lenLimit)
                    {
                        if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                            break;
                    }

                    if (maxLen < len)
                    {
                        distances[offset++] = maxLen = len;
                        distances[offset++] = delta - 1;
                        if (len == lenLimit)
                        {
                            _son[ptr1] = _son[cyclicPos];
                            _son[ptr0] = _son[cyclicPos + 1];

                            break;
                        }
                    }
                }

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

            MovePos();

            return offset;
        }

        public void Skip(uint num)
        {
            do
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
                        continue;
                    }
                }

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
                while (true)
                {
                    if (curMatch <= matchMinPos || count-- == 0)
                    {
                        _son[ptr0] = _son[ptr1] = EmptyHashValue;

                        break;
                    }

                    var delta = _pos - curMatch;
                    var cyclicPos = ((delta <= _cyclicBufferPos) ?
                        (_cyclicBufferPos - delta) :
                        (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                    var pby1 = _bufferOffset + curMatch;
                    var len = Math.Min(len0, len1);
                    if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                    {
                        while (++len != lenLimit)
                        {
                            if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                                break;
                        }

                        if (len == lenLimit)
                        {
                            _son[ptr1] = _son[cyclicPos];
                            _son[ptr0] = _son[cyclicPos + 1];
                            break;
                        }
                    }

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

                MovePos();
            }
            while (--num != 0);
        }
        
        public void SetCutValue(uint cutValue) =>
            _cutValue = cutValue;


        private void NormalizeLinks(uint[] items, uint numItems, uint subValue)
        {
            for (uint i = 0; i < numItems; i++)
            {
                var value = items[i];
                if (value <= subValue) value = EmptyHashValue;
                else value -= subValue;
                items[i] = value;
            }
        }

        private void Normalize()
        {
            uint subValue = _pos - _cyclicBufferSize;

            NormalizeLinks(_son, _cyclicBufferSize * 2, subValue);
            NormalizeLinks(_hash, _hashSizeSum, subValue);

            ReduceOffsets((int)subValue);
        }
    }
}
