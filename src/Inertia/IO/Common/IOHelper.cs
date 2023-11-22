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

        public static string GetSHA256(this byte[] data)
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
        public static async Task<string> GetSHA256Async(this byte[] data)
        {
            return await Task.Run(() => GetSHA256(data)).ConfigureAwait(false);
        }
        public static string GetSHA256(this FileStream stream)
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
        public static async Task<string> GetSHA256Async(this FileStream stream)
        {
            return await Task.Run(() => GetSHA256(stream)).ConfigureAwait(false);
        }

        public static BinaryTransformationResult GzipCompress(this byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(data, 0, data.Length);
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
        public static BinaryTransformationResult GzipDecompress(this byte[] compressedData)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var gzms = new MemoryStream(compressedData))
                    using (var gzs = new GZipStream(gzms, CompressionMode.Decompress))
                    {
                        gzs.CopyTo(ms);
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
        public static BinaryTransformationResult DeflateCompress(this byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(ms, CompressionMode.Compress))
                    {
                        deflate.Write(data, 0, data.Length);
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
        public static BinaryTransformationResult DeflateDecompress(this byte[] compressedData)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var dsms = new MemoryStream(compressedData))
                    using (var deflate = new DeflateStream(dsms, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(ms);
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }

        public static BinaryTransformationResult AesEncrypt(this byte[] data, string key)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new PasswordDeriveBytes(key, Encoding.UTF8.GetBytes(key)))
                    {
                        using (var aes = Aes.Create())
                        {
                            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                                cs.FlushFinalBlock();
                            }
                        }
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
        public static BinaryTransformationResult TryAesDecrypt(this byte[] encryptedData, string key)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new PasswordDeriveBytes(key, Encoding.UTF8.GetBytes(key)))
                    {
                        using (var aes = Aes.Create())
                        {
                            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedData);
                                cs.FlushFinalBlock();
                            }
                        }
                    }

                    return new BinaryTransformationResult(true, ms.ToArray(), null);
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
    }
}