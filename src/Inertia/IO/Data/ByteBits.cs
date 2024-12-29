namespace System
{
    public sealed class ByteBits
    {
        public static implicit operator byte(ByteBits byteBits) => byteBits._byte;
        public static implicit operator ByteBits(byte b) => new ByteBits(b);

        private byte _byte;

        public ByteBits()
        {
        }
        public ByteBits(params bool[] bits)
        {
            Set(bits);
        }
        public ByteBits(byte value)
        {
            _byte = value;
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
            if (bits.Length > 8) throw new IndexOutOfRangeException($"Bits array size cannot be greater than 8.");

            var length = Math.Min(bits.Length, 8);
            for (var i = 0; i < length; i++)
            {
                _byte = _byte.SetBit(i, bits[i]);
            }
        }
        public void SetAll(bool value)
        {
            for (var i = 0; i < 8; i++)
            {
                _byte = _byte.SetBit(i, value);
            }
        }
    }
}