using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Storage
{
    public class FileStorage : InertiaStorage
    {
        #region Events

        public event FileStorageCompletedHandler Saved = (storage) => { };
        public event FileStorageCompletedHandler Loaded = (storage) => { };
        public event FileStorageProgressFailedHandler SaveFailed = (storage, exception) => { };
        public event FileStorageProgressFailedHandler LoadFailed = (storage, exception) => { };
        public event FileStorageProgressHandler SaveProgress = (storage, progression) => { };
        public event FileStorageProgressHandler LoadProgress = (storage, progression) => { };
        public event FileStorageAddFileCompletedHandler FileAdded = (file) => { };
        public event FileStorageAddFileFailedHandler AddFileFailed = (file, exception) => { };
        public event FileStorageAddDirectoryCompletedHandler DirectoryAdded = (storage, files) => { };
        public event FileStorageAddDirectoryProgressHandler AddDirectoryProgress = (storage, file, exception) => { };
        public event FileStorageCompletedHandler Extracted = (storage) => { };
        public event FileStorageExtractFailedHandler ExtractFailed = (storage, file, exception) => { };
        public event FileStorageExtractProgressHandler ExtractProgress = (storage, file, progression) => { };
        public event FileStorageExtractFileCompletedHandler FileExtracted = (storage, file) => { };

        #endregion

        #region Static methods

        public static FileStorage Create()
        {
            return new FileStorage("Unknown");
        }
        public static FileStorage Create(string name)
        {
            return new FileStorage(name);
        }

        #endregion

        #region Public variables

        public override int Count => Files.Count;

        #endregion

        #region Private variables

        private Dictionary<string, FileMemoryData> Files;

        #endregion

        #region Constructors

        internal FileStorage(string name)
        {
            Files = new Dictionary<string, FileMemoryData>();

            Name = name;
        }

        #endregion

        public FileMemoryData this[string key]
        {
            get
            {
                if (!Files.ContainsKey(key))
                    throw new KeyNotFoundException("Key [" + key + "] - not found");

                return Files[key];
            }
        }
        
        public string[] GetKeys()
        {
            return Files.Keys.ToArray();
        }

        public FileMemoryData AddFile(string path)
        {
            FileMemoryData file = null;
            try
            {
                file = new FileMemoryData(this, path);
                Files.Add(file.Name, file);
            }
            catch (Exception e)
            {
                AddFileFailed(file, e);
            }

            return file;
        }
        public FileMemoryData AddFile(string name, byte[] data)
        {
            FileMemoryData file = null;

            try
            {
                file = new FileMemoryData(this, name, data);
                Files.Add(file.Name, file);
            }
            catch (Exception e)
            {
                AddFileFailed(file, e);
            }

            return file;
        }
        public void AddFileAsync(string path)
        {
            CreateTask(() => {
                var file = AddFile(path);
                if (file != null)
                    FileAdded(file);
            }, false);
        }
        public FileMemoryData[] AddFiles(params string[] paths)
        {
            var files = new List<FileMemoryData>();
            for (var i = 0; i < paths.Length; i++)
            {
                var file = AddFile(paths[i]);
                if (file == null)
                    continue;

                files.Add(file);
            }

            return files.ToArray();
        }
        public void AddFilesAsync(InertiaAction<FileMemoryData[]> callback, params string[] paths)
        {
            CreateTask(() => {
                var files = new List<FileMemoryData>();
                for (var i = 0; i < paths.Length; i++)
                {
                    var file = AddFile(paths[i]);
                    if (file == null)
                        continue;

                    FileAdded(file);
                    files.Add(file);
                }

                callback(files.ToArray());
            }, false);
        }

        public FileMemoryData[] AddAllFilesFromDirectory(string directoryPath)
        {
            return AddFromDirectory(directoryPath, false);
        }
        public void AddAllFilesFromDirectoryAsync(string directoryPath)
        {
            CreateTask(() => {
                var files = AddFromDirectory(directoryPath, true);
                DirectoryAdded(this, files);
            }, false);
        }

        public void RemoveFile(string key)
        {
            if (Files.ContainsKey(key))
                Files.Remove(key);
        }

        public void ExtractTo(string directoryPath)
        {
            PrivateExtract(directoryPath, false);
        }
        public void ExtractToAsync(string directoryPath)
        {
            CreateTask(() => {
                PrivateExtract(directoryPath, true);
            }, false);
        }

        private FileMemoryData[] AddFromDirectory(string directoryPath, bool async)
        {
            if (directoryPath.Contains("/"))
                directoryPath = directoryPath.Replace("/", @"\");

            var files = InertiaIO.GetFilesPathFromDirectory(directoryPath, true);

            var progression = new StorageAsyncProgression(files.Length);
            var result = new List<FileMemoryData>();

            foreach (var path in files)
            {
                var file = new FileMemoryData(this, path, directoryPath);

                if (Files.ContainsKey(file.Name))
                {
                    AddFileFailed(file, new Exception("Key [" + file.Name + "] - already exit"));
                    continue;
                }

                Files.Add(file.Name, file);

                if (async)
                {
                    progression.Current++;

                    FileAdded(file);
                    AddDirectoryProgress(this, file, progression);
                }

                result.Add(file);
            }

            return result.ToArray();
        }
        private void PrivateExtract(string directoryPath, bool async)
        {
            void Extraction()
            {
                var extractionFailed = false;

                lock (Files)
                {
                    var progression = new StorageAsyncProgression(Count);

                    foreach (var file in Files)
                    {
                        extractionFailed = !file.Value.ExtractTo(directoryPath);

                        if (extractionFailed)
                            break;

                        if (async)
                        {
                            progression.Current++;
                            ExtractProgress(this, file.Value, progression);
                        }
                    }

                    if (async && !extractionFailed)
                        Extracted(this);
                }
            }

            if (async)
                CreateTask(Extraction, true);
            else
                Extraction();
        }

        internal override byte[] Serialize(bool async)
        {
            try
            {
                var keys = GetKeys();
                var progression = new StorageAsyncProgression(keys.Length);
                var writer = new InertiaWriter()
                    .SetInt(keys.Length);

                for (var i = 0; i < keys.Length; i++)
                {
                    var file = Files[keys[i]];

                    writer
                        .SetString(file.Name)
                        .SetByte((byte)file.StorageType);

                    switch (file.StorageType)
                    {
                        case FileMemoryStorageType.Internal:
                            writer.SetBool(file.IsDirectoryChild);
                            break;
                    }

                    var data = file.GetData();
                    var compressable = InertiaIO.Compress(data, out data);

                    writer
                        .SetBool(compressable)
                        .SetBytes(data);
                    
                    if (async)
                    {
                        progression.Current++;
                        SaveProgress(this, progression);
                    }
                }

                return writer.ExportAndDispose();
            }
            catch (Exception e)
            {
                SaveFailed(this, e);
            }

            return null;
        }
        internal override void Deserialize(byte[] data, bool async)
        {
            OnDispose();
            Files = new Dictionary<string, FileMemoryData>();

            try
            {
                var reader = new InertiaReader(data);
                var progression = new StorageAsyncProgression(reader.GetInt());

                for (var i = 0; i < progression.Total; i++)
                {
                    var key = reader.GetString();
                    var type = (FileMemoryStorageType)reader.GetByte();
                    FileMemoryData file = null;

                    switch (type)
                    {
                        case FileMemoryStorageType.Internal:
                            var isChild = reader.GetBool();
                            byte[] fileData = null;
                            if (reader.GetBool())
                                fileData = InertiaIO.Decompress(reader.GetBytes());
                            else
                                fileData = reader.GetBytes();

                            file = new FileMemoryData(this, key, fileData, isChild);
                            break;
                        case FileMemoryStorageType.Manual:
                            if (reader.GetBool())
                                fileData = InertiaIO.Decompress(reader.GetBytes());
                            else
                                fileData = reader.GetBytes();
                            file = new FileMemoryData(this, key, fileData);
                            break;
                    }

                    Files.Add(file.Name, file);

                    if (async)
                    {
                        progression.Current++;
                        LoadProgress(this, progression);
                    }
                }

                reader.Dispose();
            }
            catch (Exception e)
            {
                LoadFailed(this, e);
            }
        }

        internal override void OnDispose()
        {
            if (Files == null)
                return;

            foreach (var file in Files)
                file.Value.Dispose();
            Files.Clear();
            Files = null;
        }

        internal override void OnSaveCompleted()
        {
            Saved(this);
        }
        internal override void OnLoadCompleted()
        {
            Loaded(this);
        }
        internal override void OnSaveFailed(Exception e)
        {
            SaveFailed(this, e);
        }
        internal override void OnLoadFailed(Exception e)
        {
            LoadFailed(this, e);
        }
        internal void OnFileExtracted(FileMemoryData file)
        {
            FileExtracted(this, file);
        }
        internal void OnFileExtractedFailed(FileMemoryData file, Exception e)
        {
            ExtractFailed(this, file, e);
        }
    }
}
