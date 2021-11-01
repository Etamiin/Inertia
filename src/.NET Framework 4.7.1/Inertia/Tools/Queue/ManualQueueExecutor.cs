using System;

namespace Inertia
{
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
    public sealed class ManualQueueExecutor : IDisposable
    {
        private event BasicAction Executing;

<<<<<<< HEAD
        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count { get; private set; }

<<<<<<< HEAD
        /// <summary>
        /// Enqueue the specified actions at the end of the queue.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns>Returns the current instance</returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public ManualQueueExecutor Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));
            }

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];

                Executing += handler;
                Count++;

                void handler()
                {
                    action();

                    Executing -= handler;
                    Count--;
                }
            }

            return this;
        }

        /// <summary>
        /// Execute all the actions queued and remove them from the queue.
        /// </summary>
        public void Execute()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));
            }

            lock (Executing)
            {
                Executing?.Invoke();
            }
        }

<<<<<<< HEAD
        /// <summary>
        ///
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Executing = null;
                IsDisposed = true;
            }
        }
    }
}
