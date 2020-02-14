using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Inertia.Internal;

namespace Inertia.Storage
{
    public class FileMemoryData : IDisposable
    {
        #region Public variables

        public FileStorage Storage;
        public string Name;

        #endregion

        #region Internal variables

        internal FileMemoryStorageType StorageType;
        internal bool IsDirectoryChild;
        internal string DirectoryPath;
        internal string FilePath;
        internal byte[] FileData;

        #endregion

        #region Constructors

        internal FileMemoryData(FileStorage storage, string path) : this(storage, path, string.Empty)
        {
        }
        internal FileMemoryData(FileStorage storage, string path, string dirPath)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File [" + path + "] - don't exist");

            var info = new FileInfo(path);

            Storage = storage;
            Name = info.Name;
            StorageType = FileMemoryStorageType.Internal;
            FilePath = path;
            if (!string.IsNullOrEmpty(dirPath))
            {
                IsDirectoryChild = true;
                DirectoryPath = dirPath;
                FilePath = path.Replace(dirPath + @"\", "");
                Name = FilePath;
            }
        }
        internal FileMemoryData(FileStorage storage, string name, byte[] data)
        {
            Storage = storage;
            Name = name;
            StorageType = FileMemoryStorageType.Manual;
            FileData = data;
        }
        internal FileMemoryData(FileStorage storage, string name, byte[] data, bool isDirectoryChild) : this(storage, name, data)
        {
            IsDirectoryChild = isDirectoryChild;
            if (isDirectoryChild)
                FilePath = name;
        }

        #endregion

        public byte[] GetData()
        {
            switch (StorageType)
            {
                case FileMemoryStorageType.Internal:
                    var targetPath = IsDirectoryChild ? Path.Combine(DirectoryPath, FilePath) : FilePath;

                    if (!File.Exists(targetPath))
                        throw new FileNotFoundException("File [" + targetPath + "] - deleted");
                    return File.ReadAllBytes(targetPath);
                case FileMemoryStorageType.Manual:
                    return FileData;
                default:
                    return null;
            }
        }

        public bool ExtractTo(string directoryPath)
        {
            try
            {
                var path = Path.Combine(directoryPath, Name);
                var info = new FileInfo(path);

                if (!info.Directory.Exists)
                    info.Directory.Create();

                File.WriteAllBytes(path, GetData());

                return true;
            }
            catch (Exception e)
            {
                Storage.OnFileExtractedFailed(this, e);
                return false;
            }
        }
        public void ExtractToAsync(string directoryPath)
        {
            Task.Factory.StartNew(() => {
                ExtractTo(directoryPath);
                Storage.OnFileExtracted(this);
            });
        }

        public void Dispose()
        {
            Storage = null;
            Name = null;
            DirectoryPath = null;
            FilePath = null;
            FileData = null;
        }
    }
}
