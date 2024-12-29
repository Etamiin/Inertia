using System;

namespace Inertia.Logging
{
    public class NullLogger : ILogger
    {
        public static NullLogger Instance => _lazy.Value;
        private static readonly Lazy<NullLogger> _lazy = new Lazy<NullLogger>(new NullLogger());

        internal NullLogger()
        {
        }

        public void Log(LogLevel level, object content)
        {
            //No implementation
        }
        public void Debug(object content)
        {
            //No implementation
        }
        public void Info(object content)
        {
            //No implementation
        }
        public void Warn(object content)
        {
            //No implementation
        }
        public void Error(object content)
        {
            //No implementation
        }
        public bool IsLogEnabled(LogLevel level)
        {
            return true;
        }
    }
}
