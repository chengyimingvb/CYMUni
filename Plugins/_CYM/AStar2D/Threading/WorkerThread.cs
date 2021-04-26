using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CYM.AStar2D
{
    public sealed class WorkerThread
    {
        // Private        
        private static readonly int averageRange = 8;
        private static readonly int timeout = 300;
        private static short id = 0;

        private Queue<AsyncPathRequest> incoming = new Queue<AsyncPathRequest>();
        private Queue<AsyncPathResult> outgoing = new Queue<AsyncPathResult>();
        private List<long> timing = new List<long>();

        private Thread thread = null;
        private volatile bool isRunning = false;
        private volatile bool isMessageQueueEmpty = false;
        private volatile float threadLoad = 0;
        private volatile int idleFrames = 0;
        private long averageTime = 1;
        private int workerID = 0;

        // Public
        public static readonly long targetTime = 10; // Max milliseconds per frame - anything more is considered as stress

        // Constructor
        public WorkerThread(int id)
        {
            this.workerID = id;
        }
        // Properties
        public float ThreadLoad
        {
            get { return threadLoad; }
        }
        public bool IsMessageQueueEmpty
        {
            get { return isMessageQueueEmpty; }
        }
        public int IdleFrames
        {
            get { return idleFrames; }
        }

        #region set
        // Methods
        public void Launch()
        {
            // Create the thread
            thread = new Thread(new ThreadStart(ThreadMain));

            // Initialize
            thread.IsBackground = true;
            thread.Name = string.Format("AStar_2D (Worker [{0}])", id++);

            // Start the thread
            thread.Start();
        }
        public void EndOrAbort()
        {
            // Set the running flag
            isRunning = false;

            // Check for valid thread
            if (thread == null)
                return;

            // Wait for the thread to quit
            thread.Join(timeout);

            // Check if the thread is still active
            if (thread.IsAlive == true)
            {
                // Force quit
                thread.Abort();
            }
        }
        public void AsyncRequest(AsyncPathRequest request)
        {
            // Lock the queue
            lock (incoming)
            {
                // Push the request
                incoming.Enqueue(request);
            }
        }
        /// <summary>
        /// Only call this method from the main thread.
        /// </summary>
        public void ProcessMessageQueue()
        {
            AsyncPathResult result = null;

            // Lock the output queue
            lock (outgoing)
            {
                // Update the flag
                isMessageQueueEmpty = (outgoing.Count == 0);

                // Get the result
                if (outgoing.Count > 0)
                    result = outgoing.Dequeue();
            }

            if (result != null)
            {
                // Process the result
                result.invokeCallback();
            }
        }
        private void ThreadMain()
        {
            // Set the flag
            isRunning = true;

            // Used to calcualte the average            
            Stopwatch timer = new Stopwatch();

            try
            {
                // Loop forever
                while (isRunning == true)
                {
                    // Get a request
                    AsyncPathRequest request = null;

                    // Lock the input queue
                    lock (incoming)
                    {
                        // Calcualte the current thread load
                        CalcualteLoad();

                        // Get a request
                        if (incoming.Count > 0)
                            request = incoming.Dequeue();
                    }

                    // Check for a request
                    if (request == null)
                    {
                        idleFrames++;

                        // Take a long sleep - no load
                        Thread.Sleep((int)targetTime);
                        continue;
                    }

                    // Reset the idle frames
                    idleFrames = 0;

                    // Begin timing
                    timer.Reset();
                    timer.Start();

                    // Lock the grid while we search
                    lock (request.Grid)
                    {
                        // Execute the request
                        request.Grid.FindPath(request.Start, request.End, request.Diagonal, request.Traversal2D, (Path path, PathRequestStatus status) =>
                        {
                            // Create a result
                            AsyncPathResult result = new AsyncPathResult(request, path, status);

                            // Push the result to the outgoing queue
                            lock (outgoing)
                            {
                                // Add result
                                outgoing.Enqueue(result);
                            }
                        });
                    }

                    // Stop timing and calculate the average time
                    timer.Stop();
                    CalculateAverageTime(timer.ElapsedMilliseconds);

                    // Calculate the amount of rest time based on the thread load
                    int sleepDuration = (int)((1 - threadLoad) * targetTime);

                    // Sleep based on the current thread load
                    Thread.Sleep(sleepDuration);
                } // End while
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }
        /// <summary>
        /// Only call while the incoming queue is locked
        /// </summary>
        private void CalcualteLoad()
        {
            // The number of waiting tasks
            int awaiting = incoming.Count;

            // Take a performance sample
#if UNITY_EDITOR
            Performance.AddUsageSample(workerID, threadLoad);
#endif

            // Get the average time per task
            long estimatedCompletionTime = averageTime * awaiting;

            // Check for excessive
            if (estimatedCompletionTime > targetTime)
            {
                // Direct assign max load value
                threadLoad = 1;
            }
            else
            {
                // Calcualte the load as a scalar
                threadLoad = estimatedCompletionTime / (float)targetTime;
            }


        }
        private void CalculateAverageTime(long addValue)
        {
#if UNITY_EDITOR
            Performance.AddTimingSample((float)addValue / targetTime);
#endif

            // Add the values
            timing.Add(addValue);

            // Check for many
            if (timing.Count > averageRange)
                timing.RemoveAt(0);

            long accumulator = 1;

            // Add each value
            foreach (long value in timing)
                accumulator += value;

            // Cache average
            averageTime = accumulator / timing.Count;
        }
        #endregion
    }
}
