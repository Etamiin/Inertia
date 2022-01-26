using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Inertia;

public static class Log
{
    private static LogOptions _options;
    private static ExecutorPool _pool;
    private static StringBuilder _log;
    private static DateTime _lastSaveTime;

    static Log()
    {
        SetOptions(LogOptions.Default);
        _lastSaveTime = DateTime.Now;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    public static void SetOptions(LogOptions options)
    {
        _options = options;

        if (options.ExecuteAsync)
        {
            _pool = new ExecutorPool(100, true);
        }
        else if (_pool != null)
        {
            _pool.Dispose();
            _pool = null;
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
        if (_log != null)
        {
            lock (_log)
            {
                File.AppendAllTextAsync($"logs/log { _lastSaveTime: yyyy MM dd}.txt", _log.ToString());

                _lastSaveTime = DateTime.Now;
                _log.Clear();
            }
        }
    }

    public static void Line(object content, params object[] args)
    {
        Line(content, _options.DefaultTitle, _options.DefaultColor);
    }    
    public static void Warn(object content, params object[] args)
    {
        Line(content, _options.WarnTitle, _options.WarnColor);
    }    
    public static void Error(object content, params object[] args)
    {
        Line(content, _options.ErrorTitle, _options.ErrorColor);
    }

    private static void Line(object content, string title, ConsoleColor textColor)
    {
        if (_options.ExecuteAsync)
        {
            _pool.Enqueue(Finalize);
        }
        else
        {
            Finalize();
        }

        void Finalize()
        {
            var time = _options.IncludeTime ? $"[{DateTime.Now.ToLongTimeString()}]" : string.Empty;
            var log = $"{time}{ title }{ content }";

            Console.ForegroundColor = textColor;
            Console.WriteLine(log);

            if (textColor != _options.DefaultColor)
            {
                Console.ForegroundColor = _options.DefaultColor;
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

    private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        Error(e.Exception);
        e.SetObserved();
    }
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Error((Exception)e.ExceptionObject);

        if (e.IsTerminating)
        {
            _pool?.ForceExecution();
            SaveNow();
        }
    }
}