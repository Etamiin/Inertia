using System;
using System.Diagnostics;

namespace Inertia
{
    public sealed class Clock
    {
        public static double GetElapsedMilliseconds(BasicAction action)
        {
            var clock = new Clock();
            action();

            return clock.GetElapsedMilliseconds();
        }   
        public static double GetElapsedSeconds(BasicAction action)
        {
            var clock = new Clock();
            action();

            return clock.GetElapsedSeconds();
        }

        public long ElapsedTicks => _cw.ElapsedTicks;
        
        private readonly Stopwatch _cw;

        public Clock()
        {
            _cw = new Stopwatch();
            _cw.Start();
        }
        
        public Clock Reset()
        {
            _cw.Restart();
            return this;
        }

        public double GetElapsedMilliseconds()
        {
            return _cw.Elapsed.TotalMilliseconds;
        }
        public double GetElapsedMillisecondsAndReset()
        {
            var ms = _cw.Elapsed.TotalMilliseconds;
            Reset();

            return ms;
        }

        public double GetElapsedSeconds()
        {
            return _cw.Elapsed.TotalSeconds;
        }        
        public double GetElapsedSecondsAndReset()
        {
            var seconds = GetElapsedMillisecondsAndReset();
            Reset();

            return seconds;
        }
    }
}