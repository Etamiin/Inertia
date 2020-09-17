using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Inertia
{
    /// <summary>
    /// Represent the default class for displaying logs
    /// </summary>
    public abstract class BaseLogger
    {
        /// <summary>
        /// Return the specified <typeparamref name="T"/> instance
        /// </summary>
        /// <typeparam name="T">The target <see cref="BaseLogger"/> derivation class</typeparam>
        /// <returns>The target logger instance</returns>
        public static T GetLogger<T>() where T : BaseLogger
        {
            var cs = typeof(T).GetConstructors();

            return (T)cs[0].Invoke(new object[] { });
        }

        #region Public variables

        /// <summary>
        /// This is the default logger used in internal architecture for displaying logs, by default the class used is <see cref="ConsoleLogger"/>
        /// </summary>
        public static BaseLogger DefaultLogger
        {
            get
            {
                if (m_defaultLogger == null)
                    m_defaultLogger = GetLogger<ConsoleLogger>();

                return m_defaultLogger;
            }
            set
            {
                m_defaultLogger = value;
            }
        }

        #endregion

        #region Private variables

        private static BaseLogger m_defaultLogger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="BaseLogger"/>
        /// </summary>
        public BaseLogger()
        {
            OnInitialized();
        }

        #endregion

        /// <summary>
        /// This method is called when the class is instantiated
        /// </summary>
        protected virtual void OnInitialized() { }
        /// <summary>
        /// This method is called each time a new log is parsed
        /// </summary>
        /// <param name="parsedLog">The final parsed log</param>
        protected abstract void OnLog(string parsedLog);

        /// <summary>
        /// Create a new log
        /// </summary>
        /// <param name="log">The string content of the log</param>
        /// <param name="parameters">The parameters used to parse the string content</param>
        public void Log(object log, params object[] parameters)
        {
            var parsedLog = log.ToString();

            if (log.GetType().IsArray)
                parsedLog = ParseArray((Array)log);

            for (var i = 0; i < parameters.Length; i++)
            {
                var identifier = "{" + i + "}";

                if (parameters[i].GetType().IsArray) {
                    var array = (Array)parameters[i];
                    parsedLog = parsedLog.Replace(identifier, ParseArray(array));

                    continue;
                }

                parsedLog = parsedLog.Replace(identifier, parameters[i].ToString());
            }

            OnLog(parsedLog);
        }

        private string ParseArray(Array array)
        {
            var parsedArray = "[";

            for (var j = 0; j < array.Length; j++)
            {
                parsedArray += array.GetValue(j).ToString();
                if (j < array.Length - 1)
                    parsedArray += ", ";
            }

            return parsedArray + "]";
        }
    }
}
