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
        public int PoolLength => _executors.Count;
        public bool IsDisposed { get; private set; }
        public bool IsRunning { get; private set; }

        private event BasicAction Executing = () => { };

        private List<ManualQueueExecutor> _executors;
        private object _locker;
        private int _executorMaxSize;

        public ExecutorPool(int maximumExecutorSize, bool autoStart)
        {
            _executors = new List<ManualQueueExecutor>();
            _locker = new object();
            _executorMaxSize = maximumExecutorSize;

            if (autoStart) Start();
        }

        public ExecutorPool Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ExecutorPool));
            }

            IsRunning = true;
            Run();

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

            ManualQueueExecutor? best = null;

            lock (_locker)
            {
                best = _executors.FirstOrDefault((e) => !e.IsDisposed && e.Count < _executorMaxSize);

                if (best == null)
                {
                    best = new ManualQueueExecutor(_executorMaxSize);
                    Register(best);
                }

                best.Enqueue(action);
            }

            return this;
        }
        public ExecutorPool ForceExecution()
        {
            Execute();
            return this;
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
        private void Run()
        {
            Task.Factory.StartNew(async () => {
                while (IsRunning)
                {
                    if (PoolLength == 0)
                    {
                        await Task.Delay(20);
                        continue;
                    }

                    Execute();
                }
            });
        }
        private void Execute()
        {
            Executing();

            lock (_locker)
            {
                var toRemove = _executors.FindAll((x) => x.Count == 0 || x.IsDisposed).ToArray();
                foreach (var trash in toRemove)
                {
                    Unregister(trash);
                    trash.Dispose();
                }
            }
        }
    }
}
