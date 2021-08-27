using System.IO;

namespace Inertia
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
