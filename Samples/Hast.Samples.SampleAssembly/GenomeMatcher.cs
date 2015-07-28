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

            for (ushort row = 0; row < inputTwoLength; row++)
            {
                for (ushort column = 0; column < inputOneLength; column++)
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

                    if (column != 0 && row != 0)
                        diagonalCell = (ushort)memory.ReadUInt32(position - inputOneLength - 1);

                    if (column == 0 && row == 0)
                        diagonalCell = 1;

                    var a = memory.Read4Bytes(GetLCS_InputOneStartIndex + column);
                    var b = memory.Read4Bytes(inputTwoStartIndex + row);

                    //Increase the value of the diagonal cell if the current elements are the same.
                    if (memory.Read4Bytes(GetLCS_InputOneStartIndex + column)[0] == memory.Read4Bytes(inputTwoStartIndex + row)[0] && row != 0 && column != 0)
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

            ushort currentResultPosition = (ushort)(resultStartIndex + resultLength - 1);
            ushort currentResultCell = (ushort)memory.ReadUInt32(currentResultPosition);
            ushort previousCellPosition = 0;
            ushort previousCell = 0;
            ushort currentPointer = 0;
            short currentColumn = (short)inputOneLength;

            while (currentColumn > 0)
            {
                if (currentColumn == 0)
                    currentColumn--;

                currentPointer = (ushort)memory.ReadUInt32(currentResultPosition + resultLength);

                if (currentPointer == GetLCS_DiagonalCellPointerValue)
                {
                    previousCellPosition = (ushort)(currentResultPosition - inputOneLength - 1);
                    currentColumn--;
                }
                else if (currentPointer == GetLCS_LeftCellPointerValue)
                {
                    previousCellPosition = (ushort)(currentResultPosition - 1);
                    currentColumn--;
                }
                else if (currentPointer == GetLCS_TopCellPointerValue)
                {
                    previousCellPosition = (ushort)(currentResultPosition - inputOneLength);
                }

                if (previousCellPosition >= resultStartIndex)
                    previousCell = (ushort)memory.ReadUInt32(previousCellPosition);

                if (currentPointer == GetLCS_DiagonalCellPointerValue && (currentResultCell == previousCell + 1 || previousCellPosition < resultStartIndex))
                {
                    var originalValue = memory.Read4Bytes(GetLCS_InputOneStartIndex + currentColumn);
                    memory.Write4Bytes(resultStartIndex + 2 * resultLength + currentColumn, originalValue);
                }

                currentResultCell = previousCell;
                currentResultPosition = previousCellPosition;
            }
        }
    }

    public static class GenomeMatcherExtensions
    {
        public static string GetLCS(this GenomeMatcher genomeMatcher, string inputOne, string inputTwo)
        {
            var simpleMemory = CreateSimpleMemory(inputOne, inputTwo);

            genomeMatcher.GetLCS(simpleMemory);

            var length = Math.Max(inputOne.Length, inputTwo.Length);

            var result = "";
            var startIndex = GenomeMatcher.GetLCS_InputOneStartIndex + inputOne.Length + inputTwo.Length + (inputOne.Length * inputTwo.Length) * 2;


            for (int i = 0; i < length; i++)
            {
                var currentChar = simpleMemory.Read4Bytes(startIndex + i);
                var chars = Encoding.UTF8.GetChars(currentChar);
                result += chars[0];
            }

            return result.Replace("\0", "");
        }


        private static SimpleMemory CreateSimpleMemory(string inputOne, string inputTwo)
        {
            var totalLength = 2 + inputOne.Length + inputTwo.Length + (inputOne.Length * inputTwo.Length) * 2 + Math.Max(inputOne.Length, inputTwo.Length);

            var simpleMemory = new SimpleMemory(totalLength);

            simpleMemory.WriteUInt32(GenomeMatcher.GetLCS_InputOneLengthIndex, (uint)inputOne.Length);
            simpleMemory.WriteUInt32(GenomeMatcher.GetLCS_InputTwoLengthIndex, (uint)inputTwo.Length);

            for (int i = 0; i < inputOne.Length; i++)
            {
                simpleMemory.Write4Bytes(GenomeMatcher.GetLCS_InputOneStartIndex + i, Encoding.UTF8.GetBytes(inputOne[i].ToString()));
            }

            for (int i = 0; i < inputTwo.Length; i++)
            {
                simpleMemory.Write4Bytes(GenomeMatcher.GetLCS_InputOneStartIndex + i + inputOne.Length, Encoding.UTF8.GetBytes(inputTwo[i].ToString()));
            }

            return simpleMemory;
        }
    }
}
