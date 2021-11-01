using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
    public sealed class AutoQueueExecutor : IDisposable
    {
        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
=======
    public sealed class AutoQueueExecutor : IDisposable
    {
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
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

<<<<<<< HEAD
        /// <summary>
        /// Enqueue the specified actions at the end of the queue.
        /// </summary>
        /// <param name="actions"></param>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
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
