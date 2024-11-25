using Inertia.IO;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System
{
    public static class BinaryManipulationExtensions
    {
        private static byte[] _defaultSalt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65 };

        public static string GetSHA256(this byte[] data)
        {
            return TransformWithSHA256((sha256) => sha256.ComputeHash(data));
        }
        public static string GetSHA256(this FileStream stream)
        {
            return TransformWithSHA256((sha256) => sha256.ComputeHash(stream));
        }

        public static BinaryTransformationResult Compress(this byte[] data, CompressionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case CompressionAlgorithm.GZip:
                    return GzipCompress(data);
                case CompressionAlgorithm.Deflate:
                    return DeflateCompress(data);
                default:
                    throw new InvalidEnumArgumentException(nameof(algorithm), (int)algorithm, algorithm.GetType());
            }
        }
        public static BinaryTransformationResult Decompress(this byte[] compressedData, CompressionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case CompressionAlgorithm.GZip:
                    return GzipDecompress(compressedData);
                case CompressionAlgorithm.Deflate:
                    return DeflateDecompress(compressedData);
                default:
                    throw new InvalidEnumArgumentException(nameof(algorithm), (int)algorithm, algorithm.GetType());
            }
        }

        public static BinaryTransformationResult AesEncrypt(this byte[] data, string key)
        {
            return AesEncrypt(data, key, _defaultSalt);
        }
        public static BinaryTransformationResult AesEncrypt(this byte[] data, string key, byte[] salt)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new PasswordDeriveBytes(key, salt))
                    {
                        using (var aes = Aes.Create())
                        {
                            aes.Key = pdb.GetBytes(aes.KeySize / 8);
                            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

                            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                            }

                            return new BinaryTransformationResult(true, ms.ToArray(), null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new BinaryTransformationResult(false, null, ex);
            }
        }
        public static BinaryTransformationResult AesEncrypt(this string value, string key)
        {
            return AesEncrypt(value, key, Encoding.UTF8, _defaultSalt);
        }
        public static BinaryTransformationResult AesEncrypt(this string value, string key, byte[] salt)
        {
            return AesEncrypt(value, key, Encoding.UTF8, salt);
        }
        public static BinaryTransformationResult AesEncrypt(this string value, string key, Encoding encoding)
        {
            return AesEncrypt(value, key, encoding, _defaultSalt);
        }
        public static BinaryTransformationResult AesEncrypt(this string value, string key, Encoding encoding, byte[] salt)
        {
            return AesEncrypt(encoding.GetBytes(value), key, salt);
        }
        public static BinaryTransformationResult AesDecrypt(this byte[] encryptedData, string key)
        {
            return AesDecrypt(encryptedData, key, _defaultSalt);
        }
        public static BinaryTransformationResult AesDecrypt(this byte[] encryptedData, string key, byte[] salt)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new PasswordDeriveBytes(key, salt))
                    {
                        using (var aes = Aes.Create())
                        {
                            aes.Key = pdb.GetBytes(aes.KeySize / 8);
                            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

                            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedData, 0, encryptedData.Length);
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

        private static string TransformWithSHA256(Func<SHA256, byte[]> hashCompute)
        {
            using (var sha256 = SHA256.Create())
            {
                var sha256Bytes = hashCompute(sha256);
                var sBuilder = new StringBuilder();
                foreach (var byteVal in sha256Bytes)
                {
                    sBuilder.Append(byteVal.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
        private static BinaryTransformationResult GzipCompress(byte[] data)
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
        private static BinaryTransformationResult GzipDecompress(byte[] compressedData)
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
        private static BinaryTransformationResult DeflateCompress(byte[] data)
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
        private static BinaryTransformationResult DeflateDecompress(byte[] compressedData)
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
    }
}
