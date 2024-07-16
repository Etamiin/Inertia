using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    public class DataWriterParameters
    {
        public int Capacity { get; set; }
        public Encoding Encoding { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }
        public string? EncryptionKey { get; set; }

        public DataWriterParameters()
        {
            Capacity = 256;
            Encoding = Encoding.UTF8;
            CompressionAlgorithm = CompressionAlgorithm.None;
            EncryptionKey = null;
        }
    }
}
