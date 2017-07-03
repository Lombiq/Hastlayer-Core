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
        private readonly Stream _stream;
        private bool _overflow;
        private long _position;


        public long Position
        {
            set { _position = value; }
            get { return _stream != null ? _stream.Position : _position; }
        }

        public long Length => _stream != null ? _stream.Length : _bytes.Length;


        public HastlayerStream(byte[] bytes)
        {
            _bytes = bytes;
        }

        public HastlayerStream(Stream stream)
        {
            _stream = stream;
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            if (_stream != null)
            {
                _stream.Write(buffer, offset, count);

                return;
            }

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
            if (_stream != null)
            {
                _stream.WriteByte(byteToWrite);

                return;
            }

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
            if (_stream != null) return _stream.Read(buffer, offset, count);

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

        public void Close()
        {
            if (_stream != null) _stream.Close();
        }

        public void Flush()
        {
            if (_stream != null) _stream.Flush();
        }

        public byte[] GetBytes() => _bytes;
    }
}
