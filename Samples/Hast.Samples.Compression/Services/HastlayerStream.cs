using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.Compression.Services
{
    public class HastlayerStream
    {
        private readonly byte[] _bytes;
        private bool _overflow;
        private long _position;


        public long Position => _position;
        public long Length => _bytes.Length;


        public HastlayerStream(byte[] bytes)
        {
            _bytes = bytes;
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            if (_overflow) return;

            for (int i = offset; i < count; i++)
            {
                _bytes[_position] = buffer[i];

                _position++;

                if (_position >= _bytes.Length)
                {
                    _overflow = true;

                    return;
                }
            }
        }

        public void WriteByte(byte byteToWrite)
        {
            if (_overflow) return;

            _bytes[_position] = byteToWrite;

            _position++;

            if (_position >= _bytes.Length)
            {
                _overflow = true;

                return;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = 0;
            if (_overflow) return bytesRead;

            for (int i = offset; i < count; i++)
            {
                buffer[i] = _bytes[_position];

                bytesRead++;
                _position++;

                if (_position >= _bytes.Length)
                {
                    _overflow = true;

                    return bytesRead;
                }
            }

            return bytesRead;
        }

        public void Close() { }

        public void Flush() { }

        public byte[] GetBytes() => _bytes;
    }
}
