using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    public sealed class DataReaderParameters
    {
        public Encoding Encoding { get; set; }
        public string? EncryptionKey { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }

        public DataReaderParameters()
        {
            Encoding = Encoding.UTF8;
            CompressionAlgorithm = CompressionAlgorithm.None;
        }
    }
}