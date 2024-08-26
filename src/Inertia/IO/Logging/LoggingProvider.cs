namespace Inertia.Logging
{
    public static class LoggingProvider
    {
        public static ILogger Logger
        {
            get
            {
                return _logger;
            }
            set
            {
                if (value == null)
                {
                    value = NullLogger.Instance;
                }

                _logger = value;
            }
        }

        private static ILogger _logger = new BasicLogger(new BasicLoggerSettings());
    }
}
