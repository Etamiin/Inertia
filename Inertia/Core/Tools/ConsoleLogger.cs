using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Represent the logger for the <see cref="Console"/>
    /// </summary>
    public class ConsoleLogger : BaseLogger
    {
        #region Public variables

        /// <summary>
        /// The color used for basic log
        /// </summary>
        public ConsoleColor baseColor;
        /// <summary>
        /// The color used for success log
        /// </summary>
        public ConsoleColor successColor;
        /// <summary>
        /// The color used for error log
        /// </summary>
        public ConsoleColor errorColor;

        #endregion

        #region Private variables

        private ConsoleColor m_currentColor;

        #endregion

        /// <summary>
        /// Initialize the current instance of <see cref="ConsoleLogger"/>
        /// </summary>
        protected override void OnInitialized()
        {
            baseColor = ConsoleColor.Yellow;
            errorColor = ConsoleColor.Red;
            successColor = ConsoleColor.Green;

            m_currentColor = baseColor;
        }
        /// <summary>
        /// Call <see cref="Console.WriteLine(string)"/> with parsed log and specific <see cref="ConsoleColor"/>
        /// </summary>
        /// <param name="parsedLog"></param>
        protected override void OnLog(string parsedLog)
        {
            Console.ForegroundColor = m_currentColor;
            Console.WriteLine(parsedLog);
        }

        /// <summary>
        /// Create an "success" log using the <see cref="successColor"/>
        /// </summary>
        /// <param name="log">The string content of the log</param>
        /// <param name="parameters">The parameters used to parse the string content</param>
        public void Success(object log, params object[] parameters)
        {
            m_currentColor = successColor;
            base.Log(log, parameters);
            m_currentColor = baseColor;
        }
        /// <summary>
        /// Create an "error" log using the <see cref="errorColor"/>
        /// </summary>
        /// <param name="log">The string content of the log</param>
        /// <param name="parameters">The parameters used to parse the string content</param>
        public void Error(object log, params object[] parameters)
        {
            m_currentColor = errorColor;
            this.Log(log, parameters);
            m_currentColor = baseColor;
        }
    }
}
