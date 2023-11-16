using System;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class AsyncTickedQueue : TickedQueue
    {
        private static TimeSpan DefaultSleepTimeOnEmptyQueue = TimeSpan.FromMilliseconds(30);

        public TimeSpan? TimeBeforeDisposeOnEmptyQueue { get; private set; }
        public TimeSpan SleepTimeOnEmptyQueue { get; private set; }
        
        private DateTime? _emptySince;

        public AsyncTickedQueue() : this(DefaultMaxExecutionPerTick, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AsyncTickedQueue(int maximumExecutionPerTick) : this(maximumExecutionPerTick, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AsyncTickedQueue(int maximumExecutionPerTick, TimeSpan sleepTimeOnEmptyQueue) : this(maximumExecutionPerTick, sleepTimeOnEmptyQueue, null)
        {
        }
        public AsyncTickedQueue(int maximumExecutionPerTick, TimeSpan sleepTimeOnEmptyQueue, TimeSpan? timeBeforeDisposeOnEmptyQueue) : base(maximumExecutionPerTick)
        {
            SleepTimeOnEmptyQueue = sleepTimeOnEmptyQueue;
            TimeBeforeDisposeOnEmptyQueue = timeBeforeDisposeOnEmptyQueue;

            Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
        }

        private async Task Execute()
        {
            while (true)
            {
                if (DisposeRequested)
                {
                    Clean();
                    break;
                }

                if (Count == 0)
                {
                    if (TimeBeforeDisposeOnEmptyQueue.HasValue)
                    {
                        if (_emptySince.HasValue)
                        {
                            var span = DateTime.Now - _emptySince;
                            if (span >= TimeBeforeDisposeOnEmptyQueue.Value)
                            {
                                RequestDispose();
                                continue;
                            }
                        }
                        else
                        {
                            _emptySince = DateTime.Now;
                        }
                    }

                    await Task.Delay(SleepTimeOnEmptyQueue);
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