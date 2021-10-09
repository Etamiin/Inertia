using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.ConsoleToolKit
{
    public sealed class BasicConsoleLogger
    {
        private ConsoleColor _defaultTextColor;

        public BasicConsoleLogger()
        {
            _defaultTextColor = ConsoleColor.White;
        }
        public BasicConsoleLogger(ConsoleColor DefaultTextColor)
        {
            _defaultTextColor = DefaultTextColor;
            Console.ForegroundColor = _defaultTextColor;
        }

        public void Line(object content)
        {
            Console.WriteLine(content);
        }
        public void Line(string content, params object[] args)
        {
            Line((object)string.Format(content, args));
        }
        public void Line(object content, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            Console.WriteLine(content);

            Console.ForegroundColor = _defaultTextColor;
        }
        public void Line(string content, ConsoleColor textColor, params object[] args)
        {
            Line((object)string.Format(content, args), textColor);
        }
    }
}
