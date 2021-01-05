using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Storage
{
    /// <summary>
    /// Represents the class for storing and managing files
    /// </summary>
    public class FileStorage : IStorage, IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when the current instance is saved by <see cref="SaveAsync(string, string)"/>
        /// </summary>
        public event BasicAction Saved = () => { };
        /// <summary>
        /// Occurs when the current instance is loaded by <see cref="LoadAsync(string)"/>
        /// </summary>
        public event BasicAction Loaded = () => { };
        /// <summary>
        /// Occurs when the current instance extracted all files by <see cref="ExtractAllAsync(string)"/>
        /// </summary>
        public event BasicAction Extracted = () => { };
        /// <summary>
        /// occurs when all files from a folder are added by <see cref="AddFolderAsync(string, bool)"/>
        /// </summary>
        public event BasicAction FolderAdded = () => { };
        /// <summary>
        /// Occurs when the save procedure progress
        /// </summary>
        public event StorageProgressHandler SaveProgress = (progression) => { };
        /// <summary>
        /// Occurs when the load procedure progress
        /// </summary>
        public event StorageProgressHandler LoadProgress = (progression) => { };
        /// <summary>
        /// Occurs when the extraction procedure progress
        /// </summary>
        public event StorageProgressHandler ExtractProgress = (progression) => { };
        /// <summary>
        /// Occurs when adding folder procedure progress
        /// </summary>
        public event StorageProgressHandler AddFolderProgress = (progression) => { };
        /// <summary>
        /// Occurs when the save procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler SaveFailed = (ex) => { };
        /// <summary>
        /// Occurs when the load procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler LoadFailed = (ex) => { };
        /// <summary>
        /// Occurs when the extraction procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler ExtractFailed = (ex) => { };
        /// <summary>
        /// Occurs when adding folder procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler AddingFolderFailed = (ex) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return the number of files contained in the current instance
        /// </summary>
        public int Count => m_files.Count;
        /// <summary>
        /// Get or set the password that will be used for the current instance
        /// </summary>
        public string Password { get; set; }

        #endregion

        #region Internal variables

        internal string LoadedPath { get; private set; }

        #endregion

        #region Private variables

        private Dictionary<string, FileStorageData> m_files;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="FileStorage"/>
        /// </summary>
        public FileStorage()
        {
            m_files = new Dictionary<string, FileStorageData>();
        }
        /// <summary>
        /// Initialize a new instance of the class <see cref="FileStorage"/>
        /// </summary>
        /// <param name="password">The password to use for the current instance</param>
        public FileStorage(string password) : this()
        {
            Password = password;
        }

        #endregion

        /// <summary>
        /// Return all the keys of all files contained by the current instance
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            return m_files.Keys.ToArray();
        }

        /// <summary>
        /// Add all files contained in the specified folder path
        /// </summary>
        /// <param name="folderPath">Folder path where to get files</param>
        /// <param name="inheritance">True if sub folders need to be added</param>
        /// <returns>The current instance</returns>
        public FileStorage AddFolder(string folderPath, bool inheritance)
        {
            folderPath = folderPath.VerifyPathForFolder();

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException();

            var files = InertiaIO.GetFilesPathFromDirectory(folderPath, inheritance);
            var progression = new StorageProgressionEventArgs(files.Length);

            foreach (var file in files)
            {
                var key = file.Replace(folderPath, string.Empty);

                if (!File.Exists(file))
                    throw new FileNotFoundException();

                if (m_files.ContainsKey(key))
                    throw new ArgumentException("The key already exist");

                m_files.Add(key, new FileStorageData(key, file));
                AddFolderProgress(progression.Progress());
            }

            return this;
        }
        /// <summary>
        /// Add all files contained in the specified folder path asynchronously
        /// </summary>
        /// <param name="folderPath">Folder path where to get files</param>
        /// <param name="inheritance">True if sub folders need to be added</param>
        /// <returns>The current instance</returns>
        public FileStorage AddFolderAsync(string folderPath, bool inheritance)
        {
            return StartAsync(() => AddFolder(folderPath, inheritance), FolderAdded, (ex) => AddingFolderFailed(ex));
        }
        /// <summary>
        /// Remove the file associated to the specified key from the current instance
        /// </summary>
        /// <param name="key">Target key to remove</param>
        /// <returns>The current instance</returns>
        public FileStorage Remove(string key)
        {
            if (m_files.ContainsKey(key))
                m_files.Remove(key);

            return this;
        }

        /// <summary>
        /// Extract the file associated to the specified key in the specified folder path
        /// </summary>
        /// <param name="key">Key to extract</param>
        /// <param name="folderPath">Path where to save the file</param>
        /// <returns>The current instance</returns>
        public FileStorage Extract(string key, string folderPath)
        {
            folderPath = folderPath.VerifyPathForFolder();

            var path = Path.Combine(folderPath, key);

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            File.WriteAllBytes(path, GetData(key));
            GC.Collect();

            return this;
        }
        /// <summary>
        /// Extract all the files of the current instance in the specified folder path
        /// </summary>
        /// <param name="folderPath">Path where to save the file</param>
        /// <returns>The current instance</returns>
        public FileStorage ExtractAll(string folderPath)
        {
            var keys = GetKeys();
            var progression = new StorageProgressionEventArgs(keys.Length);

            folderPath = folderPath.VerifyPathForFolder();
            foreach (var key in keys)
            {
                Extract(key, folderPath);
                ExtractProgress(progression.Progress());
            }

            return this;
        }
        /// <summary>
        /// Extract all the files of the current instance asynchronously in the specified folder path
        /// </summary>
        /// <param name="folderPath">Path where to save the file</param>
        /// <returns>The current instance</returns>
        public FileStorage ExtractAllAsync(string folderPath)
        {
            return StartAsync(() => ExtractAll(folderPath), Extracted, (ex) => ExtractFailed(ex));
        }

        /// <summary>
        /// Get the data from a stored file informations in the current instance
        /// </summary>
        /// <param name="key">Target key from where to get the data</param>
        /// <returns>The data from the target file</returns>
        public byte[] GetData(string key)
        {
            if (!m_files.ContainsKey(key))
                throw new KeyNotFoundException();

            var dataStorage = m_files[key];
            var data = dataStorage.GetData();
            var isLocal = !string.IsNullOrEmpty(dataStorage.SavedPath);

            if (!isLocal)
            {
                if (!string.IsNullOrEmpty(Password))
                    data = data.DecryptWithString(Password);

                if (dataStorage.IsCompressed)
                    data = data.Decompress();
            }

            return data;
        }

        /// <summary>
        /// Save the current storage to the specified folder path using the specified file name
        /// </summary>
        /// <param name="folderPath">Folder path where to save the file</param>
        /// <param name="fileName">Target file name</param>
        public void Save(string folderPath, string fileName)
        {
            folderPath = folderPath.VerifyPathForFolder();

            var path = Path.Combine(folderPath, fileName);
            var progression = new StorageProgressionEventArgs(Count);

            if (File.Exists(path))
                File.Delete(path);

            var writer = new BasicWriter()
                .SetInt(Count);

            File.WriteAllBytes(path, writer.ToArrayAndDispose());

            foreach (var pair in m_files)
            {
                var file = pair.Value;
                var fileWriter = new BasicWriter()
                    .SetString(file.Key);

                var iData = file.GetData();
                var compressed = file.IsCompressed;
                var isLocal = !string.IsNullOrEmpty(file.SavedPath);

                if (isLocal) {
                    var ciData = iData.Compress(out compressed);
                    if (compressed)
                        iData = ciData;
                }

                if (!string.IsNullOrEmpty(Password))
                    iData = iData.EncryptWithString(Password);

                fileWriter
                    .SetBool(compressed)
                    .SetLong(file.Length)
                    .SetLong(iData.LongLength)
                    .SetBytesWithoutHeader(iData);

                InertiaIO.AppendAllBytes(path, fileWriter.ToArrayAndDispose());
                GC.Collect();

                SaveProgress(progression.Progress());
            }
        }
        /// <summary>
        /// Save asynchronously the current storage to the specified folder path using the specified file name
        /// </summary>
        /// <param name="folderPath">Folder path where to save the file</param>
        /// <param name="fileName">Target file name</param>
        public void SaveAsync(string folderPath, string fileName)
        {
            StartAsync(() => Save(folderPath, fileName), Saved, (ex) => SaveFailed(ex));
        }
        /// <summary>
        /// Load the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            LoadedPath = filePath;
            m_files.Clear();

            Load(File.ReadAllBytes(filePath));
        }
        /// <summary>
        /// Load the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        public void Load(byte[] data)
        {
            var reader = new BasicReader(data);
            var count = reader.GetInt();
            var progression = new StorageProgressionEventArgs(count);

            for (var i = 0; i < count; i++)
            {
                var key = reader.GetString();
                var compressed = reader.GetBool();
                var realLength = reader.GetLong();
                var length = reader.GetLong();
                var startPosition = reader.Position;

                reader.Position += length;

                var storageData = new FileStorageData(this, key, compressed, realLength, length, startPosition);
                m_files.Add(storageData.Key, storageData);
                LoadProgress(progression.Progress());

                GC.Collect();
            }

            reader.Dispose();
        }
        /// <summary>
        /// Load asynchronously the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        public void LoadAsync(string filePath)
        {
            StartAsync(() => Load(filePath), Loaded, (ex) => LoadFailed(ex));
        }
        /// <summary>
        /// Load asynchronously the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        public void LoadAsync(byte[] data)
        {
            StartAsync(() => Load(data), Loaded, (ex) => LoadFailed(ex));
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            m_files.Clear();
            m_files = null;
            Password = null;
            Saved = null;
            Loaded = null;
            SaveFailed = null;
            LoadFailed = null;
        }
    
        private FileStorage StartAsync(BasicAction asyncAction, BasicAction successCallback, BasicAction<Exception> failedCallback)
        {
            Task.Factory.StartNew(() => { 
                try
                {
                    asyncAction();
                    successCallback();
                }
                catch (Exception ex)
                {
                    failedCallback(ex);
                }
            });

            return this;
        }
    }
}
