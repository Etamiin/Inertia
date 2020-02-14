using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia
{
    public static class InertiaIO
    {
        #region Private variables

        private readonly static Random ioRand = new Random();

        #endregion
        
        public static string[] GetFilesPathFromDirectory(string path, bool inheritance)
        {
            StringConventionNormalizer.NormalizeFolderPath(ref path);

            if (!Directory.Exists(path))
                return new string[] { };

            var result = new List<string>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
                result.Add(file);

            if (inheritance)
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    var dirOut = GetFilesPathFromDirectory(dir, inheritance);
                    foreach (var file in dirOut)
                        result.Add(file);
                }
            }

            return result.ToArray();
        }
        public static void OpenInExplorer(string path)
        {
            StringConventionNormalizer.NormalizeFolderPath(ref path);

            if (Directory.Exists(path))
                Process.Start("explorer.exe", path);
        }
        public static void AppendAllBytes(string path, byte[] data)
        {
            using (var stream = new FileStream(path, FileMode.Append))
                stream.Write(data, 0, data.Length);
        }

        public static void Shuffle<T>(this IList<T> collection)
        {
            var idStart = 0;
            T valueSave;

            while (idStart < collection.Count - 1)
            {
                int idRand = ioRand.Next(idStart, collection.Count);
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
            var data = InertiaConfiguration.BaseEncodage.GetBytes(text);
            return GetSHA256(data);
        }
        public static string GetSHA256(this string text, Encoding encoding)
        {
            var data = encoding.GetBytes(text);
            return GetSHA256(data);
        }

        public static bool Compress(byte[] data, out byte[] buffer)
        {
            buffer = AcedDeflator.Instance.Compress(data, 0, data.Length, AcedCompressionLevel.Fast, 0, 0);
            AcedDeflator.Instance.Dispose();

            var isBetter = buffer.Length < data.Length;
            if (!isBetter)
                buffer = data;

            return isBetter;
        }
        public static byte[] Decompress(byte[] compressedData)
        {
            var result = AcedInflator.Instance.Decompress(compressedData, 0, 0, 0);
            AcedInflator.Instance.Dispose();

            return result;
        }
    
        public static byte[] EncryptWithString(byte[] data, string value)
        {
            var pdb =new PasswordDeriveBytes(value, Encoding.ASCII.GetBytes(value));
            var ms = new MemoryStream();

            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            var  cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            aes.Dispose();
            pdb.Dispose();

            return ms.ToArray();
        }
        public static byte[] DecryptWithString(byte[] cryptedData, string value)
        {
            var pdb = new PasswordDeriveBytes(value, Encoding.ASCII.GetBytes(value));
            var ms = new MemoryStream();

            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cryptedData, 0, cryptedData.Length);
            cs.Close();

            aes.Dispose();
            pdb.Dispose();

            return ms.ToArray();
        }
    }
}
