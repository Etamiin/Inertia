using System;

namespace Inertia.Logging
{
    public struct LogStyle : IEquatable<LogStyle>
    {
        public readonly string Title;
        public readonly ConsoleColor Color;

        public LogStyle(string title, ConsoleColor color)
        {
            Title = title;
            Color = color;
        }

        public bool Equals(LogStyle other)
        {
            return other.Title.Equals(Title) && other.Color == Color;
        }
    }
}