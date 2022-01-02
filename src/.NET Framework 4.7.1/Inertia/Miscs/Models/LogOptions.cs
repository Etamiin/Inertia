using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class LogOptions
    {
        public static int MinimumSaveTimer { get; } = 10000;

        public static LogOptions Default
        {
            get
            {
                return new LogOptions
                {
                    ExecuteAsync = true,
                    IncludeTime = false,
                    SaveLog = false,
                    SaveLogTimerMs = MinimumSaveTimer
                };
            }
        }

        public bool ExecuteAsync { get; set; }
        public bool SaveLog { get; set; }
        public bool IncludeTime { get; set; }
        public int SaveLogTimerMs { get; set; }

        public ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        public ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

        public string DefaultTitle { get; set; } = "[INFO]: ";
        public string WarnTitle { get; set; } = "[WARN]: ";
        public string ErrorTitle { get; set; } = "[ERROR]: ";
    }
}