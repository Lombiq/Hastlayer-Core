using System.Collections.Generic;
using System.Drawing;
namespace Hast.Samples.Kpz
{
    /// <summary>
    /// A KPZ iteration to be logged consists of a list of <see cref="KpzAction" /> items.
    /// </summary>
    public class KpzIteration
    {
        public List<KpzAction> Actions = new List<KpzAction>();
    }

    /// <summary>
    /// A KPZ action consists of a description, the full grid or heightmap and the highlight in it.
    /// There are three typical types of KPZ actions:
    /// <list type="bullet">
    /// <item><description>Empty <see cref="Grid" /> and <see cref="HeightMap" />, only <see cref="Description" />.</item></description>
    /// <item><description>Only <see cref="Grid" />, <see cref="Description" /> and optional highlight.</item></description>
    /// <item><description>Only <see cref="HeightMap" />, <see cref="Description" /> and optional highlight.</item></description>
    /// </list>
    /// </summary>
    public struct KpzAction
    {
        public string Description;
        public KpzNode[,] Grid;
        public int[,] HeightMap;
        public List<KpzCoords> HighlightedCoords;
        public Color HightlightColor;
    }

    /// <summary>
    /// It logs the state of the KPZ algorithm at particular steps.
    /// <note type="caution">As it stores the full KPZ grid at every step, it can use up a lot of memory.</note>
    /// </summary>
    public class KpzStateLogger
    {
        /// <summary>The KPZ iteration list.</summary>
        public List<KpzIteration> Iterations = new List<KpzIteration>();

        /// <summary>We add an iteration when the constructor is called, so actions can be added right away.</summary>
        public KpzStateLogger()
        {
            this.NewKpzIteration();
        }

        /// <summary>Add a new <see cref="KpzIteration" />.</summary>
        public void NewKpzIteration()
        {
            Iterations.Add(new KpzIteration());
        }

        /// <summary>Make a deep copy of a heightmap (2D int array).</summary>
        static int[,] CopyOfHeightMap(int[,] HeightMap)
        {
            return (int[,])HeightMap.Clone();
        }

        /// <summary>Make a deep copy of a grid (2D <see cref="KpzNode" /> array).</summary>
        static KpzNode[,] CopyOfGrid(KpzNode[,] Grid)
        {
            KpzNode[,] toReturn = new KpzNode[Grid.GetLength(0), Grid.GetLength(1)];
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    toReturn[x, y] = new KpzNode();
                    toReturn[x, y].dx = Grid[x, y].dx;
                    toReturn[x, y].dy = Grid[x, y].dy;
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Adds a deep copy of the grid into the current <see cref="KpzStateLogger" /> iteration.
        /// </summary>
        public void AddKpzAction(string Description, KpzNode[,] Grid)
        {
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = CopyOfGrid(Grid),
                HeightMap = new int[0, 0],
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        /// <summary>
        /// Adds a deep copy of the heightmap into the current <see cref="KpzStateLogger" /> iteration.
        /// </summary>
        public void AddKpzAction(string Description, int[,] HeightMap)
        {
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = new KpzNode[0, 0],
                HeightMap = CopyOfHeightMap(HeightMap),
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        /// <summary>
        /// Adds an action with only description into the current <see cref="KpzStateLogger" /> iteration.
        /// </summary>
        public void AddKpzAction(string Description)
        {
            // Adds a deep copy of the grid into the current interation
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = new KpzNode[0, 0],
                HeightMap = new int[0, 0],
                HightlightColor = Color.Transparent,
                HighlightedCoords = new List<KpzCoords>()
            });
        }

        /// <summary>
        /// Adds a deep copy of the grid into the current <see cref="KpzStateLogger" /> iteration, with cells
        /// to highlight (<see cref="Center" /> and <see cref="Neighbours" />). If the values in the grid were updated,
        /// they are highlighted with a green color, else they are highlighted with a red color, based on the parameter
        /// value of <see cref="ChangedGrid" />.
        /// </summary>
        public void AddKpzAction(string Description, KpzNode[,] Grid, KpzCoords Center, KpzNeighbours Neighbours, bool ChangedGrid)
        {
            List<KpzCoords> highlightedCoords = new List<KpzCoords>();
            highlightedCoords.Add(new KpzCoords { x = Center.x, y = Center.y });
            highlightedCoords.Add(new KpzCoords { x = Neighbours.nxCoords.x, y = Neighbours.nxCoords.y });
            highlightedCoords.Add(new KpzCoords { x = Neighbours.nyCoords.x, y = Neighbours.nyCoords.y });
            Iterations[Iterations.Count - 1].Actions.Add(new KpzAction
            {
                Description = Description,
                Grid = CopyOfGrid(Grid),
                HeightMap = new int[0, 0],
                HightlightColor = (ChangedGrid) ? Color.LightGreen : Color.Salmon, // green or red
                HighlightedCoords = highlightedCoords
            });
        }
    }
}