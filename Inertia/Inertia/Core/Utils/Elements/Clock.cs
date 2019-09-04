using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class Clock
    {
        private DateTime _startTime;

        public Clock()
        {
            ResetNow();
        }

        public void ResetNow()
        {
            _startTime = DateTime.Now;
        }

        public double GetElapsedMilliseconds()
        {
            var time = _startTime - DateTime.Now;
            return Math.Abs(time.TotalMilliseconds);
        }
        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000;
        }
        public double GetElapsedMillisecondsAndReset()
        {
            var time = _startTime - DateTime.Now;
            ResetNow();
            return Math.Abs(time.TotalMilliseconds);
        }
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000;
        }
    }
}
