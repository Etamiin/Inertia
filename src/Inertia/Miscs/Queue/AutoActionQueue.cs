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

        public AutoActionQueue() : this(DefaultMaxExecutionPerTick, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumExecutionPerTick) : this(maximumExecutionPerTick, DefaultSleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumExecutionPerTick, TimeSpan sleepTimeOnEmptyQueue) : this(maximumExecutionPerTick, sleepTimeOnEmptyQueue, null)
        {
        }
        public AutoActionQueue(int maximumExecutionPerTick, TimeSpan sleepTimeOnEmptyQueue, TimeSpan? timeBeforeDisposeOnEmptyQueue) : base(maximumExecutionPerTick)
        {
            SleepTimeOnEmptyQueue = sleepTimeOnEmptyQueue;
            TimeBeforeDisposeOnEmptyQueue = timeBeforeDisposeOnEmptyQueue;

            Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
        }

        private async Task Execute()
        {
            while (true)
            {
                if (_isDisposing)
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