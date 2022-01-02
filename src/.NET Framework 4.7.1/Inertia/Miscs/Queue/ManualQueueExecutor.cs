using System;
using System.Collections.Generic;

namespace Inertia
{
    public sealed class ManualQueueExecutor : IDisposable
    {
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count => _queue.Count;

        private readonly Queue<BasicAction> _queue;

        public ManualQueueExecutor()
        {
            _queue = new Queue<BasicAction>();
        }

        public ManualQueueExecutor Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));
            }

            foreach (var action in actions)
            {
                _queue.Enqueue(action);
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

            lock (_queue)
            {
                while (Count > 0)
                {
                    var action = _queue.Dequeue();
                    action.Invoke();
                }
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _queue.Clear();
                IsDisposed = true;
            }
        }
    }
}
