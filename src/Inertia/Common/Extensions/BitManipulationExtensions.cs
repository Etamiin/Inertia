using Inertia;

namespace System
{
    public static class BitManipulationExtensions
    {
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
    }
}
