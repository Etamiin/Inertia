using System;
using System.IO;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public sealed class BasicLogger : ILogger, IDisposable
    {
        public bool IsDisposed { get; private set; }

        private readonly BasicLoggerSettings _settings;
        private readonly StreamWriter? _outputFileStream;
        private readonly Stream? _outputConsoleStream;

        public BasicLogger(BasicLoggerSettings settings)
        {
            _settings = settings;

            try
            {
                var consoleAvailable = !string.IsNullOrWhiteSpace(Console.Title);
                if (consoleAvailable && settings.OutputInConsole)
                {
                    Console.OutputEncoding = settings.TextEncoding;
                    _outputConsoleStream = Console.OpenStandardOutput();
                }
            }
            catch
            {
                settings.OutputInConsole = false;
            }

            if (!string.IsNullOrWhiteSpace(settings.OutputFileName))
            {
                var outputFileInfo = new FileInfo(settings.OutputFileName);

                if (!outputFileInfo.Directory.Exists) outputFileInfo.Directory.Create();
                if (!outputFileInfo.Exists) outputFileInfo.Create().Dispose();

                _outputFileStream = new StreamWriter(outputFileInfo.FullName, true, settings.TextEncoding);
            }

            if (settings.HandleUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            if (settings.HandleUnobservedException)
            {
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            }
        }

        public void Debug(object content)
        {
            LogLine(LogLevel.Debug, content);
        }
        public Task DebugAsync(object content)
        {
            if (_settings.MinimumLogLevel > LogLevel.Debug) return Task.CompletedTask;

            return Task.Run(() => LogLine(LogLevel.Debug, content));
        }
        public void Info(object content)
        {
            LogLine(LogLevel.Info, content);
        }
        public Task InfoAsync(object content)
        {
            if (_settings.MinimumLogLevel > LogLevel.Info) return Task.CompletedTask;

            return Task.Run(() => LogLine(LogLevel.Info, content));
        }
        public void Warn(object content)
        {
            LogLine(LogLevel.Warn, content);
        }
        public Task WarnAsync(object content)
        {
            if (_settings.MinimumLogLevel > LogLevel.Warn) return Task.CompletedTask;

            return Task.Run(() => LogLine(LogLevel.Warn, content));
        }
        public void Error(object content)
        {
            LogLine(LogLevel.Error, content);
        }
        public Task ErrorAsync(object content)
        {
            if (_settings.MinimumLogLevel > LogLevel.Error) return Task.CompletedTask;

            return Task.Run(() => LogLine(LogLevel.Error, content));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void LogLine(LogLevel level, object content)
        {
            if (IsDisposed || _settings.MinimumLogLevel > level) return;

            var logTime = string.Empty;
            if (!string.IsNullOrWhiteSpace(_settings.TimeFormat))
            {
                logTime = $"{DateTime.Now.ToString(_settings.TimeFormat)} - ";
            }

            var logStr = $"{logTime}{level}: {content}{Environment.NewLine}";
            var logBytes = _settings.TextEncoding.GetBytes(logStr);

            if (_settings.OutputInConsole)
            {
                _outputConsoleStream?.Write(logBytes);
            }

            if (_outputFileStream != null)
            {
                _outputFileStream.BaseStream.Write(logBytes);
                if (_settings.AutoFlushInFile)
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