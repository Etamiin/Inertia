using System;
using System.Text;

namespace Inertia.Logging
{
    public sealed class LoggerParameters
    {
        public struct LogStyle
        {
            public readonly string Title;
            public readonly ConsoleColor Color;

            public LogStyle(string title, ConsoleColor color)
            {
                Title = title;
                Color = color;
            }
        }

        public bool ExecuteAsync { get; set; }
        public string OutputFileName { get; set; }
        public bool OutputInConsole { get; set; }
        public Encoding TextEncoding { get; set; }
        public string TimeFormat { get; set; }

        public LogStyle Debug { get; set; } = new LogStyle("[Debug]", ConsoleColor.White);
        public LogStyle Warn { get; set; } = new LogStyle("[Warn]", ConsoleColor.Yellow);
        public LogStyle Error { get; set; } = new LogStyle("[Error]", ConsoleColor.Red);
    
        public LoggerParameters()
        {
            TextEncoding = Encoding.UTF8;
            OutputInConsole = true;
        }
    }
}