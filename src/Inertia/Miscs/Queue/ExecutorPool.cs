using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public class ExecutorPool : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning { get; private set; }

        public int PoolLength => _executors.Count;

        private event BasicAction Executing = () => { };

        private List<Queue<BasicAction>> _executors;
        private Queue<BasicAction> _currentQueue;
        private object _locker;
        private int _executorCapacity, _executionCount, _maxExecutionCount;

        public ExecutorPool(int executorCapacity, bool autoStart)
        {
            _executors = new List<Queue<BasicAction>>();
            _locker = new object();
            _executorCapacity = executorCapacity;
            _maxExecutionCount = Math.Max(1, executorCapacity / 10);

            RefreshCurrentQueue();
            if (autoStart) Start();
        }

        public ExecutorPool Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ExecutorPool));
            }

            IsRunning = true;
            Task.Factory.StartNew(async () => {
                while (IsRunning)
                {
                    if (PoolLength == 1 && _currentQueue.Count == 0)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        continue;
                    }

                    Execute();
                }
            }, TaskCreationOptions.LongRunning);

            return this;
        }
        public ExecutorPool Stop()
        {
            IsRunning = false;
            return this;
        }
        public ExecutorPool Enqueue(BasicAction action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ExecutorPool));
            }

            lock (_locker)
            {
                if (_currentQueue == null || _currentQueue.Count >= _executorCapacity)
                {
                    RefreshCurrentQueue();
                }

                _currentQueue.Enqueue(action);
            }

            return this;
        }

        public void ExecuteAllAndDispose()
        {
            Execute();
            Dispose();
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Stop();
                lock (_locker)
                {
                    _executors.Clear();
                }

                IsDisposed = true;
            }
        }
        
        private void Execute()
        {
            _executionCount++;

            lock (_locker)
            {
                foreach (var queue in _executors)
                {
                    while (queue.TryDequeue(out BasicAction action))
                    {
                        action?.Invoke();
                    }
                }

                if (_executionCount >= _maxExecutionCount && _executors.Count > 1)
                {
                    Clear();
                }
            }
        }
        private void RefreshCurrentQueue()
        {
            _currentQueue = _executors.FirstOrDefault((e) => e.Count < _executorCapacity);

            if (_currentQueue == null)
            {
                _currentQueue = new Queue<BasicAction>(_executorCapacity);
                _executors.Add(_currentQueue);
            }
        }
        private void Clear()
        {
            var toRemove = _executors.FindAll((x) => x.Count == 0 && x != _currentQueue).ToArray();
            foreach (var trash in toRemove)
            {
                _executors.Remove(trash);
            }

            _executionCount = 0;
        }
    }
}