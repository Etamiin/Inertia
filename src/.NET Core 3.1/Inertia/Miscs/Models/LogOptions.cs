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
    }
}
