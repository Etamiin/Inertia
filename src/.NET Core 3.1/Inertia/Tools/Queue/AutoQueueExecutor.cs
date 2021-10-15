using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class AutoQueueExecutor : IDisposable
    {
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        private readonly Queue<BasicAction> _queue;

        /// <summary>
        /// Initialize a new instance of the class <see cref="AutoQueueExecutor"/>
        /// </summary>
        public AutoQueueExecutor()
        {
            _queue = new Queue<BasicAction>();
            Task.Factory.StartNew(Execute);
        }

        public void Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AutoQueueExecutor));
            }

            foreach (var action in actions)
            {
                if (action != null)
                {
                    _queue.Enqueue(action);
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

        private async Task Execute()
        {
            while (!IsDisposed)
            {
                if (_queue.Count == 0)
                {
                    await Task.Delay(20).ConfigureAwait(false);
                    continue;
                }

                lock (_queue)
                {
                    if (_queue.TryDequeue(out BasicAction action))
                    {
                        action?.Invoke();
                    }
                }
            }
        }
    }
}
