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
                lock (_queue)
                    return _queue.Count;
            }
        }

        private List<BasicAction> _queue;

        /// <summary>
        /// Initialize a new instance of the class <see cref="AutoQueueExecutor"/>
        /// </summary>
        public AutoQueueExecutor()
        {
            _queue = new List<BasicAction>(); ;
            Task.Factory.StartNew(Execute);
        }

        /// <summary>
        /// Enqueue the specified actions at the end of the queue.
        /// </summary>
        /// <param name="actions"></param>
        public void Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AutoQueueExecutor));

            foreach (var action in actions)
                _queue.Add(action);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            _queue.Clear();

            IsDisposed = true;
        }

        private async void Execute()
        {
            while (!IsDisposed)
            {
                if (Count == 0)
                {
                    await Task.Delay(50);
                    continue;
                }

                lock (_queue)
                {
                    for (var i = 0; i < _queue.Count; i++)
                    {
                        var action = _queue[i];
                        try
                        {
                            action();
                        }
                        catch { }
                    }

                    _queue.Clear();
                }

                await Task.Delay(1);
            }
        }
    }
}
