using System.Collections.Generic;
using System.Text;
using Inertia;

namespace System
{
    public static class Extensions
    {
        private readonly static Random Randomizer = new Random();

        public static void Shuffle<T>(this IList<T> collection)
        {
            var iStart = 0;
            T savedValue;

            while (iStart < collection.Count - 1)
            {
                int iRand = Randomizer.Next(iStart, collection.Count);
                savedValue = collection[iStart];
                collection[iStart++] = collection[iRand];
                collection[iRand] = savedValue;
            }
        }

        public static string GetSHA256(this string text)
        {
            return text.GetSHA256(Encoding.UTF8);
        }
        public static string GetSHA256(this string text, Encoding encoding)
        {
            return IOHelper.GetSHA256(encoding.GetBytes(text));
        }

        public static byte[] GzipCompress(this byte[] data, out bool hasBetterSize)
        {
            return IOHelper.GzipCompress(data, out hasBetterSize);
        }
        public static byte[] GzipDecompress(this byte[] compressedData)
        {
            return IOHelper.GzipDecompress(compressedData);
        }

        public static bool GetBit(this ref byte value, int index, EndiannessType endianness = EndiannessType.Auto)
        {
            if (index < 0 || index >= 8) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.Auto && !BitConverter.IsLittleEndian)
            {
                endianness = EndiannessType.BigEndian;
            }

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            return (value & (1 << index)) != 0;
        }
        public static byte SetBit(this ref byte value, int index, bool bitValue, EndiannessType endianness = EndiannessType.Auto)
        {
            if (index < 0 || index >= 8) throw new ArgumentOutOfRangeException(nameof(index));

            if (endianness == EndiannessType.Auto && !BitConverter.IsLittleEndian)
            {
                endianness = EndiannessType.BigEndian;
            }

            if (endianness == EndiannessType.BigEndian)
            {
                index = 7 - index;
            }

            if (bitValue)
            {
                value = (byte)(value | (1 << index));
            }
            else
            {
                value = (byte)(value & ~(1 << index));
            }

            return value;
        }
    }
}
