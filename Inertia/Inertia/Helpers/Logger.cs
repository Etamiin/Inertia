using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class Logger
    {
        public static event DebugHandler OnDebug = (log) => OnLogMethod(ConsoleColor.White, log);
        public static event DebugHandler OnError = (log) => OnLogMethod(ConsoleColor.Red, log);

        private static void OnLogMethod(ConsoleColor color, string log)
        {
            var currentColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ForegroundColor = currentColor;
        }

        public static void Log(object log)
        {
            OnDebug(log == null ? "null" : log.ToString());
        }
        public static void Error(object error)
        {
            OnError(error == null ? "null" : error.ToString());
        }
    }
}
