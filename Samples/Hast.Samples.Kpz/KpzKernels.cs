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
        uint gridWidth = 0; //TODO
        uint gridHeight = 0;
        uint integerProbabilityP = 32767, integerProbabilityQ = 32767;
        MWC64X prngCoords = new MWC64X();
        MWC64X prngDecision = new MWC64X();

        /// <summary>Push table into FPGA.</summary>
        /// <param name="grid">The grid to copy.</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        public static SimpleMemory CopyFromGridToSimpleMemory(KpzNode[,] grid, uint[] randomValues)
        {
            SimpleMemory memory = new SimpleMemory(grid.width() * grid.height() + 2); //Let's start by having dx and dy in each byte.
            memory.WriteUInt32(KpzKernels_GridWidthIndex, (uint)grid.width());
            memory.WriteUInt32(KpzKernels_GridHeightIndex, (uint)grid.height());
            for (int x = 0; x < grid.height(); x++)
            {
                for (int y = 0; y < grid.width(); y++)
                {
                    KpzNode node = grid[x, y];
                    memory.WriteUInt32(x * grid.height() + y + KpzKernels_GridStartIndex, node.SerializeToUInt32());
                }
            }
            return memory;
        }

        /// <summary>Pull table from the FPGA.</summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> instance.</param>
        /// <param name="image">The original image.</param>
        /// <returns>Returns the processed image.</returns>
        private static KpzNode[,] CopyFromSimpleMemoryToGrid(SimpleMemory memory, int gridWidth, int gridHeight)
        {
            KpzNode[,] grid = new KpzNode[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = KpzNode.DeserializeFromUInt32(memory.ReadUInt32(x * gridHeight + y + KpzKernels_GridStartIndex));
                }
            }
            return grid;
        }

        public void DoIteration(KpzNode[,] grid)
        {
            //assume that GridWidth and GridHeight are 2^N
            var numberOfStepsInIteration = grid.width() * grid.height();

            for (int i = 0; i < numberOfStepsInIteration; i++)
            {
                // We randomly choose a point on the grid. If there is a pyramid or hole, we randomly switch them.
                var randomValue = prngCoords.GetNextRandom();
                var randomPoint = new KpzCoords { x = (int)(randomValue & (gridWidth-1)),
                    y = (int)((randomValue>>16) & (gridHeight-1)) };
                RandomlySwitchFourCells(grid, randomPoint);
            }
        }

        /// Detects pyramid or hole (if any) at the given coordinates in the <see cref="grid" />, and randomly switch
        /// between pyramid and hole, based on <see cref="probabilityP" /> and <see cref="probabilityQ" /> parameters.
        /// </summary>
        /// <param name="p">
        /// contains the coordinates where the function looks if there is a pyramid or hole in the <see cref="grid" />.
        /// </param>
        private void RandomlySwitchFourCells(KpzNode[,] grid, KpzCoords p)
        {
            uint randomNumber = prngDecision.GetNextRandom();
            uint randomVariable1 = randomNumber & ((1 << 16) - 1);
            uint randomVariable2 = (randomNumber >> 16) & ((1 << 16) - 1);
            var neighbours = GetNeighbours(grid, p);
            var currentPoint = grid[p.x, p.y];
            // We check our own {dx,dy} values, and the right neighbour's dx, and bottom neighbour's dx.
            if (
                // If we get the pattern {01, 01} we have a pyramid:
                ((currentPoint.dx && !neighbours.nx.dx) && (currentPoint.dy && !neighbours.ny.dy) &&
                (randomVariable1 < integerProbabilityP)) ||
                // If we get the pattern {10, 10} we have a hole:
                ((!currentPoint.dx && neighbours.nx.dx) && (!currentPoint.dy && neighbours.ny.dy) &&
                (randomVariable2 < integerProbabilityQ))
            )
            {
                // We make a hole into a pyramid, and a pyramid into a hole.
                currentPoint.dx = !currentPoint.dx;
                currentPoint.dy = !currentPoint.dy;
                neighbours.nx.dx = !neighbours.nx.dx;
                neighbours.ny.dy = !neighbours.ny.dy;
            }
        }

        private KpzNeighbours GetNeighbours(KpzNode[,] grid, KpzCoords p)
        {
            KpzNeighbours toReturn;
            toReturn.nxCoords = new KpzCoords
            {
                x = (p.x < gridWidth - 1) ? p.x + 1 : 0,
                y = p.y
            };
            toReturn.nyCoords = new KpzCoords
            {
                x = p.x,
                y = (p.y < gridHeight - 1) ? p.y + 1 : 0
            };
            toReturn.nx = grid[toReturn.nxCoords.x, toReturn.nxCoords.y];
            toReturn.ny = grid[toReturn.nyCoords.x, toReturn.nyCoords.y];
            return toReturn;
        }

        public void TestAdd(SimpleMemory memory)
        {
            memory.WriteUInt32(2, memory.ReadUInt32(0) + memory.ReadUInt32(1));
        }
    }


    public static class KpzKernelsExtensions
    {
        public static uint TestAddWrapper(this KpzKernels kpz, uint a, uint b)
        {
            SimpleMemory sm = new SimpleMemory(3);
            sm.WriteUInt32(0, a);
            sm.WriteUInt32(1, b);
            kpz.TestAdd(sm);
            return sm.ReadUInt32(2);
        }
    }
}
