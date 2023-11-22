using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.IO;

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

        public static bool GetBit(this byte value, int index)
        {
            return GetBit(value, index, EndiannessType.Auto);
        }
        public static bool GetBit(this byte value, int index, EndiannessType endianness)
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
        public static byte SetBit(this byte value, int index, bool bit)
        {
            return SetBit(value, index, bit, EndiannessType.Auto);
        }
        public static byte SetBit(this byte value, int index, bool bit, EndiannessType endianness)
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
        public static void SetBitRef(this ref byte value, int index, bool bit)
        {
            value = SetBit(value, index, bit, EndiannessType.Auto);
        }
        public static void SetBitRef(this ref byte value, int index, bool bit, EndiannessType endianness)
        {
            value = SetBit(value, index, bit, endianness);
        }

        public static void ThrowIfDisposable(this IDisposable disposable, bool isDisposed)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(disposable.GetType().Name);
            }
        }
    }
}