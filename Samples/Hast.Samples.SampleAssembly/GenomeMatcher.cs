using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Algorithm for running Smith-Waterman Genome Matcher.
    /// </summary>
    public class GenomeMatcher
    {
        public const int GetLCS_InputOneLengthIndex = 0;
        public const int GetLCS_InputTwoLengthIndex = 1;
        public const int GetLCS_InputOneStartIndex = 2;
        public const ushort GetLCS_TopCellPointerValue = 0;
        public const ushort GetLCS_LeftCellPointerValue = 1;
        public const ushort GetLCS_DiagonalCellPointerValue = 2;


        public virtual void GetLCS(SimpleMemory memory)
        {
            ushort inputOneLength = (ushort)memory.ReadUInt32(GetLCS_InputOneLengthIndex); //This will be the width of the matrix.
            ushort inputTwoLength = (ushort)memory.ReadUInt32(GetLCS_InputTwoLengthIndex); //This will be the height of the matrix.

            ushort inputTwoStartIndex = (ushort)(GetLCS_InputOneStartIndex + inputOneLength);
            ushort resultStartIndex = (ushort)(inputTwoStartIndex + inputTwoLength);

            var resultLength = inputOneLength * inputTwoLength;

            for (ushort row = 0; row < inputOneLength; row++)
            {
                for (ushort column = 0; column < inputTwoLength; column++)
                {
                    ushort position = (ushort)(resultStartIndex + column + row * inputOneLength);

                    ushort topCell = 0;
                    ushort leftCell = 0;
                    ushort diagonalCell = 0;
                    ushort currentCell = 0;
                    ushort cellPointer = 0;

                    if (row != 0)
                        topCell = (ushort)memory.ReadUInt32(position - inputOneLength);

                    if (column != 0)
                        leftCell = (ushort)memory.ReadUInt32(position - 1);

                    if (column != 0 || row != 0)
                        diagonalCell = (ushort)memory.ReadUInt32(position - inputOneLength - 1);

                    //Increase the value of the diagonal cell if the current elements are the same.
                    if (memory.Read4Bytes(GetLCS_InputOneStartIndex + row) == memory.Read4Bytes(inputTwoStartIndex + column))
                        diagonalCell++;

                    //Select the maximum of the three cells and set the value of the current cell and pointer.
                    if (diagonalCell > leftCell)
                    {
                        if (diagonalCell > topCell)
                        {
                            currentCell = diagonalCell;
                            cellPointer = GetLCS_DiagonalCellPointerValue;
                        }
                        else
                        {
                            currentCell = topCell;
                            cellPointer = GetLCS_TopCellPointerValue;
                        }
                    }
                    else
                    {
                        if (leftCell > topCell)
                        {
                            currentCell = leftCell;
                            cellPointer = GetLCS_LeftCellPointerValue;
                        }
                        else
                        {
                            currentCell = topCell;
                            cellPointer = GetLCS_TopCellPointerValue;
                        }
                    }

                    memory.WriteUInt32(position, currentCell);
                    memory.WriteUInt32(position + resultLength, cellPointer);
                }
            }
        }
    }
}
