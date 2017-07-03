using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.Compression.Services
{
    public class SimpleMemoryStream
    {
        private bool _overflow;
        private SimpleMemory _simpleMemory;
        private int _startCellIndex;
        private int _cellCount;
        private int _cellIndex;
        private int _endCellIndex;
        private byte[] _4bytesBuffer;
        private byte _byteIndexInCell;


        public long Position => (_cellIndex - _startCellIndex) * 4 + _byteIndexInCell;
        public long Length => _cellCount * 4;
        public bool Overflow => _overflow;
        public int CellCount => _cellCount;


        public SimpleMemoryStream(SimpleMemory simpleMemory, int startCellIndex, int cellCount)
        {
            _simpleMemory = simpleMemory;
            _startCellIndex = startCellIndex;
            _cellCount = cellCount;
            _cellIndex = startCellIndex;
            _endCellIndex = _cellIndex + _cellCount - 1;

            _4bytesBuffer = new byte[4];
        }



        public void Write(byte[] buffer, int offset, int count)
        {
            if (_overflow) return;

            _4bytesBuffer = _simpleMemory.Read4Bytes(_cellIndex);
            for (var i = 0; i < count && !_overflow; i++)
            {
                _4bytesBuffer[_byteIndexInCell] = buffer[i];
                _byteIndexInCell++;

                if (_byteIndexInCell >= 4)
                {
                    _simpleMemory.Write4Bytes(_cellIndex, _4bytesBuffer);

                    IncreasePosition();

                    if (!_overflow) _4bytesBuffer = _simpleMemory.Read4Bytes(_cellIndex);
                }
            }

            if (_byteIndexInCell != 0) _simpleMemory.Write4Bytes(_cellIndex, _4bytesBuffer);
        }

        public void WriteByte(byte byteToWrite)
        {
            if (_overflow) return;

            _4bytesBuffer = _simpleMemory.Read4Bytes(_cellIndex);
            _4bytesBuffer[_byteIndexInCell] = byteToWrite;
            _simpleMemory.Write4Bytes(_cellIndex, _4bytesBuffer);

            _byteIndexInCell++;

            if (_byteIndexInCell >= 4)
            {
                IncreasePosition();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = 0;
            if (_overflow) return bytesRead;

            _4bytesBuffer = _simpleMemory.Read4Bytes(_cellIndex);
            for (var i = offset; i < count && !_overflow; i++)
            {
                buffer[i] = _4bytesBuffer[_byteIndexInCell];

                bytesRead++;
                _byteIndexInCell++;

                if (_byteIndexInCell >= 4)
                {
                    IncreasePosition();

                    if (!_overflow) _4bytesBuffer = _simpleMemory.Read4Bytes(_cellIndex);
                }
            }

            return bytesRead;
        }

        public void Close() { }

        public void Flush() { }

        public void Reset()
        {
            _cellIndex = _startCellIndex;
            _byteIndexInCell = 0;
            _overflow = false;
        }


        private void IncreasePosition()
        {
            _byteIndexInCell = 0;
            _cellIndex++;
            if (_cellIndex > _endCellIndex)
            {
                _overflow = true;
            }
        }
    }
}
