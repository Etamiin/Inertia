using System;

namespace Inertia.Logging
{
    public static class LoggingProvider
    {
        public static ILogHandler LogHandler { get; set; } = new NullLogHandler();
    }
}
