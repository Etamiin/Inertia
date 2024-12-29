using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Logging
{
    public sealed class ModulableLogger : InertiaService<ModulableLoggerConfiguration>, ILogger, IDisposable
    {
        internal static List<ILoggerModule> DefaultModules { get; } = new List<ILoggerModule>();

        private List<ILoggerModule> _modules;
        private LogLevel _minimumLogLevel { get; set; }
        private string? _levelFormat;
        private string? _timeFormat;
        private bool _useDefaultModules;

        public ModulableLogger()
        {
            _modules = new List<ILoggerModule>();
        }
        public ModulableLogger(ModulableLoggerConfiguration configuration) : this()
        {
            Configure(configuration);
        }

        public bool IsDisposed { get; private set; }

        public override void Configure(ModulableLoggerConfiguration configuration)
        {
            _minimumLogLevel = configuration.MinimumLogLevel;
            _levelFormat = configuration.LevelFormat;
            _timeFormat = configuration.TimeFormat;
            _useDefaultModules = configuration.UseDefaultModules;
        }

        public ModulableLogger AddModule(ILoggerModule module)
        {
            this.ThrowIfDisposable(IsDisposed);

            _modules.Add(module);

            return this;
        }

        public void Log(LogLevel level, object content)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!IsLogEnabled(level)) return;

            var logBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_timeFormat))
            {
                logBuilder.Append($"{DateTime.Now.ToString(_timeFormat)} ");
            }

            if (!string.IsNullOrWhiteSpace(_levelFormat))
            {
                logBuilder.Append($"{string.Format(_levelFormat, level)} ");
            }

            logBuilder.Append($"{content}{Environment.NewLine}");

            var message = logBuilder.ToString();

            foreach (var module in _modules)
            {                
                module.Write(level, message);
            }

            if (_useDefaultModules)
            {
                foreach (var module in DefaultModules)
                {
                    module.Write(level, message);
                }
            }
        }
        public void Debug(object content)
        {
            Log(LogLevel.Debug, content);
        }
        public void Info(object content)
        {
            Log(LogLevel.Info, content);
        }
        public void Warn(object content)
        {
            Log(LogLevel.Warn, content);
        }
        public void Error(object content)
        {
            Log(LogLevel.Error, content);
        }
        public bool IsLogEnabled(LogLevel level)
        {
            return level >= _minimumLogLevel;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        public void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                foreach (var module in _modules)
                {
                    module.Dispose();
                }

                _modules.Clear();

                IsDisposed = true;
            }
        }
    }
}
