using System;

namespace Inertia.IO
{
    public class BinaryTransformationProcessor
    {
        public BinaryTransformationProcessor(string encryptionKey, byte[] salt) : this(encryptionKey, salt, CompressionAlgorithm.None)
        {
        }
        public BinaryTransformationProcessor(CompressionAlgorithm compressionAlgorithm) : this(null, null, compressionAlgorithm)
        {
        }
        public BinaryTransformationProcessor(string encryptionKey, byte[] salt, CompressionAlgorithm compressionAlgorithm)
        {
            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {
                Check.ThrowsIfNull(salt, nameof(salt));

                EncryptionKey = encryptionKey;
                Salt = salt;
            }

            CompressionAlgorithm = compressionAlgorithm;
        }

        public string EncryptionKey { get; set; }
        public byte[] Salt { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }

        public byte[] Transform(byte[] data)
        {
            if (!string.IsNullOrWhiteSpace(EncryptionKey))
            {
                data = data.AesEncrypt(EncryptionKey, Salt);
            }

            if (CompressionAlgorithm != CompressionAlgorithm.None)
            {
                data = data.Compress(CompressionAlgorithm);
            }

            return data;
        }
        public byte[] Revert(byte[] data)
        {
            if (CompressionAlgorithm != CompressionAlgorithm.None)
            {
                data = data.Decompress(CompressionAlgorithm);
            }

            if (!string.IsNullOrWhiteSpace(EncryptionKey))
            {
                data = data.AesDecrypt(EncryptionKey, Salt);
            }

            return data;
        }
    }
}
