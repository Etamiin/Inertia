using Inertia.Scriptable;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Logging
{
    public class Logger : ILogger
    {
        public static ILogger Default { get; private set; }

        public static void SetDefault(ILogger logger)
        {
            Default = logger;
        }

        private LoggerParameters _parameters;
        private StreamWriter? _outputFileStream;
        private Stream? _outputConsoleStream;
        private AsyncExecutionQueuePool? _pool;
                
        static Logger()
        {
            Default = new Logger(new LoggerParameters());
        }
        public Logger(LoggerParameters parameters)
        {
            _parameters = parameters;

            if (parameters.ExecuteAsync)
            {
                _pool = RuntimeManager.QueuePool;
            }

            if (parameters.OutputInConsole)
            {
                Console.OutputEncoding = parameters.TextEncoding;
                _outputConsoleStream = Console.OpenStandardOutput();
            }

            if (!string.IsNullOrEmpty(parameters.OutputFileName))
            {
                var info = new FileInfo(parameters.OutputFileName);
                if (!info.Directory.Exists) info.Directory.Create();
                if (!info.Exists) info.Create().Dispose();

                _outputFileStream = new StreamWriter(info.FullName, true, parameters.TextEncoding);
            }
        }

        public virtual void Debug(object content)
        {
            LogLine(content, _parameters.Debug);
        }
        public virtual void Warn(object content)
        {
            LogLine(content, _parameters.Warn);
        }
        public virtual void Error(object content)
        {
            LogLine(content, _parameters.Error);
        }

        public void HandleUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        public void HandleUnobservedException()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void LogLine(object content, LoggerParameters.LogStyle logStyle)
        {
            if (_parameters.ExecuteAsync) _pool.Enqueue(Finalize);
            else Finalize();

            void Finalize()
            {
                var time = string.Empty;
                if (!string.IsNullOrEmpty(_parameters.TimeFormat))
                {
                    time = _parameters.TimeFormat;
                }

                var logBytes = _parameters.TextEncoding.GetBytes($"{time}{logStyle.Title} {content}{Environment.NewLine}");

                if (_parameters.OutputInConsole)
                {
                    Console.ForegroundColor = logStyle.Color;
                    _outputConsoleStream?.Write(logBytes);
                }
                if (_outputFileStream != null)
                {
                    _outputFileStream.BaseStream.Write(logBytes);
                    _outputFileStream.BaseStream.Flush();
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
    }
}