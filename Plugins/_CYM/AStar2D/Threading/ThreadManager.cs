using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.AStar2D
{
    public sealed class ThreadManager : MonoBehaviour, IEnumerable<WorkerThread>
    {
        // Private
        private static readonly float threadSpawnThreshold = 0.6f;
        private static readonly int minWorkerThreads = 1;
        private static ThreadManager manager = null;
        private List<WorkerThread> threads = new List<WorkerThread>();

        // Public
        public static readonly int maxAllowedWorkerThreads = 3;
        [Range(0, 16)]
        public int maxWorkerThreads = 16;
        public int maxIdleFrames = 240;

        #region life
        private void Update()
        {
            // Make sure there is always atleast 1 thread
            if (maxWorkerThreads <= 0)
                maxWorkerThreads = minWorkerThreads;

            // Process messages for this frame
            foreach (WorkerThread thread in threads)
                thread.ProcessMessageQueue();
            terminateIdleWorkers();
        }
        private void OnDestroy()
        {
            DoDestroy();
        }
        public int ActiveThreads
        {
            get { return threads.Count; }
        }
        #endregion

        #region static
        // Properties
        public static ThreadManager Active
        {
            get
            {
                // Launch the manager
                LaunchIfRequired();

                return manager;
            }
        }
        // Methods
        public static void LaunchIfRequired()
        {
            // CHeck for valid manager
            if (manager != null)
                return;

            // Check for any other instances
            ThreadManager externalManager = Component.FindObjectOfType<ThreadManager>();

            // Chekc for any found managers
            if (externalManager == null)
            {
                // Create a parent object
                GameObject go = new GameObject("AStar2D-ThreadManager");
                // Add the component
                manager = go.AddComponent<ThreadManager>();

                // Dont destroy the object
                //DontDestroyOnLoad(go);
            }
            else
            {
                // Store a reference
                manager = externalManager;
            }
        }
        #endregion

        #region set
        public void DoDestroy()
        {
            // Process each thread
            foreach (WorkerThread thread in threads)
            {
                // Dispatch each message immediatley
                while (thread.IsMessageQueueEmpty == false)
                    thread.ProcessMessageQueue();

                // Terminate the thread
                thread.EndOrAbort();
            }

            // Clear the list
            threads.Clear();

            // Reset the reference
            manager = null;
        }
        public void asyncRequest(AsyncPathRequest request)
        {
            // Get the worker
            WorkerThread thread = FindSuitableWorker();

            // Push the request
            thread.AsyncRequest(request);
        }
        public bool HasThread(WorkerThread thread)
        {
            return threads.Contains(thread);
        }
        #endregion

        #region get
        public int GetThreadID(WorkerThread thread)
        {
            return threads.IndexOf(thread);
        }
        public IEnumerator<WorkerThread> GetEnumerator()
        {
            return threads.GetEnumerator();
        }
        #endregion

        #region private
        private WorkerThread spawnThread()
        {
            // Create a new worker
            WorkerThread thread = new WorkerThread(threads.Count);

            // Register
            threads.Add(thread);

            // Begin
            thread.Launch();

            return thread;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return threads.GetEnumerator();
        }
        private IEnumerator threadTerminateRoutine(WorkerThread thread)
        {
            // Make sure all messages are dispatched before killing the thread
            while (thread.IsMessageQueueEmpty == false)
            {
                // Process thr threads messages
                thread.ProcessMessageQueue();

                // Wait for next frame
                yield return null;
            }

            // We can now kill the thread
            thread.EndOrAbort();
        }
        private void terminateIdleWorkers()
        {
            int totalThreads = threads.Count;

            // We cant termainte the remaining threads
            if (totalThreads <= minWorkerThreads)
                return;

            // Process the list of threads
            for (int i = 0; i < threads.Count; i++)
            {
                // Check if a thread is idleing
                if (threads[i].ThreadLoad == 0)
                {
                    if (threads[i].IdleFrames > maxIdleFrames)
                    {
                        // Triger routine
                        StartCoroutine(threadTerminateRoutine(threads[i]));

                        // Remove from list
                        threads.RemoveAt(i);
                    }
                }
            }
        }
        private WorkerThread FindSuitableWorker()
        {
            // Make sure there is a worker to handle the request
            if (threads.Count == 0)
                return spawnThread();

            // Try to find a suitable thread
            WorkerThread candidate = threads[0];
            float best = 1;

            foreach (WorkerThread thread in threads)
            {
                if (thread.ThreadLoad < best)
                {
                    candidate = thread;
                    best = thread.ThreadLoad;
                }
            }

            // Check for no candidate
            if (best >= threadSpawnThreshold)
            {
                // Check if we can spawn a new thread
                if (threads.Count < maxWorkerThreads)
                {
                    // Create a new worker for the request
                    return spawnThread();
                }
            }

            return candidate;
        }
        #endregion
    }
}
