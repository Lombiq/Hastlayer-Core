using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Transformer.SimpleMemory;
using Hast.Layer;
using Hast.Algorithms;

namespace Hast.Samples.Kpz
{

    /*
    public static class KpzNodeExtensions
    {
        public static int width(this KpzNode[,] grid) { return grid.GetLength(0);}
        public static int height(this KpzNode[,] grid) { return grid.GetLength(1); }
    }
    */

    public class KpzKernels
    {
        public const int gridWidth = 8; 
        public const int gridHeight = 8;
        uint[] gridRaw = new uint[gridWidth * gridHeight];
        uint integerProbabilityP = 32767, integerProbabilityQ = 32767;
        MWC64X prngCoords = new MWC64X();
        MWC64X prngDecision = new MWC64X();

        private int getIndexFromXY(int x, int y)
        {
            return x + y * gridWidth;
        }

        private bool getGridDx(int index)
        {
            return (gridRaw[index] & 1) > 0;
        }

        private bool getGridDy(int index)
        {
            return (gridRaw[index] & 2) > 0;
        }

        private void setGridDx(int index, bool value)
        {
            gridRaw[index] = (gridRaw[index] & ~1U) | ((value) ? 1U : 0);
        }

        private void setGridDy(int index, bool value)
        {
            gridRaw[index] = (gridRaw[index] & ~2U) | ((value) ? 2U : 0);
        }

