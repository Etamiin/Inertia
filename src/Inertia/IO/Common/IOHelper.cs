using Inertia.IO;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public static async Task<string> GetSHA256Async(byte[] data)
        {
            return await Task.Run(() => GetSHA256(data)).ConfigureAwait(false);
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
        public static async Task<string> GetSHA256Async(FileStream stream)
        {
            return await Task.Run(() => GetSHA256(stream)).ConfigureAwait(false);
        }

        public static ZipCompressionResult GzipCompress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new BufferedStream(new GZipStream(ms, CompressionMode.Compress)))
                {
                    gzip.Write(data, 0, data.Length);
                }

                var compressedData = ms.ToArray();

                return new ZipCompressionResult(compressedData, compressedData.Length < data.Length);
            }
        }
        public static async Task<ZipCompressionResult> GzipCompressAsync(byte[] data)
        {
            return await Task.Run(() => GzipCompress(data)).ConfigureAwait(false);
        }
        public static ZipCompressionResult GzipDecompress(byte[] compressedData)
        {
            using (var cms = new MemoryStream(compressedData))
            {
                using (var ms = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(cms, CompressionMode.Decompress)))
                    {
                        gzs.CopyTo(ms);
                    }

                    return new ZipCompressionResult(ms.ToArray(), null);
                }
            }
        }
        public static async Task<ZipCompressionResult> GzipDecompressAsync(byte[] compressedData)
        {
            return await Task.Run(() => GzipDecompress(compressedData)).ConfigureAwait(false);
        }

        public static AesEncryptionResult AesEncrypt(byte[] data, string key)
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

                            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                                cs.Close();
                            }

                            return new AesEncryptionResult(true, ms.ToArray(), null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new AesEncryptionResult(false, null, ex);
            }
        }
        public static async Task<AesEncryptionResult> AesEncryptAsync(byte[] data, string key)
        {
            return await Task.Run(() => AesEncrypt(data, key)).ConfigureAwait(false);
        }
        public static AesEncryptionResult TryAesDecrypt(byte[] encryptedData, string key)
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

                    return new AesEncryptionResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new AesEncryptionResult(false, null, ex);
            }
        }
        public static async Task<AesEncryptionResult> TryAesDecryptAsync(byte[] encryptedData, string key)
        {
            return await Task.Run(() => TryAesDecrypt(encryptedData, key)).ConfigureAwait(false);
        }
    }
}