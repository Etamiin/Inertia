using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Storage;

namespace Inertia.Internal
{
    internal class FileStorageData : IDisposable
    {
        #region Public variables

        public string Key { get; private set; }
        public string SavedPath { get; private set; }
        public bool IsCompressed { get; private set; }
        public long Length { get; private set; }

        #endregion

        #region Internal variables

        internal long DataPosition { get; private set; }

        #endregion

        #region Private variables

        private readonly long m_compressedLength;
        private FileStorage m_storage;

        #endregion

        #region Constructors

        public FileStorageData(string key, string filePath)
        {
            var info = new FileInfo(filePath);

            Key = key;
            SavedPath = filePath;
            Length = info.Length;
        }
        public FileStorageData(FileStorage storage, string key, bool compressed, long length, long cLength, long position)
        {
            Key = key;
            IsCompressed = compressed;
            Length = length;
            DataPosition = position;
            m_compressedLength = cLength;
            m_storage = storage;
        }

        #endregion

        public byte[] GetData()
        {
            if (!string.IsNullOrEmpty(SavedPath))
                return File.ReadAllBytes(SavedPath);
            else {
                if (!File.Exists(m_storage.LoadedPath))
                    throw new FileNotFoundException("The file storage don't exist at path: " + m_storage.LoadedPath);

                using (var stream = new FileStream(m_storage.LoadedPath, FileMode.Open))
                {
                    stream.Position = DataPosition;

                    var data = new byte[m_compressedLength];
                    var readedLength = stream.Read(data, 0, data.Length);

                    if (readedLength != data.Length)
                        throw new EndOfStreamException("Cannot read the file storage");

                    return data;
                }
            }
        }

        public void Dispose()
        {
            Key = null;
            SavedPath = null;
            m_storage = null;
        }
    }
}