        private void CopyToSimpleMemoryFromRawGrid(SimpleMemory memory)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int index = y * gridWidth + x;
                    memory.WriteUInt32(index, gridRaw[index]);
                }
            }
        }

        private void CopyFromSimpleMemoryToRawGrid(SimpleMemory memory)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    int index = y * gridWidth + x;
                    gridRaw[index] = memory.ReadUInt32(index);
                }
            }
        }

        public void DoIteration(SimpleMemory memory, bool testMode)
        {
            gridRaw = new uint[gridWidth*gridHeight]; 
            CopyFromSimpleMemoryToRawGrid(memory);
            //assume that GridWidth and GridHeight are 2^N
            var numberOfStepsInIteration = (testMode) ? 1 : gridWidth * gridHeight;

            for (int i = 0; i < numberOfStepsInIteration; i++)
            {
                // We randomly choose a point on the grid. If there is a pyramid or hole, we randomly switch them.
                RandomlySwitchFourCells(testMode);
            }
            CopyToSimpleMemoryFromRawGrid(memory);
        }

        /// Detects pyramid or hole (if any) at the given coordinates in the <see cref="grid" />, and randomly switch
        /// between pyramid and hole, based on <see cref="probabilityP" /> and <see cref="probabilityQ" /> parameters.
        /// </summary>
        /// <param name="p">
        /// contains the coordinates where the function looks if there is a pyramid or hole in the <see cref="grid" />.
        /// </param>
        private void RandomlySwitchFourCells(bool forceSwitch)
        {
            var randomNumber1 = prngCoords.GetNextRandom();
            var centerX = (int)(randomNumber1 & (gridWidth - 1));
            var centerY = (int)((randomNumber1 >> 16) & (gridHeight - 1));
            int centerIndex = getIndexFromXY(centerX, centerY);
            uint randomNumber2 = prngDecision.GetNextRandom();
            uint randomVariable1 = randomNumber2 & ((1 << 16) - 1);
            uint randomVariable2 = (randomNumber2 >> 16) & ((1 << 16) - 1);
            int rightNeighbourIndex;
            int bottomNeighbourIndex;
            GetNeighbourIndexes(centerX, centerY, out rightNeighbourIndex, out bottomNeighbourIndex);
            // We check our own {dx,dy} values, and the right neighbour's dx, and bottom neighbour's dx.
            if (
                // If we get the pattern {01, 01} we have a pyramid:
                ((getGridDx(centerIndex) && !getGridDx(rightNeighbourIndex)) && (getGridDy(centerIndex) && !getGridDy(bottomNeighbourIndex)) &&
                (forceSwitch || randomVariable1 < integerProbabilityP)) ||
                // If we get the pattern {10, 10} we have a hole:
                ((!getGridDx(centerIndex) && getGridDx(rightNeighbourIndex)) && (!getGridDy(centerIndex) && getGridDy(bottomNeighbourIndex)) &&
                (forceSwitch || randomVariable2 < integerProbabilityQ))
            )
            {
                // We make a hole into a pyramid, and a pyramid into a hole.
                setGridDx(centerIndex, !getGridDx(centerIndex));
                setGridDy(centerIndex, !getGridDy(centerIndex));
                setGridDx(rightNeighbourIndex, !getGridDx(rightNeighbourIndex));
                setGridDy(bottomNeighbourIndex, !getGridDy(bottomNeighbourIndex));
            }
        }

        private void GetNeighbourIndexes(
            int x, int y,
            out int rightNeighbourIndex, 
            out int bottomNeighbourIndex
        )
        {
            int rightNeighbourX = (x < gridWidth - 1) ? x + 1 : 0;
            int rightNeighbourY = y;
            int bottomNeighbourX = x;
            int bottomNeighbourY = (y < gridHeight - 1) ? y + 1 : 0;
            rightNeighbourIndex  = rightNeighbourY * gridWidth + rightNeighbourX;
            bottomNeighbourIndex = bottomNeighbourY * gridWidth + bottomNeighbourX;
        }

        public void TestAddInner(uint a, uint b, out uint c)
        {
            c = a + b;
        }

        public virtual void TestAddOut(SimpleMemory memory)
        {
            uint c;
            TestAddInner(memory.ReadUInt32(0), memory.ReadUInt32(1), out c);
            memory.WriteUInt32(2, c);
        }

        public virtual void TestAdd(SimpleMemory memory)
        {
            memory.WriteUInt32(2, memory.ReadUInt32(0) + memory.ReadUInt32(1));
        }

    }

    public static class KpzKernelsExtensions
    {
        public static uint TestAddOutWrapper(this KpzKernels kpz, uint a, uint b)
        {
            SimpleMemory sm = new SimpleMemory(3);
            sm.WriteUInt32(0, a);
            sm.WriteUInt32(1, b);
            kpz.TestAddOut(sm);
            return sm.ReadUInt32(2);
        }

        public static uint TestAddWrapper(this KpzKernels kpz, uint a, uint b)
        {
            SimpleMemory sm = new SimpleMemory(3);
            sm.WriteUInt32(0, a);
            sm.WriteUInt32(1, b);
            kpz.TestAdd(sm);
            return sm.ReadUInt32(2);
        }


        public static void DoSingleIterationWrapper(this KpzKernels kpz, KpzNode[,] hostGrid, bool pushToFpga)
        {
            SimpleMemory sm = new SimpleMemory(KpzKernels.gridWidth * KpzKernels.gridHeight);
            if(pushToFpga) CopyFromGridToSimpleMemory(hostGrid, sm);
            kpz.DoIteration(sm, true);
            CopyFromSimpleMemoryToGrid(hostGrid, sm);
        }


        /// <summary>Push table into FPGA.</summary>
        public static void CopyFromGridToSimpleMemory(KpzNode[,] gridSrc, SimpleMemory memoryDst)
        {
            for (int x = 0; x < KpzKernels.gridHeight; x++)
            {
                for (int y = 0; y < KpzKernels.gridWidth; y++)
                {
                    KpzNode node = gridSrc[x, y];
                    memoryDst.WriteUInt32(y * KpzKernels.gridWidth + x, node.SerializeToUInt32());
                }
            }
        }

        /// <summary>Pull table from the FPGA.</summary>
        public static void CopyFromSimpleMemoryToGrid(KpzNode[,] gridDst, SimpleMemory memorySrc)
        {
            for (int x = 0; x < KpzKernels.gridWidth; x++)
            {
                for (int y = 0; y < KpzKernels.gridHeight; y++)
                {
                    gridDst[x, y] = KpzNode.DeserializeFromUInt32(memorySrc.ReadUInt32(y * KpzKernels.gridWidth + x));
                }
            }
        }

    }
}
