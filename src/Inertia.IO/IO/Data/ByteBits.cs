using System;

namespace Inertia.IO
{
    public struct ByteBits
    {
        public static implicit operator byte(ByteBits byteBits) => byteBits._byte;
        public static implicit operator ByteBits(byte b) => new ByteBits(b);

        private byte _byte;

        public ByteBits(byte value)
        {
            _byte = value;
        }
        public ByteBits(params bool[] bits) : this()
        {
            Set(bits);
        }

        public bool this[int index]
        {
            get
            {
                return _byte.GetBit(index);
            }
            set
            {
                _byte = _byte.SetBit(index, value);
            }
        }

        public void Set(params bool[] bits)
        {
            if (bits.Length > 8)
            {
                throw new IndexOutOfRangeException($"Bits array size cannot be greater than 8.");
            }

            for (var i = 0; i < bits.Length; i++)
            {
                _byte.SetBitByRef(i, bits[i]);
            }
        }
        public void SetAll(bool state)
        {
            for (var i = 0; i < 8; i++)
            {
                _byte.SetBitByRef(i, state);
            }
        }
        public bool[] GetBits()
        {
            var bits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bits[i] = this[i];
            }

            return bits;
        }
    }
}