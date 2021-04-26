namespace CYM.AStar2D
{
    public sealed class AsyncPathResult
    {
        // Private
        private AsyncPathRequest request = null;
        private Path result = null;
        private PathRequestStatus status = PathRequestStatus.InvalidIndex;

        // Properties
        public AsyncPathRequest Request
        {
            get { return request; }
        }

        public Path Result
        {
            get { return result; }
        }

        public PathRequestStatus Status
        {
            get { return status; }
        }

        // Constructor
        public AsyncPathResult(AsyncPathRequest request, Path result, PathRequestStatus status)
        {
            this.request = request;
            this.result = result;
            this.status = status;
        }

        // Methods
        public void invokeCallback()
        {
            // Make sure the request has been assigned
            if (request != null)
            {
                // Make sure there is a listener
                if (request.Callback != null)
                {
                    // Trigger the method
                    request.Callback(result, status);
                }
            }
        }
    }
}
