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

        public long GetElapsedMilliseconds()
        {
            return _cw.ElapsedMilliseconds;
        }
        public long GetElapsedMillisecondsAndReset()
        {
            var ms = GetElapsedMilliseconds();
            Reset();

            return ms;
        }

        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000d;
        }        
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000d;
        }
    }
}