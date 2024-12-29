using System;

namespace Inertia.Logging
{
    public interface ILoggerModule : IDisposable
    {
        void Write(LogLevel level, string message);
    }
}
