using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public sealed class AutoQueueExecutor : IDisposable
    {
        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
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

        private Queue<BasicAction> _queue;

        /// <summary>
        /// Initialize a new instance of the class <see cref="AutoQueueExecutor"/>
        /// </summary>
        public AutoQueueExecutor()
        {
            _queue = new Queue<BasicAction>();
            Task.Factory.StartNew(Execute);
        }

        /// <summary>
        /// Enqueue the specified actions at the end of the queue.
        /// </summary>
        /// <param name="actions"></param>
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

        /// <summary>
        /// 
        /// </summary>
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
                    try
                    {
                        var action = _queue.Dequeue();
                        action.Invoke();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
