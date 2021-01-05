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


        private ConsoleColor BaseColor;
        private ConsoleColor SuccessColor;
        private ConsoleColor WarnColor;
        private ConsoleColor ErrorColor;

        #endregion

        #region Constructors

        internal ConsoleLogger()
        {
        }

        #endregion

        /// <summary>
        /// Initialize the current instance of <see cref="ConsoleLogger"/>
        /// </summary>
        protected override void OnInitialized()
        {
            BaseColor = ConsoleColor.White;
            SuccessColor = ConsoleColor.Green;
            WarnColor = ConsoleColor.DarkYellow;
            ErrorColor = ConsoleColor.Red;

            AddLoggerPattern("LOG", (log) => BaseLog("LOG", log, this.BaseColor));
            AddLoggerPattern("OK", (log) => BaseLog("OK", log, this.SuccessColor));
            AddLoggerPattern("WARN", (log) => BaseLog("WARN", log, this.WarnColor));
            AddLoggerPattern("ERROR", (log) => BaseLog("ERROR", log, this.ErrorColor));
        }

        private void BaseLog(string type, string log, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(string.Format("[{0}]: {1}", type, log));
        }
    }
}
