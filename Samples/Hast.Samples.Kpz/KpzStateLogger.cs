using System.Collections.Generic;
using System.Drawing;
namespace Hast.Samples.Kpz
{
    public class KpzStateLogger
    {
        public List<KpzIteration> Iterations = new List<KpzIteration>();

        public KpzStateLogger()
        {
            this.NewKpzIteration();
        }

        public void NewKpzIteration()
        {
            Iterations.Add(new KpzIteration());
        }

        static int[,] CopyOfHeightMap(int[,] HeightMap)
        {
            return (int[,])HeightMap.Clone();
        }

        static KpzNode[,] CopyOfGrid(KpzNode[,] Grid)
        {
            //Perform a deep copy on 2D array of KpzNodes.
            KpzNode[,] toReturn = new KpzNode[Grid.GetLength(0), Grid.GetLength(1)];
            for(int x=0; x<Grid.GetLength(0); x++)
            {
                for(int y=0; y<Grid.GetLength(1); y++)
                {
                    toReturn[x, y] = new KpzNode();
                    toReturn[x, y].dx = Grid[x, y].dx;
                    toReturn[x, y].dy = Grid[x, y].dy;
                }
            }
            return toReturn;
        }

        public void AddKpzAction(string Description, KpzNode[,] Grid)
        {
            //Adds a deep copy of the grid into the current interation
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = CopyOfGrid(Grid),
                HeightMap = new int[0,0],
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        public void AddKpzAction(string Description, int[,] HeightMap)
        {
            //Adds a deep copy of the grid into the current interation
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = new KpzNode[0,0],
                HeightMap = CopyOfHeightMap(HeightMap),
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        public void AddKpzAction(string Description)
        {
            //Adds a deep copy of the grid into the current interation
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = new KpzNode[0,0],
                HeightMap = new int[0,0],
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        public void AddKpzAction(string Description, KpzNode[,] Grid, KpzCoords Center, KpzNeighbours Neighbours, bool ChangedGrid)
        {
            List<KpzCoords> highlightedCoords = new List<KpzCoords>();
            highlightedCoords.Add(new KpzCoords{x=Center.x, y=Center.y});
            highlightedCoords.Add(new KpzCoords{x=Neighbours.nxCoords.x, y=Neighbours.nxCoords.y});
            highlightedCoords.Add(new KpzCoords{x=Neighbours.nyCoords.x, y=Neighbours.nyCoords.y});
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = CopyOfGrid(Grid),
                HeightMap = new int[0,0],
                HightlightColor = (ChangedGrid)? Color.LightGreen : Color.Salmon, //green or red
                HighlightedCoords = highlightedCoords
            });
        }
    }

    public struct KpzAction
    {
        public string Description;
        public KpzNode[,] Grid;
        public int[,] HeightMap;
        public List<KpzCoords> HighlightedCoords;
        public Color HightlightColor;
    }

    public class KpzIteration
    {
        public List<KpzAction> Actions = new List<KpzAction>();
    }
}
