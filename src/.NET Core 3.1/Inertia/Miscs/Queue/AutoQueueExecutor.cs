using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class AutoQueueExecutor : IDisposable
    {
        public bool IsDisposed => _executor.IsDisposed;
        /// <summary>
        /// Returns the number of actions currently queued.
        /// </summary>
        public int Count => _executor.Count;

        private readonly ManualQueueExecutor _executor;

        public AutoQueueExecutor()
        {
            _executor = new ManualQueueExecutor();
            Task.Factory.StartNew(Execute);
        }

        public AutoQueueExecutor Enqueue(params BasicAction[] actions)
        {
            _executor.Enqueue(actions);
            return this;
        }
        public void Dispose()
        {
            _executor.Dispose();
        }

        internal void ForceExecute()
        {
            if (IsDisposed) return;

            lock (_executor)
            {
                _executor.Execute();
            }
        }

        private async Task Execute()
        {
            try
            {
                while (!IsDisposed)
                {
                    if (_executor.Count == 0)
                    {
                        await Task.Delay(20).ConfigureAwait(false);
                        continue;
                    }

                    _executor.Execute();
                }
            }
            catch { }        
        }
    }
}