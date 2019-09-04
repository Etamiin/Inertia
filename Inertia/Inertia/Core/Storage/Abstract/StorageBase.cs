using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class StorageBase
    {
        public event ActionHandler Saved = () => { };
        public event ActionHandler Loaded = () => { };
        public event ActionHandler LoadFailed = () => { };
        public event OnStorageUpdateStateChangeHandler SaveProgression = (progression) => { };
        public event OnStorageUpdateStateChangeHandler LoadProgression = (progression) => { };

        public string FixedExtension
        {
            get
            {
                return !Extension.StartsWith(".") ? $".{ Extension }" : Extension;
            }
        }
        public abstract string Extension { get; }
        public string Name { get; set; }

        internal StorageBase(string name = "")
        {
            if (name == null)
                name = string.Empty;

            if (!name.EndsWith(FixedExtension))
                name += FixedExtension;

            Name = name;
        }

        protected void OnSaved()
        {
            Saved();
        }
        protected void OnLoaded()
        {
            Loaded();
        }
        protected void OnSaveProgress(StorageUpdate progression)
        {
            SaveProgression(progression);
        }
        protected void OnLoadProgress(StorageUpdate progression)
        {
            LoadProgression(progression);
        }
        protected void OnLoadFailed()
        {
            LoadFailed();
        }
        public void SaveAsync(string directoryPath)
        {
            RuntimeModule.ExecuteAsync(() => Save(directoryPath));
        }
        public void LoadAsync(string path)
        {
            RuntimeModule.ExecuteAsync(() => Load(path));
        }
        public void LoadAsync(byte[] data)
        {
            RuntimeModule.ExecuteAsync(() => Load(data));
        }

        public abstract bool Save(string directoryPath);
        public abstract byte[] Save();
        public abstract void Load(string path);
        public abstract void Load(byte[] data);
    }
}
