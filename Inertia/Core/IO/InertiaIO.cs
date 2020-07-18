using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Inertia.Internal;

namespace Inertia
{
    /// <summary>
    /// Class containing methods for managing and transforming data
    /// </summary>
    public static class InertiaIO
    {
        #region Private variables

        internal readonly static Random m_rand = new Random();

        #endregion

        /// <summary>
        /// Returns all file paths of the files contained in the specified folder
        /// </summary>
        /// <param name="path">The target folder path</param>
        /// <param name="inheritance">True if sub folders has to be included</param>
        /// <returns>String array representing all paths</returns>
        public static string[] GetFilesPathFromDirectory(string path, bool inheritance)
        {
            path = path.VerifyPathForFolder();
            
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
        /// <summary>
        /// Open the target folder path in the file explorer
        /// </summary>
        /// <param name="path">Target folder to open</param>
        public static void OpenInExplorer(string path)
        {
            path = path.VerifyPathForFolder();

            if (Directory.Exists(path))
                Process.Start("explorer.exe", path);
        }
        /// <summary>
        /// Add the specified byte array to the end of the target file path
        /// </summary>
        /// <param name="path">Path of the target file</param>
        /// <param name="data">The byte array to add</param>
        public static void AppendAllBytes(string path, byte[] data)
        {
            using (var stream = new FileStream(path, FileMode.Append))
                stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Get the SHA256 key representation of the specified string data
        /// </summary>
        /// <param name="content">Target string data</param>
        /// <returns>The SHA256 representation</returns>
        public static string GetSHA256(string content)
        {
            return GetSHA256(Encoding.UTF8.GetBytes(content));
        }
        /// <summary>
        /// Get the SHA256 key representation of the specified byte array data
        /// </summary>
        /// <param name="data">Target byte array data</param>
        /// <returns>The SHA256 representation</returns>
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
        /// <summary>
        /// Get the SHA256 key representation of the specified <see cref="FileStream"/>
        /// </summary>
        /// <param name="stream">Target <see cref="FileStream"/></param>
        /// <returns>The SHA256 representation</returns>
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

        /// <summary>
        /// Compress the specified byte array and return the compressed one
        /// </summary>
        /// <param name="data">Target byte array to compress</param>
        /// <param name="compressed">Return true if the returned data is lower in length than the non-compressed data</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] Compress(byte[] data, out bool compressed)
        {
            using (var cms = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(cms, CompressionMode.Compress)))
                    gzs.Write(data, 0, data.Length);

                var compressedData = cms.ToArray();

                compressed = compressedData.Length < data.Length;
                return compressedData;
            }
        }
        /// <summary>
        /// Decompress the specified byte array and return the decompressed one
        /// </summary>
        /// <param name="compressedData">Target byte array to decompress</param>
        /// <returns>Decompressed byte array</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            using (var cms = new MemoryStream(compressedData))
            {
                using (var dms = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(cms, CompressionMode.Decompress)))
                        gzs.CopyTo(dms);

                    return dms.ToArray();
                }
            }
        }

        /// <summary>
        /// Encrypt the target byte array with the specified string key
        /// </summary>
        /// <param name="data">Target byte array to encrypt</param>
        /// <param name="key">Target string key for encryption</param>
        /// <returns>Encrypted byte array</returns>
        public static byte[] EncryptWithString(byte[] data, string key)
        {
            var pdb =new PasswordDeriveBytes(key, Encoding.ASCII.GetBytes(key));
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
        /// <summary>
        /// Encrypt the target byte array with the specified string key
        /// </summary>
        /// <param name="encryptedData">Target byte array to encrypt</param>
        /// <param name="key">Target string key for encryption</param>
        /// <returns>Decrypted byte array</returns>
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
