/*
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Algorithms;

namespace Hast.Samples.Kpz
{

    public class KpzKernels
    {
        private const int Multiplier = 1000;

        public const int KpzKernels_GridHeightIndex = 0;
        public const int KpzKernels_GridWidthIndex = 1;
        public const int KpzKernels_GridStartIndex = 2;
        uint gridWidth;
        uint gridHeight;
        MWC64X prng = new MWC64X();

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
                    grid[x, y] = KpzNode.DeserializeFromUInt32(memory.ReadInt32(x * gridHeight + y + KpzKernels_GridStartIndex));
                }
            }
            return grid;
        }

        public void DoIteration(SimpleMemory memory)
        {
            gridWidth = memory.ReadUInt32(KpzKernels_GridWidthIndex);
            gridHeight = memory.ReadUInt32(KpzKernels_GridHeightIndex);
            randomLength = memory.ReadUInt32(KpzKernels_RandomLengthIndex);
            //assume that GridWidth and GridHeight are 2^N
            var numberOfStepsInIteration = gridWidth * gridHeight;

            for (int i = 0; i < numberOfStepsInIteration; i++)
            {
                // We randomly choose a point on the grid. If there is a pyramid or hole, we randomly switch them.
                var randomValue = prng.GetNextRandom();
                var randomPoint = new KpzCoords { x = randomValue & (gridWidth-1),
                    y = (randomValue>>16) & (gridHeight-1) };
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
            var neighbours = GetNeighbours(grid, p);
            var currentPoint = grid[p.x, p.y];
            bool changedGrid = false;
            // We check our own {dx,dy} values, and the right neighbour's dx, and bottom neighbour's dx.
            if (
                // If we get the pattern {01, 01} we have a pyramid:
                ((currentPoint.dx && !neighbours.nx.dx) && (currentPoint.dy && !neighbours.ny.dy) &&
                (random.NextDouble() < probabilityP)) ||
                // If we get the pattern {10, 10} we have a hole:
                ((!currentPoint.dx && neighbours.nx.dx) && (!currentPoint.dy && neighbours.ny.dy) &&
                (random.NextDouble() < probabilityQ))
            )
            {
                // We make a hole into a pyramid, and a pyramid into a hole.
                currentPoint.dx = !currentPoint.dx;
                currentPoint.dy = !currentPoint.dy;
                neighbours.nx.dx = !neighbours.nx.dx;
                neighbours.ny.dy = !neighbours.ny.dy;
                changedGrid = true;
            }
            if (enableStateLogger) StateLogger.AddKpzAction("RandomlySwitchFourCells", grid, p, neighbours, changedGrid);
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

    public static class KpzNodeExtensions
    {
        public static int width(this KpzNode[,] grid) { return GetLength(0);}
        public static int height(this KpzNode[,] grid) { return GetLength(1);}
    }

    public static class KpzKernelsExtensions
    {

    }

    internal static class KpzKernelsRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.Kpz.KpzKernels");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            using (var bitmap = new Bitmap("fpga.jpg"))
            {
                var imageFilter = await hastlayer.GenerateProxy(hardwareRepresentation, new ImageFilter());
                var filteredImage = imageFilter.DetectHorizontalEdges(bitmap);
                //How to run the same algorithm on the CPU and the FPGA?
                //Run on FPGA:     var output3 = hastlayerOptimizedAlgorithm.Run(9999);
                //Run on CPU:      var cpuOutput = new HastlayerOptimizedAlgorithm().Run(234234);
            }
        }
    }

    public static class KpzHastlayerManager
    {

        /// <summary>
        /// Specify a path here where the VHDL file describing the hardware to be generated will be saved. If the path
        /// is relative (like the default) then the file will be saved along this project's executable in the bin output
        /// directory. If an empty string or null is specified then no file will be generated.
        /// </summary>
        public static string VhdlOutputFilePath = @"Hast_IP.vhd";

        void Initialize()
        {
            Task.Run(async () =>
                {
                    //On a high level these are the steps to use Hastlayer:
                    //1. Create the Hastlayer shell.
                    //2. Configure hardware generation and generate FPGA hardware representation of the given .NET code.
                    //3. Generate proxies for hardware-transformed types and use these proxies to utilize hardware
                    //   implementations. (You can see this inside the SampleRunners.)

                    // Inititializing a Hastlayer shell for Xilinx FPGA boards.
                    using (var hastlayer = Xilinx.HastlayerFactory.Create())
                    {
                        // Hooking into an event of Hastlayer so some execution information can be made visible on the
                        // console.
                        hastlayer.ExecutedOnHardware += (sender, e) =>
                            {
                                Console.WriteLine(
                                    "Executing " +
                                    e.MemberFullName +
                                    " on hardware took " +
                                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds +
                                    "ms (net) " +
                                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds +
                                    " milliseconds (all together)");
                            };

                        var configuration = new HardwareGenerationConfiguration();

                        KpzKernelsRunner.Configure(configuration);

                        // The generated VHDL code will contain debug-level information, though it will be a bit slower
                        // to create.
                        configuration.VhdlTransformerConfiguration().VhdlGenerationOptions = VhdlGenerationOptions.Debug;

                        #region Hack
                        // You just had to open this region, didn't you?
                        // If VHDL is written to a file just after GenerateHardware() somehow an exception will be
                        // thrown from inside Hastlayer. This shouldn't happen and the exception has nothing to do with
                        // files. It's a mystery. But having this dummy file write here solves it.
                        //System.IO.File.WriteAllText(Configuration.VhdlOutputFilePath, "dummy");
                        #endregion

                        // Generating hardware from the sample assembly with the given configuration.
                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                // Selecting any type from the sample assembly here just to get its Assembly object.
                                typeof(KpzKernels).Assembly
                            },
                            configuration);

                        if (!string.IsNullOrEmpty(Configuration.VhdlOutputFilePath))
                        {
                            Helpers.HardwareRepresentationHelper.WriteVhdlToFile(hardwareRepresentation);
                        }

                        // Running Kpz kernels using Hastlayer
                        await KpzKernelsRunner.Run(hastlayer, hardwareRepresentation);
                    }
                }).Wait();
        }
    }
}
*/