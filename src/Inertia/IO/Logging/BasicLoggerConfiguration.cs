using System;
using System.Text;

namespace Inertia.Logging
{
    public sealed class BasicLoggerConfiguration
    {
        public string? OutputFileName { get; private set; }
        public bool OutputInConsole { get; set; }
        public Encoding TextEncoding { get; set; }
        public string? TimeFormat { get; set; }
        public bool AutoFlushInFile { get; set; }
        public LogLevel MinimumLogLevel { get; set; }
        public bool HandleUnhandledException { get; set; }
        public bool HandleUnobservedException { get; set; }

        public BasicLoggerConfiguration() : this(string.Empty, false, true)
        {
        }
        public BasicLoggerConfiguration(string outputFileName, bool autoFlushInFile) : this(outputFileName, autoFlushInFile, true)
        {
        }
        public BasicLoggerConfiguration(string outputFileName, bool autoFlushInFile, bool outputInConsole)
        {
            TextEncoding = Encoding.UTF8;
            OutputFileName = outputFileName;
            OutputInConsole = outputInConsole;
            AutoFlushInFile = autoFlushInFile;
            TimeFormat = "[HH:mm:ss]";
            MinimumLogLevel = LogLevel.Debug;
            HandleUnhandledException = true;
            HandleUnobservedException = true;
        }
    }
}