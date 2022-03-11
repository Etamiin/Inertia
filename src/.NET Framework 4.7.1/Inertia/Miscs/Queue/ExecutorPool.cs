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

        private List<ManualQueueExecutor> _executors;
        private ManualQueueExecutor _currentQueue;
        private object _locker;
        private int _executorCapacity, _executionCount, _clearTick;

        public ExecutorPool(int executorCapacity, bool autoStart) : this(executorCapacity, 100, autoStart)
        {
        }
        public ExecutorPool(int executorCapacity, int clearTick, bool autoStart)
        {
            _executors = new List<ManualQueueExecutor>();
            _locker = new object();
            _executorCapacity = executorCapacity;
            _clearTick = clearTick;

            ResetCurrentQueue();
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
                        await Task.Delay(10);
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

            if (_currentQueue == null || _currentQueue.Count >= _executorCapacity)
            {
                ResetCurrentQueue();
            }

            _currentQueue.Enqueue(action);
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

        private void Register(ManualQueueExecutor queue)
        {
            Executing += queue.Execute;
            _executors.Add(queue);
        }
        private void Unregister(ManualQueueExecutor queue)
        {
            Executing -= queue.Execute;
            _executors.Remove(queue);
        }
        
        private void Execute()
        {
            _executionCount++;

            Executing();
            CheckForClearing();
        }
        private void ResetCurrentQueue()
        {
            lock (_locker)
            {
                _currentQueue = _executors.FirstOrDefault((e) => !e.IsDisposed && e.Count < _executorCapacity);

                if (_currentQueue == null)
                {
                    _currentQueue = new ManualQueueExecutor(_executorCapacity);
                    Register(_currentQueue);
                }
            }
        }
        private void CheckForClearing()
        {
            if (_executionCount >= _clearTick && _executors.Count > 1)
            {
                lock (_locker)
                {
                    var toRemove = _executors.FindAll((x) => x.Count == 0 || x.IsDisposed).ToArray();
                    foreach (var trash in toRemove)
                    {
                        Unregister(trash);
                        trash.Dispose();
                    }
                }

                if (_currentQueue.IsDisposed)
                {
                    ResetCurrentQueue();
                }

                _executionCount = 0;
            }
        }
    }
}