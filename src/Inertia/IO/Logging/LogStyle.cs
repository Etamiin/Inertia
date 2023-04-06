using System;

namespace Inertia.Logging
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
}
