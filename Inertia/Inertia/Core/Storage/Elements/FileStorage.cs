using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class FileStorage : FileStorageBase
    {
        public override string Extension => ".fs";

        public FileStorage(string name = "") : base(name)
        {
        }

        protected override Dictionary<string, FileDataStorage> Deserialize(byte[] data, string password = "")
        {
            var dict = new Dictionary<string, FileDataStorage>();
            var reader = new Reader(data);
            
            if (!reader.GetBool()) {
                if (password != reader.GetString()) {
                    OnLoadFailed();
                    return dict;
                }
            }

            var readerBase = new Reader(reader.GetBytes());
            var count = readerBase.GetInt();
            var update = new StorageUpdate(this, 0, count);

            for (var i = 0; i < count; i++)
            {
                var path = readerBase.GetString();
                byte[] content = null;
                if (readerBase.GetBool())
                    content = Compression.Decompress(readerBase.GetBytes());
                else
                    content = readerBase.GetBytes();

                var storage = new FileDataStorage(path, content);
                dict.Add(storage.Path, storage);

                update.AddOne(storage.Path);
                OnLoadProgress(update);
            }

            reader.Dispose();
            readerBase.Dispose();

            return dict;
        }

        protected override byte[] Serialize()
        {
            var writer = new Writer();
            var noPass = string.IsNullOrEmpty(_password);
            var update = new StorageUpdate(this, 0, _files.Count);

            writer.SetBool(noPass);
            if (!noPass)
                writer.SetString(_password);

            var writerBase = new Writer();
            writerBase.SetInt(_files.Count);
            foreach (var file in _files.Values)
            {
                var isCompresed = Compression.Compress(file.Data, out byte[] compresedData);

                writerBase
                    .SetString(file.Path)
                    .SetBool(isCompresed)
                    .SetBytes(compresedData);

                update.AddOne(file.Path);
                OnSaveProgress(update);
            }

            writer.SetBytes(writerBase.ExportAndDispose());
            return writer.ExportAndDispose();
        }
    }
}
