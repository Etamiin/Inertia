using System.IO;

namespace Inertia
{
    public class InFileStorageElement
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }

        public InFileStorageElement(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public void ExtractTo(string targetDirectoryPath)
        {
            targetDirectoryPath = targetDirectoryPath.ConventionFolderPath();

            var path = Path.Combine(targetDirectoryPath, Name);
            var fInfo = new FileInfo(path);

            if (!fInfo.Directory.Exists)
                fInfo.Directory.Create();

            File.WriteAllBytes(path, Data);
        }
    }
}
