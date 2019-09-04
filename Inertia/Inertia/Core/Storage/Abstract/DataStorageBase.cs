using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class DataStorageBase<T> : StorageBase
    {
        protected MultiDictionary<T> _data = new MultiDictionary<T>();

        public int Count => _data.Count;

        internal DataStorageBase(string name = "") : base(name)
        {
        }

        public TDatas Get<TDatas>(T identifier)
        {
            return _data.Get<TDatas>(identifier);
        }
        public TDatas[] Get<TDatas>(params T[] identifiers)
        {
            var result = new TDatas[identifiers.Length];
            for (var i = 0; i < identifiers.Length; i++)
                result[i] = Get<TDatas>(identifiers[i]);
            return result;
        }
        public object Get(T identifier)
        {
            return _data.Get(identifier);
        }
        public object[] Get(params T[] identifiers)
        {
            var result = new object[identifiers.Length];
            for (var i = 0; i < identifiers.Length; i++)
                result[i] = Get(identifiers[i]);
            return result;
        }
        public bool Exist(T identifier)
        {
            return _data.Exist(identifier);
        }

        protected abstract byte[] Serialize();
        protected abstract MultiDictionary<T> Deserialize(byte[] data);

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
            if (string.IsNullOrEmpty(path)) {
                Logger.Error("Invalid path");
                return;
            }

            var info = new FileInfo(path);
            if (info.Extension != FixedExtension)
                path += FixedExtension;

            if (!File.Exists(path)) {
                Logger.Error($"Loading datasStorage error => file don't exist at path[{ path }]");
                return;
            }

            Name = info.Name;
            Load(File.ReadAllBytes(path));
        }
        public override void Load(byte[] data)
        {
            try
            {
                _data = Deserialize(data);
                OnLoaded();
                return;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public DataStorageBase<T> Add<TDatas>(T identifier, TDatas data)
        {
            _data.Add(identifier, data);
            return this;
        }
        public DataStorageBase<T> AddRange<TDatas>(Dictionary<T, TDatas> range)
        {
            foreach (var identifier in range.Keys)
                Add(identifier, range[identifier]);
            return this;
        }
        public DataStorageBase<T> Remove(T identifier)
        {
            _data.Remove(identifier);
            return this;
        }
        public DataStorageBase<T> RemoveRange(params T[] identifiers)
        {
            foreach (var identifier in identifiers)
                Remove(identifier);
            return this;
        }
        public DataStorageBase<T> Replace<TDatas>(T identifier, TDatas value)
        {
            _data.Replace(identifier, value);
            return this;
        }
        public DataStorageBase<T> Replace<TDatas>(Dictionary<T, TDatas> range)
        {
            foreach (var identifier in range.Keys)
                Replace(identifier, range[identifier]);
            return this;
        }
    }
}
