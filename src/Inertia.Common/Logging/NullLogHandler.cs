using System;

namespace Inertia.Logging
{
    public class NullLogHandler : ILogHandler
    {
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            // No-op
        }
    }
}
