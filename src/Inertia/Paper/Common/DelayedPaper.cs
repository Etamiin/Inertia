using System;
using System.Collections.Generic;

namespace Inertia.Paper
{
    public static class DelayedPaper
    {
        private static IEnumerator<EnumeratorPaperElement> ProcessTime(Action action, float delay, float duration)
        {
            if (delay <= 0)
            {
                action?.Invoke();
                yield break;
            }

            do
            {
                yield return new WaitForSeconds(delay);

                action?.Invoke();
                duration -= delay;
            }
            while (duration >= delay);
        }
        private static IEnumerator<EnumeratorPaperElement> ProcessNextTick(Action action)
        {
            yield return new WaitForEndOfTick();
            action?.Invoke();
        }

        public static void OnNextTick(Action action)
        {
            _ = new EnumeratorPaper(ProcessNextTick(action));
        }
        public static EnumeratorPaper Delayed(float delay, Action action)
        {
            return new EnumeratorPaper(ProcessTime(action, delay, 0));
        }
        public static EnumeratorPaper Delayed(TimeSpan delay, Action action)
        {
            return new EnumeratorPaper(ProcessTime(action, (float)delay.TotalSeconds, 0));
        }
        public static EnumeratorPaper DelayedLoop(float delayTime, float durationTime, Action action)
        {
            if (durationTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationTime), "Duration can't be negative.");
            }

            if (delayTime == 0 && durationTime > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delayTime), "Delay can't be zero when duration is greater than zero.");
            }

            return new EnumeratorPaper(ProcessTime(action, delayTime, durationTime));
        }
        public static EnumeratorPaper DelayedLoop(TimeSpan delayTime, float durationTime, Action action)
        {
            return DelayedLoop((float)delayTime.TotalSeconds, durationTime, action);
        }
    }
}