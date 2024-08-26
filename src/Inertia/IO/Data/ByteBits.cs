namespace System
{
    public sealed class ByteBits
    {
        public static bool operator ==(ByteBits b1, ByteBits b2)
        {
            if ((object)null == b1) return (object)null == b2;

            return b1.Byte == b2.Byte;
        }

        public static bool operator !=(ByteBits b1, ByteBits b2)
        {
            if ((object)null == b1) return (object)null != b2;

            return b1.Byte != b2.Byte;
        }

        public byte Byte { get; private set; }

        public ByteBits()
        {
        }
        public ByteBits(params bool[] bits)
        {
            Set(bits);
        }
        public ByteBits(byte value)
        {
            Byte = value;
        }

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return Byte.GetBit(index);
            }
            set
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Byte = Byte.SetBit(index, value);
            }
        }

        public void Set(params bool[] bits)
        {
            var length = Math.Min(bits.Length, 8);
            for (var i = 0; i < length; i++)
            {
                Byte = Byte.SetBit(i, bits[i]);
            }
        }
        public void SetAll(bool value)
        {
            for (var i = 0; i < 8; i++)
            {
                Byte = Byte.SetBit(i, value);
            }
        }
    }
}