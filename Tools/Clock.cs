using System;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public class Clock
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

        private DateTime _startTime;

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
        /// <returns>Return the current instance</returns>
        public Clock Reset()
        {
            _startTime = DateTime.Now;
            return this;
        }

        /// <summary>
        /// Returns in milliseconds the execution time since the last reset
        /// </summary>
        /// <returns></returns>
        public double GetElapsedMilliseconds()
        {
            var time = _startTime - DateTime.Now;
            return Math.Abs(time.TotalMilliseconds);
        }
        /// <summary>
        /// Returns in milliseconds the execution time since the last reset and then reset the clock
        /// </summary>
        /// <returns></returns>
        public double GetElapsedMillisecondsAndReset()
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
            return GetElapsedMilliseconds() / 1000;
        }
        /// <summary>
        /// Returns in seconds the execution time since the last reset and then reset the clock
        /// </summary>
        /// <returns></returns>
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000;
        }
    }
}
