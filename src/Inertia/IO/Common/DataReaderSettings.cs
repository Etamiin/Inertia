using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    public sealed class DataReaderSettings
    {
        public Encoding Encoding { get; set; }
        public string? EncryptionKey { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }

        public DataReaderSettings()
        {
            Encoding = Encoding.UTF8;
            CompressionAlgorithm = CompressionAlgorithm.None;
        }
    }
}