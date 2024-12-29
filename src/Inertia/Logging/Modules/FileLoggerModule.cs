using System;
using System.IO;
using System.Text;

namespace Inertia.Logging
{
    public class FileLoggerModule : ILoggerModule
    {
        private FileStream _outputFileStream;
        private Encoding _encoding;

        public FileLoggerModule(string outputFilePath) : this(Encoding.UTF8, outputFilePath, true)
        {
        }
        public FileLoggerModule(string outputFilePath, bool autoFlushInFile) : this(Encoding.UTF8, outputFilePath, autoFlushInFile)
        {
        }
        public FileLoggerModule(Encoding encoding, string outputFilePath) : this(encoding, outputFilePath, true)
        {
        }
        public FileLoggerModule(Encoding encoding, string outputFilePath, bool autoFlushInFile)
        {
            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            var outputFileInfo = new FileInfo(outputFilePath);
            if (!outputFileInfo.Directory.Exists) outputFileInfo.Directory.Create();

            AutoFlushInFile = autoFlushInFile;
            _outputFileStream = new FileStream(outputFileInfo.FullName, FileMode.Append, FileAccess.Write);
            _encoding = encoding;
        }

        public bool AutoFlushInFile { get; set; }
        public bool IsDisposed { get; private set; }

        public void Write(LogLevel level, string message)
        {
            this.ThrowIfDisposable(IsDisposed);

            _outputFileStream.Write(_encoding.GetBytes(message));

            if (AutoFlushInFile) _outputFileStream.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        public void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _outputFileStream.Dispose();

                IsDisposed = true;
            }
        }
    }
}
