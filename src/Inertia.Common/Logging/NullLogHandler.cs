using System;

namespace Inertia.Logging
{
    public class NullLogHandler : ILogHandler
    {
        public void Log(LogLevel level, string message)
        {
            // No-op
        }
        public void Log(LogLevel level, string message, Exception exception)
        {
            // No-op
        }
    }
}
