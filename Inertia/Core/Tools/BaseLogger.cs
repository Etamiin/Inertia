using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Inertia
{
    /// <summary>
    /// Represent the default class for displaying logs
    /// </summary>
    public abstract class BaseLogger
    {
        /// <summary>
        /// Specify max saved log count (100 by default)
        /// </summary>
        public static int MaxCacheCount { get; set; } = 100;
        /// <summary>
        /// Specify if the <see cref="SaveCache"/> method is auto called when max cache capacity (<see cref="MaxCacheCount"/>) is reached
        /// </summary>
        public static bool AutoSaveCacheOnMaxCount { get; set; } = true;

        /// <summary>
        /// Set the specified <see cref="BaseLogger"/> logger used as default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T SetDefaultLogger<T>() where T : BaseLogger
        {
            var constrs = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)constrs[0].Invoke(new object[] { });
        }
        /// <summary>
        /// Register a new log pattern that can be used for logs
        /// </summary>
        /// <param name="type">Pattern type</param>
        /// <param name="onMessage"><see cref="BasicAction"/> to execute when a new log is create</param>
        public static void AddLoggerPattern(string type, BasicAction<string> onMessage)
        {
            if (m_patterns == null)
                m_patterns = new Dictionary<string, BasicAction<string>>();

            if (m_patterns.ContainsKey(type))
                m_patterns[type] = onMessage;
            else
                m_patterns.Add(type, onMessage);
        }
        /// <summary>
        /// Manually save cache
        /// </summary>
        public static void SaveCache()
        {
            if (m_cache == null)
                return;

            lock (m_cache)
            {
                var date = DateTime.Now;
                var name = date.Day + "." + date.Month + "." + date.Year + ".log";

                File.AppendAllLines(name, m_cache.ToArray());
                m_cache.Clear();
            }
        }

        internal static BaseLogger DefaultLogger
        {
            get
            {
                if (m_defaultLogger == null)
                    m_defaultLogger = SetDefaultLogger<ConsoleLogger>();

                return m_defaultLogger;
            }
            set
            {
                m_defaultLogger = value;
            }
        }
        private static Dictionary<string, BasicAction<string>> m_patterns;

        #region Private variables

        private static List<string> m_cache;
        private static BaseLogger m_defaultLogger;
        private static AutoQueueExecutor m_loggerQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="BaseLogger"/>
        /// </summary>
        public BaseLogger()
        {
            if (m_patterns == null)
                m_patterns = new Dictionary<string, BasicAction<string>>();
            if (m_loggerQueue == null)
                m_loggerQueue = new AutoQueueExecutor();

            OnInitialized();
        }

        #endregion

        /// <summary>
        /// This method is called when the class is instantiated
        /// </summary>
        protected virtual void OnInitialized() { }

        /// <summary>
        /// Call first pattern and use it to log (if exist)
        /// </summary>
        /// <param name="log">The string content of the log</param>
        /// <param name="parameters">The parameters used to parse the string content</param>
        public void Log(object log, params object[] parameters)
        {
            if (m_patterns.Count > 0)
            {
                m_loggerQueue.Enqueue(() => {
                    var cache = string.Format(log == null ? "NULL" : log.ToString(), parameters);

                    m_patterns.Values.ElementAt(0).Invoke(cache);
                    AddInCache(cache);
                });
            }
        }
        /// <summary>
        /// Call specified pattern and use it to log
        /// </summary>
        /// <param name="patternType">Pattern's type to use <see cref="AddLoggerPattern(string, BasicAction{string})"/></param>
        /// <param name="log">The string content of the log</param>
        /// <param name="parameters">The parameters used to parse the string content</param>
        public void LogPattern(string patternType, object log, params object[] parameters)
        {
            if (m_patterns.TryGetValue(patternType, out BasicAction<string> onMessage))
            {
                m_loggerQueue.Enqueue(() => {
                    var cache = string.Format(log.ToString(), parameters);

                    onMessage(cache);
                    AddInCache(cache);
                });
            }
        }

        private static void AddInCache(string log)
        {
            if (m_cache == null)
                m_cache = new List<string>(MaxCacheCount);

            if (m_cache.Count == m_cache.Capacity)
            {
                if (AutoSaveCacheOnMaxCount)
                    SaveCache();
                else
                    m_cache.Clear();
            }

            m_cache.Add(log);
        }
    }
}
