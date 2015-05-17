using System;

namespace Hast.Transformer.SimpleMemory
{
    public class SimpleMemory
    {
        public const uint MemoryCellSizeBytes = 4;

        private readonly byte[] _memory;


        /// <summary>
        /// Constructs a new <see cref="SimpleMemory"/> object that represents a simplified memory model available on the FPGA for transformed
        /// algorithms.
        /// </summary>
        /// <param name="cellCount">
        /// The number of memory "cells". The memory is divided into independently accessible "cells"; the size of the allocated memory space is
        /// calculated from the cell count and the cell size indicated in <see cref="MemoryCellSizeBytes"/>.
        /// </param>
        public SimpleMemory(int cellCount)
        {
            _memory = new byte[cellCount * MemoryCellSizeBytes];
        }


        public void Write4Bytes(int cellIndex, byte[] input)
        {
            if (input.Length > MemoryCellSizeBytes)
            {
                throw new ArgumentException("The byte array to be written to memory should be shorter than " + MemoryCellSizeBytes + ".");
            }

            for (uint i = 0; i < input.Length; i++)
            {
                _memory[i + cellIndex * MemoryCellSizeBytes] = input[i];
            }

            for (uint i = (uint)input.Length; i < MemoryCellSizeBytes; i++)
            {
                _memory[i + cellIndex * MemoryCellSizeBytes] = 0;
            }
        }

        public byte[] Read4Bytes(int cellIndex)
        {
            var output = new byte[MemoryCellSizeBytes];

            for (uint i = 0; i < MemoryCellSizeBytes; i++)
            {
                output[i] = _memory[i + cellIndex * MemoryCellSizeBytes];
            }

            return output;
        }

        public void WriteUInt32(int cellIndex, UInt32 number)
        {
            Write4Bytes(cellIndex, BitConverter.GetBytes(number));
        }

        public UInt32 ReadUInt32(int cellIndex)
        {
            return BitConverter.ToUInt32(Read4Bytes(cellIndex), 0);
        }

        public void WriteInt32(int cellIndex, int number)
        {
            Write4Bytes(cellIndex, BitConverter.GetBytes(number));
        }

        public int ReadInt32(int cellIndex)
        {
            return BitConverter.ToInt32(Read4Bytes(cellIndex), 0);
        }

        public void WriteBoolean(int cellIndex, bool boolean)
        {
            Write4Bytes(cellIndex, BitConverter.GetBytes(boolean));
        }

        public bool ReadBoolean(int cellIndex)
        {
            return BitConverter.ToBoolean(Read4Bytes(cellIndex), 0);
        }

        public void WriteChar(int cellIndex, char character)
        {
            Write4Bytes(cellIndex, BitConverter.GetBytes(character));
        }

        public char ReadChar(int cellIndex)
        {
            return BitConverter.ToChar(Read4Bytes(cellIndex), 0);
        }
    }
}
