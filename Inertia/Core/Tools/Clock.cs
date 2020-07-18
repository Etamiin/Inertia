using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Offers the possibility to obtain the time elapsed between a block of code.
    /// </summary>
    public class Clock : IDisposable
    {
        /// <summary>
        /// Automatically creates an instance of the class Clock and returns in milliseconds the execution time of the indicated code then dispose the instance
        /// </summary>
        /// <param name="handler">The code to execute</param>
        /// <returns>Time elapsed in milliseconds</returns>
        public static double GetElapsedMilliseconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();
            var result = clock.GetElapsedMilliseconds();
            clock.Dispose();

            return result;
        }
        /// <summary>
        /// Automatically creates an instance of the class Clock and returns in seconds the execution time of the indicated code then dispose the instance
        /// </summary>
        /// <param name="handler">The code to execute</param>
        /// <returns>Time elapsed in seconds</returns>
        public static double GetElapsedSeconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();
            var result = clock.GetElapsedSeconds();
            clock.Dispose();

            return result;
        }

        #region Private variables

        private DateTime m_startTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="Clock"/>
        /// </summary>
        public Clock()
        {
            Reset();
        }

        #endregion

        /// <summary>
        /// Reset the start time of the clock
        /// </summary>
        /// <returns>Return the current instance</returns>
        public Clock Reset()
        {
            m_startTime = DateTime.Now;
            return this;
        }

        /// <summary>
        /// Returns in milliseconds the execution time between the last reset
        /// </summary>
        /// <returns>Time elapsed in milliseconds</returns>
        public double GetElapsedMilliseconds()
        {
            var time = m_startTime - DateTime.Now;
            return Math.Abs(time.TotalMilliseconds);
        }
        /// <summary>
        /// Returns in milliseconds the execution time between the last reset and then reset the clock
        /// </summary>
        /// <returns>Time elapsed in milliseconds</returns>
        public double GetElapsedMillisecondsAndReset()
        {
            var time = m_startTime - DateTime.Now;
            Reset();
            return Math.Abs(time.TotalMilliseconds);
        }

        /// <summary>
        /// Returns in seconds the execution time between the last reset
        /// </summary>
        /// <returns>Time elapsed in seconds</returns>
        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000;
        }
        /// <summary>
        /// Returns in seconds the execution time between the last reset and then reset the clock
        /// </summary>
        /// <returns>Time elapsed in seconds</returns>
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000;
        }

        /// <summary>
        /// Dispose the current instance of the class
        /// </summary>
        public void Dispose()
        {
        }
    }
}
