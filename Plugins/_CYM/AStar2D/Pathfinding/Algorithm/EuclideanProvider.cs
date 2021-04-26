using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// Provides a Euclidean heuristic.
    /// </summary>
    public class EuclideanProvider : HeuristicProvider
    {
        // Methods
        /// <summary>
        /// Calcualtes the Euclidean heuristic.
        /// </summary>
        /// <param name="start">The first node</param>
        /// <param name="end">The second node</param>
        /// <returns>The heuristic between the 2 nodes</returns>
        public override float heuristic(PathNode start, PathNode end)
        {
            float x = Mathf.Pow(end.Index.X - start.Index.X, 2);
            float y = Mathf.Pow(end.Index.Y - start.Index.Y, 2);

            // Require sqrt
            return Mathf.Sqrt(x + y);
        }
    }
}
