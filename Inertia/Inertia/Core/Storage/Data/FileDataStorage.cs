using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Inertia
{
    public class FileDataStorage : IDisposable
    {
        public string Path { get; private set; }
        public byte[] Data { get; private set; }

        internal FileDataStorage(string path, byte[] data)
        {
            Path = path;
            Data = data;
        }

        public void Extract(string directory)
        {
            var path = directory + Path;
            var dir = new FileInfo(path).Directory;

            if (!dir.Exists)
            {
                try
                {
                    dir.Create();
                }
                catch (Exception e)
                {
                    Logger.Error("Extraction on directory [" + dir.FullName + "] impossible, can't create specified directory >> " + e.ToString());
                    return;
                }
            }

            File.WriteAllBytes(path, Data);
        }

        public void Dispose()
        {
            Path = null;
            Data = null;
        }
    }
}
