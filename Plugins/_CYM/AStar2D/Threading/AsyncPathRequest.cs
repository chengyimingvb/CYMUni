using System;

namespace CYM.AStar2D
{
    public sealed class AsyncPathRequest
    {
        // Properties
        public SearchGrid Grid { get; private set; }

        public Index Start { get; private set; }

        public Index End { get; private set; }

        public DiagonalMode Diagonal { get; private set; }

        public BaseTraversal2D Traversal2D { get; private set; }

        internal PathRequestDelegate Callback { get; private set; }

        internal long TimeStamp { get; private set; }

        // Constructor
        public AsyncPathRequest(SearchGrid grid, Index start, Index end, PathRequestDelegate callback)
        {
            this.Grid = grid;
            this.Start = start;
            this.End = end;
            this.Callback = callback;

            // Create a time stamp
            TimeStamp = DateTime.UtcNow.Ticks;
        }

        public AsyncPathRequest(SearchGrid grid, Index start, Index end, DiagonalMode diagonal, BaseTraversal2D traversal2D, PathRequestDelegate callback)
        {
            this.Grid = grid;
            this.Start = start;
            this.End = end;
            this.Diagonal = diagonal;
            this.Callback = callback;
            this.Traversal2D = traversal2D;

            // Create a time stamp
            TimeStamp = DateTime.UtcNow.Ticks;
        }
    }
}
