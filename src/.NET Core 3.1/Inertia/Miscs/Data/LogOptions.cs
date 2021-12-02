using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class LogOptions
    {
        public const int MinimumSaveTimer = 1000;

        public static LogOptions Default
        {
            get
            {
                return new LogOptions()
                {
                    ExecuteAsync = true,
                    SaveLog = false,
                    SaveLogTimerMs = MinimumSaveTimer
                };
            }
        }

        public bool ExecuteAsync { get; set; }
        public bool SaveLog { get; set; }
        public int SaveLogTimerMs { get; set; }
    }
}
