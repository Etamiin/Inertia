using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Inertia;

public static class Log
{
    private static LogOptions _options;
    private static ExecutorPool _pool;
    private static StreamWriter _outputFileStream;
    private static Stream _outputConsoleStream;

    static Log()
    {
        SetOptions(LogOptions.Default);

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    public static void SetOptions(LogOptions options)
    {
        _options = options;

        if (options.ExecuteAsync)
        {
            if (_pool == null)
            {
                _pool = new ExecutorPool(1000, true);
            }
        }
        else if (_pool != null)
        {
            _pool.Dispose();
            _pool = null;
        }

        if (options.OutputInConsole)
        {
            _outputConsoleStream = Console.OpenStandardOutput();
        }
        else if (_outputConsoleStream != null)
        {
            _outputConsoleStream.Close();
        }

        if (!string.IsNullOrEmpty(options.OutputFileName))
        {
            if (_outputFileStream != null)
            {
                _outputFileStream.Close();
            }

            var info = new FileInfo(options.OutputFileName);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }

            _outputFileStream = File.CreateText(options.OutputFileName);
        }
    }

    public static void Line(object content)
    {
        GenericLine(content, _options.Line);
    }    
    public static void Warn(object content)
    {
        GenericLine(content, _options.Warn);
    }    
    public static void Error(object content)
    {
        GenericLine(content, _options.Error);
    }

    private static void GenericLine(object content, LogOptions.LogType logType)
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
            var logBytes = Encoding.UTF8.GetBytes($"{time}{ logType.Title } { content }{Environment.NewLine}");

            if (_options.OutputInConsole)
            {
                Console.ForegroundColor = logType.Color;
                _outputConsoleStream.Write(logBytes, 0, logBytes.Length);
            }
            if (_outputFileStream != null)
            {
                _outputFileStream.BaseStream.Write(logBytes, 0, logBytes.Length);
                _outputFileStream.BaseStream.Flush();
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
            _pool?.ExecuteAllAndDispose();
        }
    }
}