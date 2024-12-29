using Inertia.IO;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Inertia;

namespace System
{
    public static class BinaryManipulationExtensions
    {
        private static byte[] _defaultSalt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65 };

        public static string GetSHA256(this byte[] data)
        {
            return WithSHA256((sha256) => sha256.ComputeHash(data));
        }
        public static string GetSHA256(this FileStream stream)
        {
            return WithSHA256((sha256) => sha256.ComputeHash(stream));
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
            return WithAesEncryption(data, key, salt, (aes) => aes.CreateEncryptor());
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
            return WithAesEncryption(encryptedData, key, salt, (aes) => aes.CreateDecryptor());
        }

        public static bool GetBit(this byte value, int index)
        {
            return GetBit(value, index, EndiannessType.Auto);
        }
        public static bool GetBit(this byte value, int index, EndiannessType endianness)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.Auto)
            {
                endianness = BitConverter.IsLittleEndian ? EndiannessType.LittleEndian : EndiannessType.BigEndian;
            }

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            return (value & (1 << index)) != 0;
        }
        public static byte SetBit(this byte value, int index, bool bit)
        {
            return SetBit(value, index, bit, EndiannessType.Auto);
        }
        public static byte SetBit(this byte value, int index, bool bit, EndiannessType endianness)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.Auto)
            {
                endianness = BitConverter.IsLittleEndian ? EndiannessType.LittleEndian : EndiannessType.BigEndian;
            }

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            if (bit)
            {
                value = (byte)(value | (1 << index));
            }
            else
            {
                value = (byte)(value & ~(1 << index));
            }

            return value;
        }
        public static void SetBitByRef(this ref byte value, int index, bool bit)
        {
            value = SetBit(value, index, bit, EndiannessType.Auto);
        }
        public static void SetBitByRef(this ref byte value, int index, bool bit, EndiannessType endianness)
        {
            value = SetBit(value, index, bit, endianness);
        }

        private static string WithSHA256(Func<SHA256, byte[]> hashCompute)
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
        private static BinaryTransformationResult WithAesEncryption(byte[] data, string key, byte[] salt, Func<Aes, ICryptoTransform> createCryptoTransform)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var pdb = new Rfc2898DeriveBytes(key, salt))
                    {
                        using (var aes = Aes.Create())
                        {
                            aes.Key = pdb.GetBytes(aes.KeySize / 8);
                            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

                            using (var cs = new CryptoStream(ms, createCryptoTransform(aes), CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                            }
                        }
                    }

                    return BinaryTransformationResult.Success(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BinaryTransformationResult.Failure(ex);
            }
        }
        private static BinaryTransformationResult GzipCompress(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream())
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);

                    return BinaryTransformationResult.Success(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BinaryTransformationResult.Failure(ex);
            }
        }
        private static BinaryTransformationResult GzipDecompress(byte[] compressedData)
        {
            try
            {
                using (var ms = new MemoryStream())
                using (var gzms = new MemoryStream(compressedData))
                using (var gzs = new GZipStream(gzms, CompressionMode.Decompress))
                {
                    gzs.CopyTo(ms);

                    return BinaryTransformationResult.Success(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BinaryTransformationResult.Failure(ex);
            }
        }
        private static BinaryTransformationResult DeflateCompress(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream())
                using (var deflate = new DeflateStream(ms, CompressionMode.Compress))
                {
                    deflate.Write(data, 0, data.Length);

                    return BinaryTransformationResult.Success(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BinaryTransformationResult.Failure(ex);
            }
        }
        private static BinaryTransformationResult DeflateDecompress(byte[] compressedData)
        {
            try
            {
                using (var ms = new MemoryStream())
                using (var dsms = new MemoryStream(compressedData))
                using (var deflate = new DeflateStream(dsms, CompressionMode.Decompress))
                {
                    deflate.CopyTo(ms);

                    return BinaryTransformationResult.Success(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BinaryTransformationResult.Failure(ex);
            }
        }
    }
}
