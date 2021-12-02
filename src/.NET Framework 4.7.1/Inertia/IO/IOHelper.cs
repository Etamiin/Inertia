using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Inertia
{
    public static class IOHelper
    {
        public static string[] GetFilesFromDirectory(string path, bool includeSubFolders)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            path = path.ConventionFolderPath();

            var result = new List<string>();
            FindPaths(path);

            return result.ToArray();

            void FindPaths(string currentFolder)
            {
                var files = Directory.GetFiles(currentFolder);
                foreach (var file in files)
                {
                    result.Add(file);
                }

                if (includeSubFolders)
                {
                    var directories = Directory.GetDirectories(currentFolder);
                    foreach (var dir in directories)
                    {
                        FindPaths(dir);
                    }
                }
            }
        }
        public static void AppendAllBytes(string filePath, byte[] data)
        {
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
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
                {
                    sBuilder.Append(byteVal.ToString("x2"));
                }

                result = sBuilder.ToString();
            }

            return result;
        }        
        public static string GetSHA256(FileStream stream)
        {
            return GetSHA256(stream, ushort.MaxValue);
        }
        public static string GetSHA256(FileStream stream, int bufferLength)
        {
            using (stream)
            {
                var sha256 = new StringBuilder();
                long totalLength = 0;

                while (stream.Length > totalLength)
                {
                    var v = stream.Length - totalLength;
                    var length = v >= bufferLength ? bufferLength : v;

                    var data = new byte[length];
                    for (int x = 0; x < data.Length; x++)
                    {
                        data[x] = (byte)stream.ReadByte();
                    }

                    totalLength += length;
                    sha256.Append(GetSHA256(data));
                }

                return sha256.ToString();
            }
        }

        public static byte[] GzipCompress(ref byte[] data, out bool hasBetterSize)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new BufferedStream(new GZipStream(ms, CompressionMode.Compress)))
                {
                    gzip.Write(data, 0, data.Length);
                }

                var compressedData = ms.ToArray();

                hasBetterSize = compressedData.Length < data.Length;
                return compressedData;
            }
        }
        public static byte[] GzipDecompress(byte[] compressedData)
        {
            using (var cms = new MemoryStream(compressedData))
            {
                using (var ms = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(cms, CompressionMode.Decompress)))
                    {
                        gzs.CopyTo(ms);
                    }

                    return ms.ToArray();
                }
            }
        }

        public static byte[] EncryptWithString(byte[] data, string key)
        {
            var pdb = new PasswordDeriveBytes(key, Encoding.ASCII.GetBytes(key));
            var ms = new MemoryStream();

            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            aes.Dispose();
            pdb.Dispose();

            return ms.ToArray();
        }        
        public static byte[] DecryptWithString(byte[] encryptedData, string key)
        {
            var pdb = new PasswordDeriveBytes(key, Encoding.ASCII.GetBytes(key));
            var ms = new MemoryStream();

            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.Close();

            aes.Dispose();
            pdb.Dispose();

            return ms.ToArray();
        }
    }
}