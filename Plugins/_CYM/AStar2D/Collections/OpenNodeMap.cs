namespace CYM.AStar2D
{
    internal sealed class OpenNodeMap<T> where T : class, IBaseNode
    {
        // Private
        private IBaseNode[,] nodeMap = null;
        private int width = 0;
        private int height = 0;
        private int count = 0;

        // Properties
        public T this[int x, int y]
        {
            get { return nodeMap[x, y] as T; }
        }

        public T this[T node]
        {
            get { return nodeMap[node.Index.X, node.Index.Y] as T; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int Count
        {
            get { return count; }
        }

        // Constructor
        public OpenNodeMap(int width, int height)
        {
            // Store values
            this.width = width;
            this.height = height;

            // Create the map
            nodeMap = new IBaseNode[width, height];
        }

        // Methods
        public bool contains(T value)
        {
            // Get the item at the index
            IBaseNode item = nodeMap[value.Index.X, value.Index.Y];

            if (item == null)
                return false;

            if (value.Equals(item) == false)
                return false;

            return true;
        }

        public void add(T value)
        {
            // Get the item at the index
            //IPathNode item = nodeMap[value.Index.X, value.Index.Y];

            // Update the size and value
            count++;
            nodeMap[value.Index.X, value.Index.Y] = value;
        }

        public void remove(T value)
        {
            // Get the item at the index
            //IPathNode item = nodeMap[value.Index.X, value.Index.Y];

            // Update the size and value
            count--;
            nodeMap[value.Index.X, value.Index.Y] = null;
        }

        public void clear()
        {
            // Reset all values to null
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    nodeMap[x, y] = null;

            // Reset the count
            count = 0;
        }
    }
}
