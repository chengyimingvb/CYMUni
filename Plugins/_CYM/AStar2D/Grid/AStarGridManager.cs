using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace CYM.AStar2D
{
    /// <summary>
    /// Maintains a collection of all existing instances of the <see cref="AStarGrid"/> class.
    /// This class will be expanded in the future.
    /// </summary>
    public static class AStarGridManager
    {
        // Private
        private static List<AStarGrid> activeGrids = new List<AStarGrid>();

        // Properties
        /// <summary>
        /// Attempts to access the default grid, typically the grid that is created first.
        /// </summary>
        public static AStarGrid DefaultGrid
        {
            get { return (activeGrids.Count > 0) ? activeGrids[0] : null; }
        }

        // Methods
        internal static void RegisterGrid(AStarGrid grid)
        {
            // Add the grid to the active grids
            activeGrids.Add(grid);
        }

        internal static void UnregisterGrid(AStarGrid grid)
        {
            // Remove the grid from the list
            activeGrids.Remove(grid);
        }
    }
}
