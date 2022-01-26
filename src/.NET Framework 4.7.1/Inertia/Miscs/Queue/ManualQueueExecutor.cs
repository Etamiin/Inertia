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

        private object _locker;
        private readonly Queue<BasicAction> _queue;

        public ManualQueueExecutor() : this(0)
        {
        }
        public ManualQueueExecutor(int capacity)
        {
            _locker = new object();
            _queue = new Queue<BasicAction>(capacity);
        }

        public ManualQueueExecutor Enqueue(BasicAction action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));
            }

            lock (_locker)
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

            while (_queue.Count > 0)
            {
                var action = _queue.Dequeue();
                action?.Invoke();
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
