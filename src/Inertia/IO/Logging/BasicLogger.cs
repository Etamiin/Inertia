using System;
using System.IO;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public sealed class BasicLogger : ILogger, IDisposable
    {
        public static ILogger Default { get; private set; }

        public static void SetDefault(ILogger logger)
        {
            Default = logger;
        }

        public bool IsDisposed { get; private set; }

        private readonly BasicLoggerConfiguration _configuration;
        private readonly StreamWriter? _outputFileStream;
        private readonly Stream? _outputConsoleStream;
        private readonly AsyncTickedQueue? _asyncQueue;

        static BasicLogger()
        {
            Default = new BasicLogger(new BasicLoggerConfiguration());
        }

        public BasicLogger(BasicLoggerConfiguration configuration)
        {
            _configuration = configuration;

            if (configuration.ExecuteAsync)
            {
                _asyncQueue = new AsyncTickedQueue(100, TimeSpan.FromMilliseconds(200));
            }

            try
            {
                var consoleAvailable = !string.IsNullOrWhiteSpace(Console.Title);
                if (consoleAvailable && configuration.OutputInConsole)
                {
                    Console.OutputEncoding = configuration.TextEncoding;
                    _outputConsoleStream = Console.OpenStandardOutput();
                }
            }
            catch
            {
                configuration.OutputInConsole = false;
            }

            if (!string.IsNullOrWhiteSpace(configuration.OutputFileName))
            {
                var info = new FileInfo(configuration.OutputFileName);
                if (!info.Directory.Exists) info.Directory.Create();
                if (!info.Exists) info.Create().Dispose();

                _outputFileStream = new StreamWriter(info.FullName, true, configuration.TextEncoding);
            }
        }

        public void Debug(object content)
        {
            LogLine(content, _configuration.Debug);
        }
        public void Warn(object content)
        {
            LogLine(content, _configuration.Warn);
        }
        public void Error(object content)
        {
            LogLine(content, _configuration.Error);
        }

        public void HandleUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        public void HandleUnobservedException()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void LogLine(object content, LogStyle logStyle)
        {
            if (_asyncQueue != null) _asyncQueue.Enqueue(Log);
            else Log();

            void Log()
            {
                var time = string.Empty;
                if (!string.IsNullOrWhiteSpace(_configuration.TimeFormat))
                {
                    time = DateTime.Now.ToString(_configuration.TimeFormat);
                }

                var logStr = string.Format(_configuration.LogFormat, time, logStyle.Title, $"{content}{Environment.NewLine}");
                var logBytes = _configuration.TextEncoding.GetBytes(logStr);

                if (_configuration.OutputInConsole)
                {
                    Console.ForegroundColor = logStyle.Color;
                    _outputConsoleStream?.Write(logBytes);
                }

                if (_outputFileStream != null)
                {
                    _outputFileStream.BaseStream.Write(logBytes);
                    if (_configuration.AutoFlushInFile)
                    {
                        _outputFileStream.BaseStream.Flush();
                    }
                }
            }
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Error(e.Exception);
            e.SetObserved();
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Error(e.ExceptionObject);
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed || this == Default) return;

            if (disposing)
            {
                _outputFileStream?.Flush();
                _outputFileStream?.Dispose();
                _outputConsoleStream?.Dispose();
            }

            IsDisposed = true;
        }
    }
}