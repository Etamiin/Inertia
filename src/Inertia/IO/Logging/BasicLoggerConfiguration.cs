using System;
using System.Text;

namespace Inertia.Logging
{
    public sealed class BasicLoggerConfiguration
    {
        public bool ExecuteAsync { get; set; }
        public string? OutputFileName { get; set; }
        public bool OutputInConsole { get; set; }
        public Encoding TextEncoding { get; set; }
        public string? TimeFormat { get; set; }
        public bool AutoFlushInFile { get; set; } = true;

        public LogStyle Debug { get; set; } = new LogStyle("[Debug]", ConsoleColor.White);
        public LogStyle Warn { get; set; } = new LogStyle("[Warn]", ConsoleColor.Yellow);
        public LogStyle Error { get; set; } = new LogStyle("[Error]", ConsoleColor.Red);
    
        public BasicLoggerConfiguration()
        {
            TextEncoding = Encoding.UTF8;
            OutputInConsole = true;
        }
    }
}