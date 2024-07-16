using System;
using System.IO;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public sealed class BasicLogger : ILogger, IDisposable
    {
        public bool IsDisposed { get; private set; }

        private readonly BasicLoggerConfiguration _configuration;
        private readonly StreamWriter? _outputFileStream;
        private readonly Stream? _outputConsoleStream;

        public BasicLogger(BasicLoggerConfiguration configuration)
        {
            _configuration = configuration;

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
                var outputFileInfo = new FileInfo(configuration.OutputFileName);

                if (!outputFileInfo.Directory.Exists) outputFileInfo.Directory.Create();
                if (!outputFileInfo.Exists) outputFileInfo.Create().Dispose();

                _outputFileStream = new StreamWriter(outputFileInfo.FullName, true, configuration.TextEncoding);
            }

            if (configuration.HandleUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            if (configuration.HandleUnobservedException)
            {
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            }
        }

        public void Debug(object content)
        {
            LogLine(LogLevel.Debug, content);
        }
        public void DebugAsync(object content)
        {
            if (_configuration.MinimumLogLevel > LogLevel.Debug) return;

            Task.Run(() => LogLine(LogLevel.Debug, content));
        }
        public void Info(object content)
        {
            LogLine(LogLevel.Info, content);
        }
        public void InfoAsync(object content)
        {
            if (_configuration.MinimumLogLevel > LogLevel.Info) return;

            Task.Run(() => LogLine(LogLevel.Info, content));
        }
        public void Warn(object content)
        {
            LogLine(LogLevel.Warn, content);
        }
        public void WarnAsync(object content)
        {
            if (_configuration.MinimumLogLevel > LogLevel.Warn) return;

            Task.Run(() => LogLine(LogLevel.Warn, content));
        }
        public void Error(object content)
        {
            LogLine(LogLevel.Error, content);
        }
        public void ErrorAsync(object content)
        {
            if (_configuration.MinimumLogLevel > LogLevel.Error) return;

            Task.Run(() => LogLine(LogLevel.Error, content));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void LogLine(LogLevel level, object content)
        {
            if (_configuration.MinimumLogLevel > level) return;

            var logTime = string.Empty;
            if (!string.IsNullOrWhiteSpace(_configuration.TimeFormat))
            {
                logTime = $"{DateTime.Now.ToString(_configuration.TimeFormat)} - ";
            }

            var logStr = $"{logTime}{level}: {content}{Environment.NewLine}";
            var logBytes = _configuration.TextEncoding.GetBytes(logStr);

            if (_configuration.OutputInConsole)
            {
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
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Error(e.ExceptionObject);
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Error(e.Exception);
            e.SetObserved();
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

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