using System;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ManualQueueExecutor : IDisposable
    {
        private event BasicAction QueueExecutor = () => { };

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
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));

            foreach (var action in actions)
            {
                void handler()
                {
                    action();

                    QueueExecutor -= handler;
                    Count--;
                }

                QueueExecutor += handler;
                Count++;
            }

            return this;
        }

        /// <summary>
        /// Execute all the actions queued and remove them from the queue.
        /// </summary>
        public void Execute()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));

            lock (QueueExecutor)
                QueueExecutor();
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            QueueExecutor = null;

            IsDisposed = true;
        }
    }
}
