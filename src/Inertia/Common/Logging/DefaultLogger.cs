using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public sealed class DefaultLoggerOptions
    {
        public Encoding TextEncoding { get; set; }
        public LogLevel MinimumLogLevel { get; set; }
        public string? OutputFileName { get; private set; }
        public bool OutputInConsole { get; internal set; }
        public bool AutoFlushInFile { get; private set; }
        public string? TimeFormat { get; set; }
        public bool CatchUnhandledException { get; set; }
        public bool CatchUnobservedException { get; set; }

        public DefaultLoggerOptions() : this(null, false, true)
        {
        }

        public DefaultLoggerOptions(bool outputInConsole) : this(string.Empty, false, outputInConsole)
        {
        }

        public DefaultLoggerOptions(string outputFileName, bool autoFlushInFile) : this(outputFileName, autoFlushInFile, true)
        {
        }

        public DefaultLoggerOptions(string outputFileName, bool autoFlushInFile, bool outputInConsole)
        {
            TextEncoding = Encoding.UTF8;
            OutputFileName = outputFileName;
            OutputInConsole = outputInConsole;
            AutoFlushInFile = autoFlushInFile;
            TimeFormat = "[HH:mm:ss]";
            MinimumLogLevel = (LogLevel)Enum.GetValues(typeof(LogLevel)).Cast<byte>().Max();
            CatchUnhandledException = true;
            CatchUnobservedException = true;
        }
    }

    public sealed class DefaultLogger : ILogger, IDisposable
    {
        public bool IsDisposed { get; private set; }

        private DefaultLoggerOptions _options;
        private StreamWriter? _outputFileStream;
        private Stream? _outputConsoleStream;
        private bool _loggingDisabled;

        public DefaultLogger() : this(new DefaultLoggerOptions()) { }
        public DefaultLogger(DefaultLoggerOptions options)
        {
            SetOptions(options);
        }

        public void SetOptions(DefaultLoggerOptions options)
        {
            if (_options != null)
            {
                _outputFileStream?.Dispose();
                _outputConsoleStream?.Dispose();

                if (_options.CatchUnhandledException)
                {
                    AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                }

                if (_options.CatchUnobservedException)
                {
                    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
                }
            }

            _options = options;

            if (options.OutputInConsole)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(Console.Title))
                    {
                        Console.OutputEncoding = options.TextEncoding;
                        _outputConsoleStream = Console.OpenStandardOutput();
                    }
                }
                catch
                {
                    // Console not available
                }
            }

            if (!string.IsNullOrWhiteSpace(options.OutputFileName))
            {
                var outputFileInfo = new FileInfo(options.OutputFileName);

                if (!outputFileInfo.Directory.Exists) outputFileInfo.Directory.Create();
                if (!outputFileInfo.Exists) outputFileInfo.Create().Dispose();

                _outputFileStream = new StreamWriter(outputFileInfo.FullName, true, options.TextEncoding);
            }

            if (options.CatchUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            }

            if (options.CatchUnobservedException)
            {
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            }

            _loggingDisabled = _outputConsoleStream is null && _outputFileStream is null;
        }

        public void Log(LogLevel level, object content)
        {
            if (CannotLog(level)) return;

            var logBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_options.TimeFormat))
            {
                logBuilder.Append($"{DateTime.Now.ToString(_options.TimeFormat)} - ");
            }

            logBuilder.Append($"{level}: {content}{Environment.NewLine}");

            var logBytes = _options.TextEncoding.GetBytes(logBuilder.ToString());

            if (_options.OutputInConsole)
            {
                _outputConsoleStream?.Write(logBytes);
            }

            if (_outputFileStream != null)
            {
                _outputFileStream.BaseStream.Write(logBytes);
                if (_options.AutoFlushInFile)
                {
                    _outputFileStream.BaseStream.Flush();
                }
            }
        }
        public Task LogAsync(LogLevel level, object content)
        {
            if (CannotLog(level)) return Task.CompletedTask;

            return Task.Run(() => Log(level, content));
        }
        public void Debug(object content)
        {
            Log(LogLevel.Debug, content);
        }
        public Task DebugAsync(object content)
        {
            if (CannotLog(LogLevel.Debug)) return Task.CompletedTask;

            return Task.Run(() => Log(LogLevel.Debug, content));
        }
        public void Info(object content)
        {
            Log(LogLevel.Info, content);
        }
        public Task InfoAsync(object content)
        {
            if (CannotLog(LogLevel.Info)) return Task.CompletedTask;

            return Task.Run(() => Log(LogLevel.Info, content));
        }
        public void Warn(object content)
        {
            Log(LogLevel.Warn, content);
        }
        public Task WarnAsync(object content)
        {
            if (CannotLog(LogLevel.Warn)) return Task.CompletedTask;

            return Task.Run(() => Log(LogLevel.Warn, content));
        }
        public void Error(object content)
        {
            Log(LogLevel.Error, content);
        }
        public Task ErrorAsync(object content)
        {
            if (CannotLog(LogLevel.Error)) return Task.CompletedTask;

            return Task.Run(() => Log(LogLevel.Error, content));
        }
        public bool IsLogEnabled(LogLevel level)
        {
            return _options.MinimumLogLevel >= level;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool CannotLog(LogLevel level)
        {
            return IsDisposed || _loggingDisabled || !IsLogEnabled(level);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Error(e.ExceptionObject);
        }
        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Error(e.Exception);
            e.SetObserved();
        }
        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _outputFileStream?.Flush();
                _outputFileStream?.Dispose();
                _outputConsoleStream?.Dispose();

                IsDisposed = true;
            }
        }
    }
}