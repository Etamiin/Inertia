using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Inertia;

namespace Inertia.Storage
{
    internal class FileStoragePack
    {
        internal readonly string BaseDirectory;
        internal readonly string[] Pack;

        internal FileStoragePack(string baseDirectory, string[] pack)
        {
            BaseDirectory = baseDirectory.ConventionFolderPath();
            Pack = pack;
        }

        internal void PackIn(BasicWriter writer)
        {
            for (var i = 0; i < Pack.Length; i++)
            {
                var file = Pack[i];

                if (!File.Exists(file))
                    continue;

                var completeName = file.Replace(BaseDirectory, string.Empty);
                var data = File.ReadAllBytes(file);
                writer
                    .SetString(completeName)
                    .SetLong(data.LongLength)
                    .SetBytesWithoutHeader(data);
            }
        }
    }
}
