using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Transformer.SimpleMemory;
using Hast.Layer;
using Hast.Algorithms;

namespace Hast.Samples.Kpz
{

    public static class KpzNodeExtensions
    {
        public static int width(this KpzNode[,] grid) { return grid.GetLength(0);}
        public static int height(this KpzNode[,] grid) { return grid.GetLength(1); }
    }

    public class KpzKernels
    {
        private const int Multiplier = 1000;

        public const int KpzKernels_GridHeightIndex = 0;
        public const int KpzKernels_GridWidthIndex = 1;
        public const int KpzKernels_GridStartIndex = 2;
        int gridWidth = 0; 
        int gridHeight = 0;
        uint[] gridRaw;
        uint integerProbabilityP = 32767, integerProbabilityQ = 32767;
        MWC64X prngCoords = new MWC64X();
        MWC64X prngDecision = new MWC64X();

        private bool getGridDx(int x, int y)
        {
            return (gridRaw[x + y * gridWidth] & 1) > 0;
        }

        private bool getGridDy(int x, int y)
        {
            return (gridRaw[x + y * gridWidth] & 2) > 0;
        }

        private void setGridDx(int x, int y, bool value)
        {
            gridRaw[x + y * gridWidth] = (gridRaw[x + y * gridWidth] & ~1U) | ((value) ? 1U : 0);
        }

        private void setGridDy(int x, int y, bool value)
        {
            gridRaw[x + y * gridWidth] = (gridRaw[x + y * gridWidth] & ~2U) | ((value) ? 2U : 0);
        }

        private void CopyToSimpleMemoryFromRawGrid(SimpleMemory memory)
        {
            memory.WriteUInt32(KpzKernels.KpzKernels_GridWidthIndex, (uint)gridWidth);
            memory.WriteUInt32(KpzKernels.KpzKernels_GridHeightIndex, (uint)gridHeight);
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int index = y * gridWidth + x;
                    memory.WriteUInt32(index + KpzKernels.KpzKernels_GridStartIndex, gridRaw[index]);
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
                    gridRaw[index] = memory.ReadUInt32(index + KpzKernels.KpzKernels_GridStartIndex);
                }
            }
        }

        public void DoIteration(SimpleMemory memory, bool testMode)
        {
            gridWidth = (int)memory.ReadUInt32(KpzKernels_GridWidthIndex);
            gridHeight = (int)memory.ReadUInt32(KpzKernels_GridHeightIndex);
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
            var randomPointX = (int)(randomNumber1 & (gridWidth - 1));
            var randomPointY = (int)((randomNumber1 >> 16) & (gridHeight - 1));
            uint randomNumber2 = prngDecision.GetNextRandom();
            uint randomVariable1 = randomNumber2 & ((1 << 16) - 1);
            uint randomVariable2 = (randomNumber2 >> 16) & ((1 << 16) - 1);
            var neighbours = GetNeighbourIndexes(p);
            // We check our own {dx,dy} values, and the right neighbour's dx, and bottom neighbour's dx.
            if (
                // If we get the pattern {01, 01} we have a pyramid:
                ((currentPoint.dx && !neighbours.nx.dx) && (currentPoint.dy && !neighbours.ny.dy) &&
                (forceSwitch || randomVariable1 < integerProbabilityP)) ||
                // If we get the pattern {10, 10} we have a hole:
                ((!currentPoint.dx && neighbours.nx.dx) && (!currentPoint.dy && neighbours.ny.dy) &&
                (forceSwitch || randomVariable2 < integerProbabilityQ))
            )
            {
                // We make a hole into a pyramid, and a pyramid into a hole.
                currentPoint.dx = !currentPoint.dx;
                currentPoint.dy = !currentPoint.dy;
                neighbours.nx.dx = !neighbours.nx.dx;
                neighbours.ny.dy = !neighbours.ny.dy;
            }
        }

        public struct KpzNeighbourIndexes
        {
            public int nxIndex; // Right neighbour coordinates
            public int nyIndex; // Bottom neighbour coordinates
        }

        public struct KpzCoords
        {
            public int x;
            public int y;
        }

        private KpzNeighbourIndexes GetNeighbourIndexes(KpzCoords p)
        {
            KpzNeighbourIndexes toReturn;
            int rightNeighbourX = (p.x < gridWidth - 1) ? p.x + 1 : 0;
            int rightNeighbourY = p.y;
            int bottomNeighbourX = p.x;
            int bottomNeighbourY = (p.y < gridHeight - 1) ? p.y + 1 : 0;
            toReturn.nxIndex = rightNeighbourY * gridWidth + rightNeighbourX;
            toReturn.nyIndex = bottomNeighbourY * gridWidth + bottomNeighbourX;
            return toReturn;
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

        /*
        public static void DoSingleIterationWrapper(this KpzKernels kpz, KpzNode[,] hostGrid)
        {
            SimpleMemory sm = new SimpleMemory(hostGrid.width() * hostGrid.height() + KpzKernels.KpzKernels_GridStartIndex);
            CopyFromGridToSimpleMemory(hostGrid, sm);
            kpz.DoIteration(sm, true);
            CopyFromSimpleMemoryToGrid(hostGrid, sm);
        }
        */

        /// <summary>Push table into FPGA.</summary>
        /// <param name="gridSrc">The grid to copy.</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        public static void CopyFromGridToSimpleMemory(KpzNode[,] gridSrc, SimpleMemory memoryDst)
        {
            //SimpleMemory memory = new SimpleMemory(grid.width() * grid.height() + 2); //Let's start by having dx and dy in each byte.
            memoryDst.WriteUInt32(KpzKernels.KpzKernels_GridWidthIndex, (uint)gridSrc.width());
            memoryDst.WriteUInt32(KpzKernels.KpzKernels_GridHeightIndex, (uint)gridSrc.height());
            for (int x = 0; x < gridSrc.height(); x++)
            {
                for (int y = 0; y < gridSrc.width(); y++)
                {
                    KpzNode node = gridSrc[x, y];
                    memoryDst.WriteUInt32(x * gridSrc.height() + y + KpzKernels.KpzKernels_GridStartIndex, node.SerializeToUInt32());
                }
            }
        }

        /// <summary>Pull table from the FPGA.</summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> instance.</param>
        /// <param name="image">The original image.</param>
        /// <returns>Returns the processed image.</returns>
        public static void CopyFromSimpleMemoryToGrid(KpzNode[,] grid, SimpleMemory memory)
        {
            for (int x = 0; x < grid.width(); x++)
            {
                for (int y = 0; y < grid.height(); y++)
                {
                    grid[x, y] = KpzNode.DeserializeFromUInt32(memory.ReadUInt32(x * grid.height() + y + KpzKernels.KpzKernels_GridStartIndex));
                }
            }
        }

    }
}
