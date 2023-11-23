using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    public class WriterParameters
    {
        public int Size { get; set; }
        public Encoding Encoding { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }
        public string? EncryptionKey { get; set; }

        public WriterParameters()
        {
            Size = 256;
            Encoding = Encoding.UTF8;
            CompressionAlgorithm = CompressionAlgorithm.None;
            EncryptionKey = null;
        }
    }
}
