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
        public static string GetSHA256(this byte[] data)
        {
            return WithSHA256((sha256) => sha256.ComputeHash(data));
        }
        public static string GetSHA256(this FileStream stream)
        {
            return WithSHA256((sha256) => sha256.ComputeHash(stream));
        }

        public static byte[] Compress(this byte[] data, CompressionAlgorithm algorithm)
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
        public static byte[] Decompress(this byte[] compressedData, CompressionAlgorithm algorithm)
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

        public static byte[] AesEncrypt(this byte[] data, string key, byte[] salt)
        {
            return WithAesEncryption(data, key, salt, (aes) => aes.CreateEncryptor());
        }
        public static byte[] AesEncrypt(this string value, string key, byte[] salt)
        {
            return AesEncrypt(value, key, Encoding.UTF8, salt);
        }
        public static byte[] AesEncrypt(this string value, string key, Encoding encoding, byte[] salt)
        {
            return AesEncrypt(encoding.GetBytes(value), key, salt);
        }
        public static byte[] AesDecrypt(this byte[] encryptedData, string key, byte[] salt)
        {
            return WithAesEncryption(encryptedData, key, salt, (aes) => aes.CreateDecryptor());
        }

        public static bool GetBit(this byte value, int index)
        {
            return GetBit(value, index, EndiannessType.LittleEndian);
        }
        public static bool GetBit(this byte value, int index, EndiannessType endianness)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            return (value & (1 << index)) != 0;
        }
        public static byte SetBit(this byte value, int index, bool state)
        {
            return SetBit(value, index, state, EndiannessType.LittleEndian);
        }
        public static byte SetBit(this byte value, int index, bool state, EndiannessType endianness)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            if (state)
            {
                value = (byte)(value | (1 << index));
            }
            else
            {
                value = (byte)(value & ~(1 << index));
            }

            return value;
        }
        public static void SetBitByRef(this ref byte value, int index, bool state)
        {
            value = SetBit(value, index, state, EndiannessType.LittleEndian);
        }
        public static void SetBitByRef(this ref byte value, int index, bool state, EndiannessType endianness)
        {
            value = SetBit(value, index, state, endianness);
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
        private static byte[] WithAesEncryption(byte[] data, string key, byte[] salt, Func<Aes, ICryptoTransform> createCryptoTransform)
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

                return ms.ToArray();
            }
        }
        private static byte[] GzipCompress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }
        private static byte[] GzipDecompress(byte[] compressedData)
        {
            using (var ms = new MemoryStream())
            using (var gzms = new MemoryStream(compressedData))
            {
                using (var gzs = new GZipStream(gzms, CompressionMode.Decompress))
                {
                    gzs.CopyTo(ms);
                }

                return ms.ToArray();
            }
        }
        private static byte[] DeflateCompress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var deflate = new DeflateStream(ms, CompressionMode.Compress))
                {
                    deflate.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }
        private static byte[] DeflateDecompress(byte[] compressedData)
        {
            using (var ms = new MemoryStream())
            using (var dsms = new MemoryStream(compressedData))
            {
                using (var deflate = new DeflateStream(dsms, CompressionMode.Decompress))
                {
                    deflate.CopyTo(ms);
                }

                return ms.ToArray();
            }
        }
    }
}
