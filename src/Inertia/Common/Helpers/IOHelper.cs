using System.IO;

namespace Inertia
{
    public static class IOHelper
    {
        public static void AppendAllBytes(string filePath, byte[] data)
        {
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
            }
        }
    }
}