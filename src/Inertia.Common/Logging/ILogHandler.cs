using System;

namespace Inertia.Logging
{
    public interface ILogHandler
    {
        void Log(LogLevel level, string message);
        void Log(LogLevel level, string message, Exception exception);
    }
}
