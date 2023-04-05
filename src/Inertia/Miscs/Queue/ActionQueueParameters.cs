using System;

namespace Inertia
{
    public sealed class ActionQueueParameters
    {
        private const int DefaultMaxExecutionPerTick = 30;
        private static TimeSpan DefaultSleepTimeOnEmptyQueue = TimeSpan.FromMilliseconds(100);

        public int MaximumExecutionPerTick { get; set; } = DefaultMaxExecutionPerTick;
        public TimeSpan? TimeBeforeDisposeOnEmptyQueue { get; set; }
        public TimeSpan SleepTimeOnEmptyQueue { get; set; } = DefaultSleepTimeOnEmptyQueue;

        public ActionQueueParameters()
        {
        }
    }
}
