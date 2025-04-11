using System;

namespace Inertia.Logging
{
    public static class LoggingProvider
    {
        public class NullLogHandler : ILogHandler
        {
            public void Log(LogLevel level, string message, Exception exception = null)
            {
                //
            }
        }

        public static ILogHandler LogHandler { get; set; } = new NullLogHandler();
    }
}
