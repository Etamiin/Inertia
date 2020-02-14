using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public abstract class InertiaStorage : IDisposable
    {
        #region Public variables

        public abstract int Count { get; }
        public string Password { set; internal get; }
        public string Name { get; set; }
        public bool IsRunningAsyncTasks
        {
            get
            {
                var taskRunning = false;

                lock (TaskStatus)
                {
                    taskRunning = TaskStatus.Contains(false);
                    if (!taskRunning)
                        TaskStatus.Clear();
                }

                return taskRunning;
            }
        }

        #endregion

        #region Private variables

        private protected bool TaskLocked { get; private set; }
        private readonly List<bool> TaskStatus;

        #endregion

        #region Constructors

        public InertiaStorage()
        {
            TaskStatus = new List<bool>();
        }

        #endregion

        public void Save(string directoryPath, SaveMethod method)
        {
            PrivateSave(directoryPath, method, false);
        }
        public void SaveAsync(string directoryPath, SaveMethod method)
        {
            PrivateSave(directoryPath, method, true);
        }

        public void Load(string path)
        {
            LoadFromFile(path, false);
        }
        public void Load(byte[] data)
        {
            LoadFromBytes(data, false);
        }
        public void LoadAsync(string path)
        {
            LoadFromFile(path, true);
        }
        public void LoadAsync(byte[] data)
        {
            LoadFromBytes(data, true);
        }

        public byte[] ToBytes()
        {
            return GetFinalSerialization(Serialize(false));
        }
        public void ToBytesAsync(InertiaAction<byte[]> callback)
        {
            Task.Factory.StartNew(() => {
                var data = GetFinalSerialization(Serialize(true));
                callback(data);
            });
        }

        private void PrivateSave(string dirPath, SaveMethod method, bool async)
        {
            void SaveAll()
            {
                try
                {
                    var path = Path.Combine(dirPath, GetFinalName(dirPath, method));

                    File.WriteAllBytes(path, GetFinalSerialization(Serialize(async)));

                    if (async)
                        OnSaveCompleted();
                }
                catch (Exception e)
                {
                    OnSaveFailed(e);
                }
            }

            if (async)
                CreateTask(SaveAll, true);
            else
                SaveAll();
        }

        private string GetFinalName(string directoryPath, SaveMethod method)
        {
            if (method == SaveMethod.Overwrite)
            {
                var basePath = Path.Combine(directoryPath, Name);
                if (File.Exists(basePath))
                    File.Delete(basePath);

                return Name;
            }

            var _files = Directory.GetFiles(directoryPath);
            var _id = -1;

            foreach (var file in _files)
            {
                var _fileInfo = new FileInfo(file);
                var _tempName = _fileInfo.Name.Replace(Name, "");

                if (string.IsNullOrEmpty(_tempName))
                    _id = 0;

                var parsable = int.TryParse(_tempName, out int _tempId);

                if (parsable && _id <= _tempId)
                    _id = _tempId + 1;
            }

            var _name = Name;
            if (_id >= 0)
                _name += _id;

            return _name;
        }
        private void LoadFromFile(string path, bool async)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            var info = new FileInfo(path);
            Name = info.Name;

            LoadFromBytes(File.ReadAllBytes(path), async);
        }
        private void LoadFromBytes(byte[] data, bool async)
        {
            void LoadAll()
            {
                var deserialized = FirstDeserialization(data);
                if (deserialized == null)
                    return;

                Deserialize(deserialized, async);
                if (async)
                    OnLoadCompleted();
            }

            if (async)
                CreateTask(LoadAll, true);
            else
                LoadAll();
        }

        private byte[] GetFinalSerialization(byte[] serializedData)
        {
            var final = new InertiaWriter()
                .SetBool(!string.IsNullOrEmpty(Password));

            if (!string.IsNullOrEmpty(Password))
                final.SetBytes(InertiaIO.EncryptWithString(serializedData, Password));
            else
                final.SetBytes(serializedData);

            return final.ExportAndDispose();
        }
        private byte[] FirstDeserialization(byte[] data)
        {
            var first = new InertiaReader(data);
            byte[] result;

            if (first.GetBool())
            {
                try
                {
                    result = InertiaIO.DecryptWithString(first.GetBytes(), Password);
                }
                catch
                {
                    result = null;
                    OnLoadFailed(new InvalidPasswordException());
                }
            }
            else
                result = first.GetBytes();

            first.Dispose();
            return result;
        }

        private protected void WaitTasks(bool forceLock = false)
        {
            var count = 0;
            lock (TaskStatus)
                count = TaskStatus.Count;

            while (IsRunningAsyncTasks || TaskLocked)
                System.Threading.Thread.Sleep(250 + (count * 20));

            TaskLocked = forceLock;
        }
        private protected void CreateTask(InertiaAction action, bool locked)
        {
            void TaskAction()
            {
                var taskIndex = StartTask();
                action();
                EndTask(taskIndex);
            }

            if (!locked) {
                Task.Factory.StartNew(() => TaskAction());
            }
            else {
                Task.Factory.StartNew(() => {
                    WaitTasks(true);
                    TaskAction();
                    TaskLocked = false;
                });
            }
        }

        private int StartTask()
        {
            var index = 0;
            lock (TaskStatus)
            {
                TaskStatus.Add(false);
                index = TaskStatus.Count - 1;
            }

            return index;
        }
        private void EndTask(int index)
        {
            lock (TaskStatus)
            {
                TaskStatus[index] = true;
            }
        }

        internal abstract byte[] Serialize(bool async);
        internal abstract void Deserialize(byte[] data, bool async);
        internal abstract void OnDispose();

        internal abstract void OnSaveCompleted();
        internal abstract void OnLoadCompleted();
        internal abstract void OnSaveFailed(Exception e);
        internal abstract void OnLoadFailed(Exception e);

        public DataStorage<T> ToDataStorage<T>()
        {
            return (DataStorage<T>)this;
        }
        public FileStorage ToFileStorage()
        {
            return (FileStorage)this;
        }
        
        public void Dispose()
        {
            OnDispose();
            Name = null;
            Password = null;
        }
    }
}
