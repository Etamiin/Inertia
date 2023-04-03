using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Inertia
{
    internal class ActionQueue
    {
        private const int LimitProcessorUsageMaxDequeuePerTick = 20;

        internal event BasicAction<ActionQueue>? Disposing;

        internal bool IsDisposed { get; private set; }

        internal int Id { get; private set; }
        internal DateTime? EmptySince { get; private set; }
        internal bool DisposeRequested { get; private set; }
        internal int Count
        {
            get
            {
                return _queue.Count;
            }
        }
        internal bool AsyncExecution { get; set; }

        private readonly ConcurrentQueue<BasicAction> _queue;
        private readonly TimeSpan _maxTimeQueueAlive;
        private readonly bool _limitProcessorUsage;
        private Task? _currentTask;

        internal ActionQueue(int id, TimeSpan maxTimeQueueAlive, bool limitProcessorUsage, bool asyncExecution)
        {
            Id = id;
            _queue = new ConcurrentQueue<BasicAction>();
            _maxTimeQueueAlive = maxTimeQueueAlive;
            _limitProcessorUsage = limitProcessorUsage;

            SetAsyncExecutionState(asyncExecution);
        }

        internal void SetAsyncExecutionState(bool active)
        {
            var state = AsyncExecution;

            AsyncExecution = active;
            if (!state && active)
            {
                if (_currentTask == null && !DisposeRequested)
                {
                    _currentTask = Task.Factory.StartNew(ProcessQueueAsync, TaskCreationOptions.LongRunning);
                }
            }
        }
        internal void SyncProcessQueue()
        {
            if (IsDisposed) return;

            CheckForDisposal();
            if (_queue.Count == 0) return;

            var i = 0;
            while (i < LimitProcessorUsageMaxDequeuePerTick && _queue.TryDequeue(out var action))
            {
                action?.Invoke();
                i++;
            }
        }
        internal void Enqueue(BasicAction action)
        {
            if (IsDisposed) return;

            _queue.Enqueue(action);
        }
        internal void BeginDispose()
        {
            DisposeRequested = true;
        }

        private async void ProcessQueueAsync()
        {
            var isRunning = true;
            while (AsyncExecution && isRunning)
            {
                isRunning = CheckForDisposal();
                if (_queue.Count == 0)
                {
                    if (isRunning)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                    }

                    continue;
                }

                EmptySince = null;

                if (_limitProcessorUsage)
                {
                    var i = 0;
                    while (i < LimitProcessorUsageMaxDequeuePerTick && _queue.TryDequeue(out var action))
                    {
                        action?.Invoke();
                        i++;
                    }

                    await Task.Delay(10).ConfigureAwait(false);
                }
                else if (_queue.TryDequeue(out var action)) action?.Invoke();
            }
        }
        private bool CheckForDisposal()
        {
            if (_queue.Count == 0)
            {
                if (DisposeRequested)
                {
                    Disposing?.Invoke(this);
                    _queue?.Clear();
                    Disposing = null;
                    EmptySince = null;
                    _currentTask = null;
                    IsDisposed = true;

                    return false;
                }
                else
                {
                    if (EmptySince.HasValue)
                    {
                        var span = DateTime.Now - EmptySince.Value;
                        if (span >= _maxTimeQueueAlive) BeginDispose();
                    }
                    else
                    {
                        EmptySince = DateTime.Now;
                    }
                }
            }

            return true;
        }
    }
}
