using System;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ManualQueueExecutor : IDisposable
    {
        private event BasicAction Executing = () => { };

        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Initialize a new instance of the class <see cref="ManualQueueExecutor"/>
        /// </summary>
        public ManualQueueExecutor()
        {
        }

        /// <summary>
        /// Enqueue the specified actions at the end of the queue.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns>Returns the current instance</returns>
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

        /// <summary>
        ///
        /// </summary>
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
