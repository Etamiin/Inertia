using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

public class CustomLogger : BaseLogger
{

    protected override void OnInitialized()
    {
        //logger initialized

        //Create a new logger's pattern named "Example"
        //have fun with your logger
        AddLoggerPattern("Example", (log) => {
            Console.Write("[");
            OnColor(ConsoleColor.Red, "E");
            OnColor(ConsoleColor.Green, "X");
            OnColor(ConsoleColor.Yellow, "A");
            OnColor(ConsoleColor.Blue, "M");
            OnColor(ConsoleColor.Magenta, "P");
            OnColor(ConsoleColor.Cyan, "L");
            OnColor(ConsoleColor.DarkGreen, "E");
            Console.Write("]: " + log);
            Console.Beep();
            Console.WriteLine();
        });

        //Create a new logger's pattern named "OK"
        AddLoggerPattern("OK", (log) => {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.WriteLine(log);
            Console.ResetColor();
        });
    }

    private void OnColor(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}