using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class Clock : IDisposable
    {
        public static Clock Create()
        {
            return new Clock();
        }

        public static double GetElapsedMilliseconds(InertiaAction handler)
        {
            var clock = Create();
            handler();
            var result = clock.GetElapsedMilliseconds();
            clock.Dispose();

            return result;
        }
        public static double GetElapsedSeconds(InertiaAction handler)
        {
            var clock = Create();
            handler();
            var result = clock.GetElapsedSeconds();
            clock.Dispose();

            return result;
        }

        #region Private variables

        private DateTime StartTime;

        #endregion

        #region Constructors

        private Clock()
        {
            Reset();
        }

        #endregion

        public Clock Reset()
        {
            StartTime = DateTime.Now;
            return this;
        }

        public double GetElapsedMilliseconds()
        {
            var time = StartTime - DateTime.Now;
            return Math.Abs(time.TotalMilliseconds);
        }
        public double GetElapsedMillisecondsAndReset()
        {
            var time = StartTime - DateTime.Now;
            Reset();
            return Math.Abs(time.TotalMilliseconds);
        }

        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000;
        }
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000;
        }

        public void Dispose()
        {
        }
    }
}
