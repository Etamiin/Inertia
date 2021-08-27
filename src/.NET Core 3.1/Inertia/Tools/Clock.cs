using System;
using System.Diagnostics;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Clock
    {
        /// <summary>
        /// Automatically creates an instance of the class <see cref="Clock"/> and returns in milliseconds the execution time of the indicated code
        /// </summary>
        /// <param name="handler">The code to execute</param>
        /// <returns>Time elapsed in milliseconds</returns>
        public static double GetElapsedMilliseconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();

            return clock.GetElapsedMilliseconds();
        }
        /// <summary>
        /// Automatically creates an instance of the class <see cref="Clock"/> and returns in seconds the execution time of the indicated code
        /// </summary>
        /// <param name="handler">The code to execute</param>
        /// <returns>Time elapsed in seconds</returns>
        public static double GetElapsedSeconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();

            return clock.GetElapsedSeconds();
        }

        private Stopwatch _cw;

        /// <summary>
        /// Initialize a new instance of the class <see cref="Clock"/>
        /// </summary>
        public Clock()
        {
            Reset();
        }

        /// <summary>
        /// Reset the clock's time
        /// </summary>
        /// <returns>Returns the current instance</returns>
        public Clock Reset()
        {
            if (_cw == null) _cw = new Stopwatch();

            _cw.Restart();
            return this;
        }

        /// <summary>
        /// Returns in milliseconds the execution time since the last reset
        /// </summary>
        /// <returns></returns>
        public long GetElapsedMilliseconds()
        {
            return _cw.ElapsedMilliseconds;
        }
        /// <summary>
        /// Returns in milliseconds the execution time since the last reset and then reset the clock
        /// </summary>
        /// <returns></returns>
        public long GetElapsedMillisecondsAndReset()
        {
            var ms = GetElapsedMilliseconds();
            Reset();

            return ms;
        }

        /// <summary>
        /// Returns in seconds the execution time since the last reset
        /// </summary>
        /// <returns></returns>
        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000d;
        }
        /// <summary>
        /// Returns in seconds the execution time since the last reset and then reset the clock
        /// </summary>
        /// <returns></returns>
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000d;
        }
    }
}
