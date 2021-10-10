using System;
using System.Collections.Generic;
using System.Text;
using Inertia;

public static class Log
{
    public static bool UseAsyncQueue { get; set; } = true;

    public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
    public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;
    public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

    public static string DefaultTitle { get; set; } = "[INFO]: ";
    public static string WarnTitle { get; set; } = "[WARN]: ";
    public static string ErrorTitle { get; set; } = "[ERROR]: ";

    private static AutoQueueExecutor _queue;

    public static void Line(object content)
    {
        Line(content, DefaultTitle, DefaultColor);
    }
    public static void Line(string content, params object[] args)
    {
        Line((object)string.Format(content, args), DefaultTitle, DefaultColor);
    }

    public static void Warn(object content)
    {
        Line(content, WarnTitle, WarnColor);
    }
    public static void Warn(string content, params object[] args)
    {
        Line((object)string.Format(content, args), WarnTitle, WarnColor);
    }

    public static void Error(object content)
    {
        Line(content, ErrorTitle, ErrorColor);
    }
    public static void Error(string content, params object[] args)
    {
        Line((object)string.Format(content, args), ErrorTitle, ErrorColor);
    }

    private static void Line(object content, string title, ConsoleColor textColor)
    {
        if (UseAsyncQueue)
        {
            if (_queue == null)
            {
                _queue = new AutoQueueExecutor();
            }

            _queue.Enqueue(Finalize);
        }
        else
        {
            Finalize();
        }

        void Finalize()
        {
            Console.ForegroundColor = textColor;
            Console.WriteLine($"{ title }{ content }");

            if (textColor != DefaultColor)
            {
                Console.ForegroundColor = DefaultColor;
            }
        }
    }
}