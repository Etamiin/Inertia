namespace Inertia.Logging
{
    public static class LoggingProvider
    {
        public static ILogger Logger { get; set; } = NullLogger.Instance;
    }
}
