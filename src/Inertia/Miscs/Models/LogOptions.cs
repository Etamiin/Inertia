using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class LogOptions
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

        public static LogOptions Default
        {
            get
            {
                return new LogOptions
                {
                    ExecuteAsync = true,
                    OutputFileName = string.Empty,
                    OutputInConsole = true
                };
            }
        }

        public bool ExecuteAsync { get; set; }
        public string OutputFileName { get; set; }
        public bool IncludeTime { get; set; }
        public bool OutputInConsole { get; set; }
        public Encoding TextEncoding { get; set; }

        public LogStyle Line { get; set; } = new LogStyle("[Info]:", ConsoleColor.White);
        public LogStyle Warn { get; set; } = new LogStyle("[Warn]:", ConsoleColor.Yellow);
        public LogStyle Error { get; set; } = new LogStyle("[Error]:", ConsoleColor.Red);
    
        public LogOptions()
        {
            TextEncoding = Encoding.UTF8;
        }
    }
}