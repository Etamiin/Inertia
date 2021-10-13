using System;
using System.Collections.Generic;
using System.Text;
using Inertia;

/// <summary>
/// 
/// </summary>
public static class Log
{
    /// <summary>
    /// 
    /// </summary>
    public static bool UseAsyncQueue { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
    /// <summary>
    /// 
    /// </summary>
    public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;
    /// <summary>
    /// 
    /// </summary>
    public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// 
    /// </summary>
    public static string DefaultTitle { get; set; } = "[INFO]: ";
    /// <summary>
    /// 
    /// </summary>
    public static string WarnTitle { get; set; } = "[WARN]: ";
    /// <summary>
    /// 
    /// </summary>
    public static string ErrorTitle { get; set; } = "[ERROR]: ";

    private static AutoQueueExecutor _queue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    public static void Line(object content)
    {
        Line(content, DefaultTitle, DefaultColor);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    public static void Line(string content, params object[] args)
    {
        Line((object)string.Format(content, args), DefaultTitle, DefaultColor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    public static void Warn(object content)
    {
        Line(content, WarnTitle, WarnColor);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    public static void Warn(string content, params object[] args)
    {
        Line((object)string.Format(content, args), WarnTitle, WarnColor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    public static void Error(object content)
    {
        Line(content, ErrorTitle, ErrorColor);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
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