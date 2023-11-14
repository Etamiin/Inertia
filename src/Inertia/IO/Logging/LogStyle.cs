using System;

namespace Inertia.Logging
{
    public struct LogStyle : IEquatable<LogStyle>
    {
        public static LogStyle Empty => _empty;
        private static LogStyle _empty = new LogStyle(string.Empty, ConsoleColor.Black);

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