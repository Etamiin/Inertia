using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public class AutoQueueExecutor : IDisposable
    {
        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return the number of actions currently queued
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
            _queue = new List<BasicAction>();;
            Task.Factory.StartNew(Execute);
        }

        /// <summary>
        /// Enqueue the specified actions at the end of the queue
        /// </summary>
        /// <param name="handlers">Actions to enqueue</param>
        public void Enqueue(params BasicAction[] handlers)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AutoQueueExecutor));

            foreach (var handler in handlers)
                _queue.Add(handler);
        }
        
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            _queue.Clear();
            _queue = null;
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

                Thread.Sleep(1);
                await Task.Delay(1);
            }
        }
    }
}
