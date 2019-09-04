using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class FileStorageBase : StorageBase
    {
        public event OnStorageUpdateStateChangeHandler OnExtract = (update) => { };
        public event OnStorageUpdateStateChangeHandler OnAddFiles = (update) => { };
        public event ActionHandler OnExtracted = () => { };
        public event ActionHandler OnFilesAdded = () => { };

        protected Dictionary<string, FileDataStorage> _files = new Dictionary<string, FileDataStorage>();
        protected string _password;

        public int Count
        {
            get
            {
                return _files.Count;
            }
        }

        internal FileStorageBase(string name = "") : base(name)
        {
        }

        public void SetPassword(string password)
        {
            this._password = IOHelper.GetSHA256(password);
        }
        public void AddFiles(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            IOHelper.GetFilesFromDirectory(directory, out string[] files);
            var update = new StorageUpdate(this, 0, files.Length);
            foreach (var file in files) {
                var storage = new FileDataStorage(file.Replace(directory, ""), File.ReadAllBytes(file));
                update.AddOne(storage.Path);
                this._files.Add(storage.Path, storage);
                OnAddFiles(update);
            }

            OnFilesAdded();
        }
        public void AddFilesAsync(string directory)
        {
            RuntimeModule.ExecuteAsync(() => AddFiles(directory));
        }

        protected abstract byte[] Serialize();
        protected abstract Dictionary<string, FileDataStorage> Deserialize(byte[] data, string password = "");

        public override byte[] Save() => Serialize();
        public override bool Save(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                File.WriteAllBytes(Path.Combine(directoryPath, Name), Serialize());
                OnSaved();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return false;
        }
        public override void Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Logger.Error("Invalid path");
                return;
            }

            var info = new FileInfo(path);
            if (info.Extension != FixedExtension)
                path += FixedExtension;

            if (!File.Exists(path))
            {
                Logger.Error($"Loading fileStorage error => file don't exist at path[{ path }]");
                return;
            }

            Name = info.Name;
            Load(File.ReadAllBytes(path));
        }
        public override void Load(byte[] data)
        {
            try
            {
                this._files = Deserialize(data);
                OnLoaded();
                return;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Extract(string directory)
        {
            RuntimeModule.ExecuteAsync(() => {
                var update = new StorageUpdate(this, 0, _files.Count);
                lock (_files)
                {
                    foreach (var file in _files)
                    {
                        file.Value.Extract(directory);
                        update.AddOne(file.Key);

                        OnExtract(update);
                    }
                }

                OnExtracted();
            });
        }
    }
}
