using System;

namespace Inertia
{
    public sealed class ManualQueueExecutor : IDisposable
    {
        private event BasicAction Executing;

        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count { get; private set; }

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
