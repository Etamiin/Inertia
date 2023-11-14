using Inertia;

namespace System
{
    public sealed class BitByte : ISerializableObject
    {
        public byte Byte { get; private set; }

        public BitByte()
        {
        }
        public BitByte(params bool[] bits)
        {
            Set(bits);
        }
        public BitByte(byte value)
        {
            Byte = value;
        }

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index > 7)
                {
                    throw new IndexOutOfRangeException();
                }

                return Byte.GetBit(index);
            }
            set
            {
                if (index < 0 || index > 7)
                {
                    throw new IndexOutOfRangeException();
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

        public void Serialize(BasicWriter writer)
        {
            writer.SetByte(Byte);
        }
        public void Deserialize(BasicReader reader)
        {
            Byte = reader.GetByte();
        }
    }
}