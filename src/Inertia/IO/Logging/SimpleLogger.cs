using Inertia.Scriptable;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public sealed class SimpleLogger : ILogger, IDisposable
    {
        public static ILogger Default { get; private set; }

        public static void SetDefault(ILogger logger)
        {
            Default = logger;
        }

        public bool IsDisposed { get; private set; }

        private readonly SimpleLoggerConfiguration _configuration;
        private readonly StreamWriter? _outputFileStream;
        private readonly Stream? _outputConsoleStream;
        private readonly AsyncExecutionQueuePool? _pool;
                
        static SimpleLogger()
        {
            Default = new SimpleLogger(new SimpleLoggerConfiguration());
        }
        public SimpleLogger(SimpleLoggerConfiguration configuration)
        {
            _configuration = configuration;

            if (configuration.ExecuteAsync)
            {
                _pool = RuntimeManager.QueuePool;
            }

            if (configuration.OutputInConsole)
            {
                Console.OutputEncoding = configuration.TextEncoding;
                _outputConsoleStream = Console.OpenStandardOutput();
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

        private void LogLine(object content, SimpleLoggerConfiguration.LogStyle logStyle)
        {
            if (_configuration.ExecuteAsync) _pool.Enqueue(Finalize);
            else Finalize();

            void Finalize()
            {
                var time = string.Empty;
                if (!string.IsNullOrWhiteSpace(_configuration.TimeFormat))
                {
                    time = DateTime.Now.ToString(_configuration.TimeFormat);
                }

                var logBytes = _configuration.TextEncoding.GetBytes($"{time}{logStyle.Title} {content}{Environment.NewLine}");

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