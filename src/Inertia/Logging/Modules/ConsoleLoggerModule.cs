using System;
using System.Text;

namespace Inertia.Logging
{
    public class ConsoleLoggerModule : ILoggerModule
    {
        public ConsoleLoggerModule() : this(Encoding.UTF8)
        {
        }
        public ConsoleLoggerModule(Encoding encoding)
        {
            Console.OutputEncoding = encoding;
        }

        public bool IsDisposed { get; private set; }

        public void Write(LogLevel level, string message)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (level < LogLevel.Warn)
            {
                Console.Write(message);
            }
            else
            {
                Console.Error.Write(message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        public void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsDisposed = true;
            }
        }
    }
}
