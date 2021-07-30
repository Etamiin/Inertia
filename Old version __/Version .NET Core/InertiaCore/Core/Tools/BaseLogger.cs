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
    ///
    /// </summary>
    public class BaseLogger
    {
        /// <summary>
        /// Specify the max cached logs
        /// </summary>
        public static int MaxCacheCapacity { get; set; } = 100;
        /// <summary>
        /// Specify if the <see cref="SaveCache"/> method is auto called when max cache capacity is reached
        /// </summary>
        public static bool AutoSaveCache { get; set; } = true;

        /// <summary>
        /// Get the current instance
        /// </summary>
        public static BaseLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BaseLogger();

                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        private static BaseLogger _instance;
        private Dictionary<string, BasicAction<string>> _patterns;
        private List<string> _cache;
        private AutoQueueExecutor _queue;

        internal BaseLogger()
        {
            _patterns = new Dictionary<string, BasicAction<string>>();
            _queue = new AutoQueueExecutor();
            _cache = new List<string>();

            this
                .AddLoggerPattern("LOG", (log) => DefaultPattern("LOG", log, ConsoleColor.White))
                .AddLoggerPattern("OK", (log) => DefaultPattern("OK", log, ConsoleColor.Green))
                .AddLoggerPattern("WARN", (log) => DefaultPattern("WARN", log, ConsoleColor.DarkYellow))
                .AddLoggerPattern("ERROR", (log) => DefaultPattern("ERROR", log, ConsoleColor.Red));
        }

        /// <summary>
        /// Register a new pattern, replace if the key already exist
        /// </summary>
        /// <param name="patternKey">Pattern key</param>
        /// <param name="onMessage"><see cref="BasicAction"/> to execute when a new log is created</param>
        public BaseLogger AddLoggerPattern(string patternKey, BasicAction<string> onMessage)
        {
            if (_patterns.ContainsKey(patternKey))
                _patterns[patternKey] = onMessage;
            else
                _patterns.Add(patternKey, onMessage);

            return this;
        }

        /// <summary>
        /// Call first pattern to log
        /// </summary>
        /// <param name="log"></param>
        /// <param name="args"></param>
        public void Log(object log, params object[] args)
        {
            if (_patterns.Count > 0)
            {
                _queue.Enqueue(() => {
                    var cache = string.Format(log == null ? "NULL" : log.ToString(), args);

                    _patterns.Values.ElementAt(0).Invoke(cache);
                    AddInCache(cache);
                });
            }
        }
        /// <summary>
        /// Call specified pattern to log
        /// </summary>
        /// <param name="patternKey">Pattern key to use</param>
        /// <param name="log"></param>
        /// <param name="args"></param>
        public void LogP(string patternKey, object log, params object[] args)
        {
            if (_patterns.TryGetValue(patternKey, out BasicAction<string> onMessage))
            {
                _queue.Enqueue(() => {
                    var cache = string.Format(log == null ? "NULL" : log.ToString(), args);

                    onMessage(cache);
                    AddInCache(cache);
                });
            }
        }

        /// <summary>
        /// Manually save cache
        /// </summary>
        public void SaveCache()
        {
            if (_cache.Count == 0)
                return;

            lock (_cache)
            {
                var date = DateTime.Now;
                var name = date.Day + "." + date.Month + "." + date.Year + ".log";

                File.AppendAllLines(name, _cache.ToArray());
                _cache.Clear();
            }
        }

        private void AddInCache(string log)
        {
            if (_cache.Count >= _cache.Capacity)
            {
                if (AutoSaveCache)
                    SaveCache();
                else
                    _cache.Clear();
            }

            _cache.Add(log);
        }
        private void DefaultPattern(string key, string log, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(string.Format("[{0}]: {1}", key, log));
            Console.ResetColor();
        }
    }
}
