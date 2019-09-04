using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class IOHelper
    {
        public static string ExecutableFolderPath => Environment.CurrentDirectory;

        private static Random ioRand = new Random();

        public static void GetFilesFromDirectory(string directoryPath, out string[] output)
        {
            var result = new List<string>();
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
                result.Add(file);

            var directories = Directory.GetDirectories(directoryPath);
            foreach (var dir in directories)
            {
                GetFilesFromDirectory(dir, out string[] dirOut);
                foreach (var file in dirOut)
                    result.Add(file);
            }

            output = result.ToArray();
        }
        public static void OpenFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                Process.Start("explorer.exe", directoryPath);
        }
        public static void AppendAllBytes(string path, byte[] data)
        {
            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public static void Shuffle<T>(this IList<T> collection)
        {
            var idStart = 0;
            var idRand = 0;
            T valueSave;

            while (idStart < collection.Count - 1)
            {
                idRand = ioRand.Next(idStart, collection.Count);
                valueSave = collection[idStart];
                collection[idStart++] = collection[idRand];
                collection[idRand] = valueSave;
            }
        }

        public static string GetSHA256(byte[] data)
        {
            string result = string.Empty;
            using (var sha256 = SHA256.Create())
            {
                var sha256Bytes = sha256.ComputeHash(data);
                var sBuilder = new StringBuilder();
                foreach (var byteVal in sha256Bytes)
                    sBuilder.Append(byteVal.ToString("x2"));
                result = sBuilder.ToString();
            }

            return result;
        }
        public static string GetSHA256(FileStream stream)
        {
            using (stream)
            {
                var lengthCount = 81920;
                var bigDatas = string.Empty;
                long count = 0;

                while (stream.Length > count)
                {
                    var v = stream.Length - count;
                    var length = v >= lengthCount ? lengthCount : v;

                    var data = new byte[length];
                    for (int x = 0; x < data.Length; x++)
                        data[x] = (byte)stream.ReadByte();

                    count += length;
                    bigDatas += GetSHA256(data);
                }

                return bigDatas;
            }
        }
        public static string GetSHA256(this string text)
        {
            var data = Encoding.ASCII.GetBytes(text);
            return GetSHA256(data);
        }
        public static string GetSHA256(this string text, Encoding encoding)
        {
            var data = encoding.GetBytes(text);
            return GetSHA256(data);
        }
    }
}
