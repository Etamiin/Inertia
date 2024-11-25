using System;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class AutoActionQueue : ActionQueueBase
    {
        private static TimeSpan DefaultSleepTimeOnEmptyQueue = TimeSpan.FromMilliseconds(30);

        public TimeSpan? TimeBeforeDisposeOnEmptyQueue { get; private set; }
        public TimeSpan SleepTimeOnEmptyQueue { get; private set; }
        
        private DateTime? _emptySince;

        public AutoActionQueue() : this(DefaultMaxDequeuePerExecution, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumDequeuePerExecution) : this(maximumDequeuePerExecution, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumDequeuePerExecution, TimeSpan sleepTimeOnEmptyQueue) : this(maximumDequeuePerExecution, sleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumDequeuePerExecution, TimeSpan sleepTimeOnEmptyQueue, TimeSpan? timeBeforeDisposeOnEmptyQueue) : base(maximumDequeuePerExecution)
        {
            SleepTimeOnEmptyQueue = sleepTimeOnEmptyQueue;
            TimeBeforeDisposeOnEmptyQueue = timeBeforeDisposeOnEmptyQueue;

            _ = Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
        }

        private async Task Execute()
        {
            while (!IsDisposed)
            {
                if (Count == 0)
                {
                    if (TimeBeforeDisposeOnEmptyQueue.HasValue)
                    {
                        if (_emptySince.HasValue)
                        {
                            var span = DateTime.Now - _emptySince.Value;
                            if (span >= TimeBeforeDisposeOnEmptyQueue.Value)
                            {
                                Dispose();
                                continue;
                            }
                        }
                        else
                        {
                            _emptySince = DateTime.Now;
                        }
                    }

                    await Task.Delay(SleepTimeOnEmptyQueue).ConfigureAwait(false);
                }
                else
                {
                    _emptySince = null;
                    ProcessQueue();
                }
            }
        }
    }
}