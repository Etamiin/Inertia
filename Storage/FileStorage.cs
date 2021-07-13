using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Inertia
{
    //TODO: V2
    [Obsolete]
    public class FileStorage : ISerializableObject
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Compress { get; set; } = true;

        private BasicReader _reader;
        private Dictionary<string, InFileStorageMark> _marks;
        private List<FileStoragePack> _packs;

        public FileStorage(string name)
        {
            Name = name;
            _marks = new Dictionary<string, InFileStorageMark>();
            _packs = new List<FileStoragePack>();
        }
        public FileStorage(string name, byte[] data, string password = "", bool compress = true) : this(name)
        {
            Password = password;
            Compress = compress;

            if (!string.IsNullOrEmpty(password))
                data = data.DecryptWithString(password);
            if (Compress)
                data = data.GzipDecompress();

            using (var reader = new BasicReader(data))
                Deserialize(reader);
        }

        public FileStorage TryRemove(string fullName)
        {
            _marks.Remove(fullName);
            return this;
        }
        public InFileStorageElement Find(string fullName)
        {
            _marks.TryGetValue(fullName, out InFileStorageMark mark);
            if (mark != null)
                return mark.Retrieve(_reader);

            return null;
        }

        public void Extract(string targetDirectoryPath)
        {
            InFileStorageMark[] marks = null;
            lock (_marks)
                marks = _marks.Values.ToArray();

            foreach (var mark in marks)
            {
                var element = mark.Retrieve(_reader);
                element.ExtractTo(targetDirectoryPath);
            }
        }

        public void Pack(string directoryPath)
        {
            var files = IOHelper.GetFilesFromDirectory(directoryPath, true);
            _packs.Add(new FileStoragePack(directoryPath, files));
        }
        public void ProcessPacksAsync(BasicAction OnCompleted)
        {
            if (_packs.Count == 0)
                return;

            Task.Factory.StartNew(() => {
                FileStoragePack[] packs = null;
                lock (_packs)
                    packs = _packs.ToArray();

                using (var writer = new BasicWriter())
                {
                    for (var i = 0; i < _packs.Count; i++)
                        _packs[i].PackIn(writer);

                    Serialize(writer);
                    Deserialize(new BasicReader(writer.ToArray()));

                    OnCompleted();
                }
            });
        }

        public void Save(string directoryPath)
        {
            if (_packs.Count > 0)
                return;

            directoryPath = directoryPath.ConventionFolderPath();

            using (var writer = new BasicWriter())
            {
                Serialize(writer);

                var final = writer.ToArray();
                if (Compress)
                    final = final.GzipCompress(out _);
                if (!string.IsNullOrEmpty(Password))
                    final = final.EncryptWithString(Password);

                File.WriteAllBytes(Path.Combine(directoryPath, Name), final);
            }
        }

        public void Serialize(BasicWriter writer)
        {
            var marks = _marks.Values.ToArray();
            foreach (InFileStorageMark mark in marks)
            {
                var element = mark.Retrieve(_reader);
                writer
                    .SetString(element.Name)
                    .SetLong(element.Data.LongLength)
                    .SetBytesWithoutHeader(element.Data);
            }
        }
        public void Deserialize(BasicReader reader)
        {
            _marks.Clear();
            _packs.Clear();
            if (_reader != null)
                _reader.Dispose();

            _reader = reader;

            while (reader.UnreadedLength > 0)
            {
                var mark = new InFileStorageMark(reader.GetString(), reader.Position);

                if (!_marks.ContainsKey(mark.Name))
                    _marks.Add(mark.Name, mark);
            }
        }
    }
}
