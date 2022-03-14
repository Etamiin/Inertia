using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class LogOptions
    {
        public struct LogType
        {
            public readonly string Title;
            public readonly ConsoleColor Color;

            public LogType(string title, ConsoleColor color)
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

        public LogType Line { get; set; } = new LogType("[INFO]:", ConsoleColor.White);
        public LogType Warn { get; set; } = new LogType("[WARN]:", ConsoleColor.Yellow);
        public LogType Error { get; set; } = new LogType("[ERROR]:", ConsoleColor.Red);
    
        public LogOptions()
        {
            TextEncoding = Encoding.UTF8;
        }
    }
}