using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.Kpz
{
    public struct KpzNeighbours
    {
        public KpzNode nx;
        public KpzNode ny;
        public KpzCoords nxCoords;
        public KpzCoords nyCoords;
    }
    public class KpzNode
    {
        public bool dx; //Right
        public bool dy; //Bottom
    }
    public struct KpzCoords
    {
        public int x;
        public int y;
    }


    public class Kpz
    {
        public int gridWidth { get { return grid.GetLength(0); } }
        public int gridHeight { get { return grid.GetLength(1); } }
        KpzNode[,] grid;
        private double probabilityP = 0.5d;
        private double probabilityQ = 0.5d;
        private Random random = new Random();

        private bool enableStateLogger = false;
        public KpzStateLogger StateLogger;

        public Kpz(int newGridWidth, int newGridHeight, double probabilityP, double probabilityQ, bool enableStateLogger)
        {
            grid = new KpzNode[newGridWidth, newGridHeight];
            this.probabilityP = probabilityP;
            this.probabilityQ = probabilityQ;
            this.enableStateLogger = enableStateLogger;
            if(this.enableStateLogger) StateLogger = new KpzStateLogger();
        }

        public void RandomizeGrid()
        {
            //Fill grid with random numbers
            for(int x=0; x<gridWidth; x++)
            {
                for(int y=0; y<gridHeight; y++)
                {
                    grid[x, y] = new KpzNode();
                    grid[x, y].dx = random.Next(0, 2) == 0;
                    grid[x, y].dy = random.Next(0, 2) == 0;
                }
            }
            if(enableStateLogger) StateLogger.AddKpzAction("RandomizeGrid", grid);
        }

        public void InitializeGrid()
        {
            //Fill grid with a pattern that is already valid as pyramids
            for(int x=0; x<gridWidth; x++)
            {
                for(int y=0; y<gridHeight; y++)
                {
                    grid[x, y] = new KpzNode();
                    grid[x, y].dx = (bool) ((x & 1) != 0);
                    grid[x, y].dy = (bool) ((y & 1) != 0);
                }
            }
            if(enableStateLogger) StateLogger.AddKpzAction("InitializeGrid", grid);
        }

        static int Bool2Delta(bool what)
        {
            return (what)?1:-1;
        }

        public int[,] GenerateHeightMap(out double mean, out bool periodicityValid, out int periodicityInvalidXCount, out int periodicityInvalidYCount)
        {
            const bool doVerboseLoggingToConsole = true;
            int[,] heightMap = new int[gridWidth, gridHeight];
            int heightNow = 0;
            mean = 0;
            //int meanNumItems = 0;
            periodicityValid = true;
            periodicityInvalidXCount = periodicityInvalidYCount = 0;
            for(int y=0; y<gridHeight; y++)
            {
                //if(y>0) heightNow = heightMap[0,y-1] + Bool2Delta(grid[0, y].dy);
                for(int x=0; x<gridWidth; x++)
                {
                    heightNow += Bool2Delta(grid[(x+1)%gridWidth, y].dx);
                    heightMap[x, y] = heightNow;
                    //mean = mean * (meanNumItems/(meanNumItems+1)) + heightNow/(meanNumItems+1);
                    mean += heightNow;
                    //meanNumItems++;
                }
                if(heightNow + Bool2Delta(grid[1, y].dx) != heightMap[0, y])
                {
                    periodicityValid = false;
                    periodicityInvalidXCount++;
                    if(doVerboseLoggingToConsole) Console.WriteLine(String.Format("periodicityInvalidX at line {0}", y));
                }
                heightNow += Bool2Delta(grid[0, (y+1)%gridHeight].dy);
            }
            if(heightMap[0, gridHeight-1] + Bool2Delta(grid[0, 0].dy) != heightMap[0, 0])
            {
                periodicityValid = false;
                periodicityInvalidYCount++;
                if(doVerboseLoggingToConsole) Console.WriteLine(String.Format("periodicityInvalidY {0} + {1} != {2}", heightMap[0, gridHeight-1], Bool2Delta(grid[0, 0].dy), heightMap[0, 0]));
            }
            if(enableStateLogger) StateLogger.AddKpzAction("GenerateHeightMap", heightMap);
            mean /= gridWidth * gridHeight;
            return heightMap;
        }

        public double HeightMapStandardDeviation(int[,] inputHeightMap, double mean)
        { //Idea: https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Two-pass_algorithm
            double variance = 0;
            for(int y=0; y<gridHeight; y++)
            {
                for(int x=0; x<gridWidth; x++)
                {
                    variance += Math.Pow(inputHeightMap[x,y] - mean, 2);
                }
            }
            variance /= gridWidth*gridHeight-1;
            double standardDeviation = Math.Sqrt(variance);
            if(enableStateLogger) StateLogger.AddKpzAction(String.Format("HeightMapStandardDeviation: {0}", standardDeviation));
            return standardDeviation;
        }

        private void RandomlySwitchFourCells(KpzNode[,] grid, KpzCoords p)
        {
            //Detect pyramid or hole. Randomly switch between pyramid and hole.
            var neighbours = GetNeighbours(grid, p);
            var currentPoint = grid[p.x, p.y];
            bool changedGrid = false;
            //We check our own {dx,dy} values, and the right neighbour's dx, and bottom neighbour's dx.
            if(
                //If we get the pattern {01, 01} we have a pyramid:
                ((currentPoint.dx && !neighbours.nx.dx) && (currentPoint.dy && !neighbours.ny.dy) &&
                (random.NextDouble() < probabilityP)) ||
                //If we get the pattern {10, 10} we have a hole:
                ((!currentPoint.dx && neighbours.nx.dx) && (!currentPoint.dy && neighbours.ny.dy) &&
                (random.NextDouble() < probabilityQ))
            )
            {
                //We make a hole into a pyramid, and a pyramid into a hole.
                currentPoint.dx = !currentPoint.dx;
                currentPoint.dy = !currentPoint.dy;
                neighbours.nx.dx = !neighbours.nx.dx;
                neighbours.ny.dy = !neighbours.ny.dy;
                changedGrid = true;
            }
            if(enableStateLogger) StateLogger.AddKpzAction("RandomlySwitchFourCells", grid, p, neighbours, changedGrid);
        }

        public void DoIteration()
        {
            //We randomly choose a point in the grid.
            var numberOfStepsInIteration = gridWidth*gridHeight;
            if(enableStateLogger) StateLogger.NewKpzIteration();
            for(int i=0;i<numberOfStepsInIteration;i++)
            {
                //If there is a pyramid or hole, we randomly swtich them.
                var randomPoint = new KpzCoords{ x=random.Next(0, gridWidth), y=random.Next(0, gridHeight) };
                RandomlySwitchFourCells(grid, randomPoint);
            }
        }

        private KpzNeighbours GetNeighbours(KpzNode[,] grid, KpzCoords p)
        {
            KpzNeighbours toReturn;
            toReturn.nxCoords = new KpzCoords
            {
                x = (p.x<gridWidth-1) ? p.x + 1 : 0,
                y = p.y
            };
            toReturn.nyCoords = new KpzCoords
            {
                x = p.x,
                y = (p.y<gridHeight-1) ? p.y + 1 : 0
            };
            toReturn.nx = grid[toReturn.nxCoords.x, toReturn.nxCoords.y];
            toReturn.ny = grid[toReturn.nyCoords.x, toReturn.nyCoords.y];
            return toReturn;
        }
    }

}
