using System;
using System.Diagnostics;

namespace Inertia
{
<<<<<<< HEAD
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
=======
    public sealed class Clock
    {
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static double GetElapsedMilliseconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();

            return clock.GetElapsedMilliseconds();
<<<<<<< HEAD
        }
        /// <summary>
        /// Automatically creates an instance of the class <see cref="Clock"/> and returns in seconds the execution time of the indicated code
        /// </summary>
        /// <param name="handler">The code to execute</param>
        /// <returns>Time elapsed in seconds</returns>
=======
        }   
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static double GetElapsedSeconds(BasicAction handler)
        {
            var clock = new Clock();
            handler();

            return clock.GetElapsedSeconds();
        }

        private Stopwatch _cw;

<<<<<<< HEAD
        /// <summary>
        /// Initialize a new instance of the class <see cref="Clock"/>
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public Clock()
        {
            Reset();
        }
<<<<<<< HEAD

        /// <summary>
        /// Reset the clock's time
        /// </summary>
        /// <returns>Returns the current instance</returns>
=======
        
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public Clock Reset()
        {
            if (_cw == null)
            {
                _cw = new Stopwatch();
            }

            _cw.Restart();
            return this;
        }

<<<<<<< HEAD
        /// <summary>
        /// Returns in milliseconds the execution time since the last reset
        /// </summary>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public long GetElapsedMilliseconds()
        {
            return _cw.ElapsedMilliseconds;
        }
<<<<<<< HEAD
        /// <summary>
        /// Returns in milliseconds the execution time since the last reset and then reset the clock
        /// </summary>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public long GetElapsedMillisecondsAndReset()
        {
            var ms = GetElapsedMilliseconds();
            Reset();

            return ms;
        }

<<<<<<< HEAD
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
=======
        public double GetElapsedSeconds()
        {
            return GetElapsedMilliseconds() / 1000d;
        }        
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public double GetElapsedSecondsAndReset()
        {
            return GetElapsedMillisecondsAndReset() / 1000d;
        }
    }
}
