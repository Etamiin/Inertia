using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    /// <summary>
    /// Represents the class for storing and managing datas using <see cref="FlexDictionary{KeyType}"/>
    /// </summary>
    /// <typeparam name="KeyType">Target key <see cref="Type"/></typeparam>
    public class DataStorage<KeyType> : FlexDictionary<KeyType>, IStorage, IDisposable
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
        /// Occurs when the save procedure progress
        /// </summary>
        public event StorageProgressHandler SaveProgress = (progression) => { };
        /// <summary>
        /// Occurs when the load procedure progress
        /// </summary>
        public event StorageProgressHandler LoadProgress = (progression) => { };
        /// <summary>
        /// Occurs when the save procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler SaveFailed = (ex) => { };
        /// <summary>
        /// Occurs when the load procedure failed
        /// </summary>
        public event StorageUpdateFailedHandler LoadFailed = (ex) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Get or set the password that will be used for the current instance
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AutoCompression { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="DataStorage{KeyType}"/>
        /// </summary>
        public DataStorage(bool autoCompression = false) : base()
        {
            AutoCompression = autoCompression;
        }
        /// <summary>
        /// Initialize a new instance of the class <see cref="DataStorage{KeyType}"/>
        /// </summary>
        /// <param name="password">The password to use in the current instance</param>
        /// <param name="autoCompression"></param>
        public DataStorage(string password, bool autoCompression = false) : base()
        {
            Password = password;
            AutoCompression = autoCompression;
        }

        #endregion

        internal override byte[] Serialize(bool autoCompression, BasicAction<StorageProgressionEventArgs> progressCallback)
        {
            if (!string.IsNullOrEmpty(Password)) {
                return base
                    .Serialize(autoCompression, progressCallback)
                    .EncryptWithString(Password);
            }
            else
                return base.Serialize(autoCompression, progressCallback);
        }
        internal override void Deserialize(bool autoCompression, byte[] data, BasicAction<StorageProgressionEventArgs> progressCallback)
        {
            if (!string.IsNullOrEmpty(Password))
            {
                try
                {
                    base.Deserialize(autoCompression, data.DecryptWithString(Password), progressCallback);
                }
                catch
                {
                    throw new InvalidPasswordException();
                }
            }
            else
                base.Deserialize(autoCompression, data, progressCallback);
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

            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllBytes(path, Serialize(AutoCompression, (progression) => SaveProgress(progression)));
        }
        /// <summary>
        /// Save asynchronously the current storage to the specified folder path using the specified file name
        /// </summary>
        /// <param name="folderPath">Folder path where to save the file</param>
        /// <param name="fileName">Target file name</param>
        public void SaveAsync(string folderPath, string fileName)
        {
            DoJobAsync(() => Save(folderPath, fileName), Saved, (ex) => SaveFailed(ex));
        }
        /// <summary>
        /// Load the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            base.Clear();
            Deserialize(AutoCompression, File.ReadAllBytes(filePath), (progression) => LoadProgress(progression));
        }
        /// <summary>
        /// Load the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        public void Load(byte[] data)
        {
            Deserialize(AutoCompression, data, (progression) => LoadProgress(progression));
        }
        /// <summary>
        /// Load asynchronously the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        public void LoadAsync(string filePath)
        {
            DoJobAsync(() => Load(filePath), Loaded, (ex) => LoadFailed(ex));
        }
        /// <summary>
        /// Load asynchronously the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        public void LoadAsync(byte[] data)
        {
            DoJobAsync(() => Load(data), Loaded, (ex) => LoadFailed(ex));
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();

            Password = null;
            Saved = null;
            Loaded = null;
            SaveFailed = null;
            LoadFailed = null;
        }

        private void DoJobAsync(BasicAction asyncAction, BasicAction successCallback, BasicAction<Exception> failedCallback)
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
        }
    }
}
