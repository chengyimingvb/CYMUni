using System.Collections.Generic;

namespace CYM.AStar2D
{
    internal sealed class NodeQueue<T> where T : IBaseNode
    {
        // Private
        private List<T> collection = new List<T>();
        private IComparer<T> comparer;

        // Properties
        public int Count
        {
            get { return collection.Count; }
        }

        // Constructor
        public NodeQueue(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        // Methods
        public int Push(T value)
        {
            int size = Count;
            int length = 0;

            // Update the index
            value.TileNodeIndex = Count;

            // Add to the collection
            collection.Add(value);

            do
            {
                // Check for empty
                if (size == 0)
                    break;

                // Calcualte the length
                length = (size - 1) / 2;

                // Compare the elements
                if (CompareElement(size, length) < 0)
                {
                    // Swap the elements to sort
                    SwapElement(size, length);
                    size = length;
                }
                else
                {
                    // Exit the loop
                    break;
                }
            }
            while (true);

            return size;
        }

        public T Pop()
        {
            // Get the item at the front of the list
            T value = collection[0];

            int size = 0;
            int index0 = 0;
            int index1 = 0;
            int length = 0;

            // Move the last element to the front
            collection[0] = collection[Count - 1];
            collection[0].TileNodeIndex = 0;

            // Remove the last item
            collection.RemoveAt(Count - 1);

            // Update the value index
            value.TileNodeIndex = -1;

            do
            {
                length = size;
                index0 = 2 * size + 1;
                index1 = 2 * size + 2;

                // Check if index 0 is better
                if (Count > index0 && CompareElement(size, index0) > 0)
                    size = index0;

                // CHeck if index 1 is better
                if (Count > index1 && CompareElement(size, index1) > 0)
                    size = index1;

                // No improvement was found
                if (size == length)
                    break;

                // Swap the elements so that the priority is lower
                SwapElement(size, length);
            }
            while (true);

            return value;
        }

        public T Peek()
        {
            // Make sure there is atleast 1 item in the collection
            if (Count > 0)
                return collection[0];

            // Return an error value (null usually)
            return default(T);
        }

        public void SwapElement(int x, int y)
        {
            // Get the value and store it in a temp value
            T value = collection[x];

            // Switch the values
            collection[x] = collection[y];
            collection[y] = value;

            // Update the priority indexes
            collection[x].TileNodeIndex = x;
            collection[y].TileNodeIndex = y;
        }

        public int CompareElement(int x, int y)
        {
            // Use the comparer to check the values
            return comparer.Compare(collection[x], collection[y]);
        }

        public void Refresh(T value)
        {
            while ((value.TileNodeIndex - 1 >= 0) && (CompareElement(value.TileNodeIndex - 1, value.TileNodeIndex) > 0))
                SwapElement(value.TileNodeIndex - 1, value.TileNodeIndex);

            while ((value.TileNodeIndex + 1 < Count) && (CompareElement(value.TileNodeIndex + 1, value.TileNodeIndex) < 0))
                SwapElement(value.TileNodeIndex + 1, value.TileNodeIndex);
        }

        public void Clear()
        {
            collection.Clear();
        }
    }
}
