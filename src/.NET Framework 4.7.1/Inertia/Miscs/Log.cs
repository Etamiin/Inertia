﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Inertia;

public static class Log
{
    public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
    public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;    
    public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

    public static string DefaultTitle { get; set; } = "[INFO]: ";
    public static string WarnTitle { get; set; } = "[WARN]: ";
    public static string ErrorTitle { get; set; } = "[ERROR]: ";

    private static LogOptions _options;
    private static AutoQueueExecutor _queue;
    private static StringBuilder _log;
    private static DateTime _lastSaveTime;

    static Log()
    {
        SetOptions(LogOptions.Default);
        _lastSaveTime = DateTime.Now;
    }

    public static void SetOptions(LogOptions options)
    {
        _options = options;

        if (options.ExecuteAsync)
        {
            _queue = new AutoQueueExecutor();
        }
        else if (_queue != null)
        {
            _queue.Dispose();
            _queue = null;
        }

        if (options.SaveLog)
        {
            _log = new StringBuilder();
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            if (options.SaveLogTimerMs < LogOptions.MinimumSaveTimer)
            {
                options.SaveLogTimerMs = LogOptions.MinimumSaveTimer;
            }
        }
    }
    public static void SaveNow()
    {
        File.AppendAllText($"logs/log { _lastSaveTime: yyyy MM dd}.txt", _log.ToString());

        _lastSaveTime = DateTime.Now;
        _log.Clear();
    }

    public static void Line(object content, params object[] args)
    {
        Line(content, DefaultTitle, DefaultColor);
    }    
    public static void Warn(object content, params object[] args)
    {
        Line(content, WarnTitle, WarnColor);
    }    
    public static void Error(object content, params object[] args)
    {
        Line(content, ErrorTitle, ErrorColor);
    }

    private static void Line(object content, string title, ConsoleColor textColor)
    {
        if (_options.ExecuteAsync)
        {
            _queue.Enqueue(Finalize);
        }
        else
        {
            Finalize();
        }

        void Finalize()
        {
            var log = $"{ title }{ content }";

            Console.ForegroundColor = textColor;
            Console.WriteLine(log);

            if (textColor != DefaultColor)
            {
                Console.ForegroundColor = DefaultColor;
            }

            if (_options.SaveLog)
            {
                _log.AppendLine(log);

                var ts = DateTime.Now - _lastSaveTime;
                if (ts.TotalMilliseconds >= _options.SaveLogTimerMs)
                {
                    SaveNow();
                }
            }
        }
    }
}