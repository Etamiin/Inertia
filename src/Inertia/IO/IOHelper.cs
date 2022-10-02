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
        public static void AppendAllBytes(string filePath, byte[] data)
        {
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public static string GetSHA256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                var sha256Bytes = sha256.ComputeHash(data);
                var sBuilder = new StringBuilder();
                foreach (var byteVal in sha256Bytes)
                {
                    sBuilder.Append(byteVal.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
        public static string GetSHA256(FileStream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                var sha256Bytes = sha256.ComputeHash(stream);
                var sBuilder = new StringBuilder();
                foreach (var byteVal in sha256Bytes)
                {
                    sBuilder.Append(byteVal.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static byte[] GzipCompress(byte[] data, out bool hasBetterSize)
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
            using (var ms = new MemoryStream())
            {
                using (var pdb = new PasswordDeriveBytes(key, Encoding.ASCII.GetBytes(key)))
                {
                    using (var aes = new AesManaged())
                    {
                        aes.Key = pdb.GetBytes(aes.KeySize / 8);
                        aes.IV = pdb.GetBytes(aes.BlockSize / 8);

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.Close();
                        }

                        return ms.ToArray();
                    }
                }
            }
        }        
        public static bool TryDecryptWithString(byte[] encryptedData, string key, out byte[] decryptedData)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new PasswordDeriveBytes(key, Encoding.ASCII.GetBytes(key)))
                    {
                        using (var aes = new AesManaged())
                        {
                            aes.Key = pdb.GetBytes(aes.KeySize / 8);
                            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

                            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedData, 0, encryptedData.Length);
                                cs.Close();
                            }
                        }
                    }

                    decryptedData = ms.ToArray();
                    return true;
                }
            }
            catch
            {
                decryptedData = null;
                return false;
            }
        }
    }
}