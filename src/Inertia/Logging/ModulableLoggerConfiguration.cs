namespace Inertia.Logging
{
    public class ModulableLoggerConfiguration
    {
        public ModulableLoggerConfiguration() : this(LogLevel.Debug)
        {
        }
        public ModulableLoggerConfiguration(LogLevel minimumLogLevel)
        {
            MinimumLogLevel = minimumLogLevel;
        }

        public LogLevel MinimumLogLevel { get; set; }
        public string? LevelFormat { get; set; }
        public string? TimeFormat { get; set; }
        public bool UseDefaultModules { get; set; } = true;
    }
}
