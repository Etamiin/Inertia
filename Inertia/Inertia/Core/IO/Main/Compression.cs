using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class Compression
    {
        public static bool Compress(byte[] data, out byte[] buffer)
        {
            buffer = AcedDeflator.Instance.Compress(data, 0, data.Length, AcedCompressionLevel.Fast, 0, 0);
            AcedDeflator.Instance.Dispose();

            var isBetter = buffer.Length < data.Length;
            if (!isBetter)
                buffer = data;

            return isBetter;
        }
        public static byte[] Decompress(byte[] compressedData)
        {
            var result = AcedInflator.Instance.Decompress(compressedData, 0, 0, 0);
            AcedInflator.Instance.Dispose();

            return result;
        }

    }
}
