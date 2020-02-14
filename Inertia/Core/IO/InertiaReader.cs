using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class InertiaReader
    {
        #region Public variables

        public bool IsDisposed { get; private set; }
        public int TotalLength
        {
            get
            {
                if (Reader == null)
                    return 0;

                return (int)Reader.BaseStream.Length;
            }
        }
        public int UnreadedLength
        {
            get
            {
                if (Reader == null)
                    return 0;

                return LastRealBytePosition - Position;
            }
        }
        public int Position
        {
            get
            {
                if (Reader == null)
                    return 0;

                return (int)Reader.BaseStream.Position;
            }
            set
            {
                Reader.BaseStream.Position = value;
            }
        }

        #endregion

        #region Private variables

        private BinaryReader Reader;
        private int LastRealBytePosition;

        #endregion

        #region Constructors

        internal InertiaReader()
        {
            Reader = new BinaryReader(new MemoryStream(), InertiaConfiguration.BaseEncodage);
        }
        public InertiaReader(byte[] data) : this()
        {
            Fill(data);
        }

        #endregion

        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaReader));

            if (Reader == null)
                return;

            Reader.Dispose();
            Reader = new BinaryReader(new MemoryStream(), InertiaConfiguration.BaseEncodage);
        }
        
        public InertiaReader Fill(byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaReader));

            var newLength = LastRealBytePosition + data.Length;
            var currentPosition = Position;

            if (TotalLength < newLength)
                Reader.BaseStream.SetLength(newLength);

            Position = LastRealBytePosition;
            Reader.BaseStream.Write(data, 0, data.Length);

            LastRealBytePosition = Position;
            Position = currentPosition;
            
            return this;
        }
        public InertiaReader Skip(int length)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaReader));

            Position += length;
            return this;
        }
        public InertiaReader RemoveReadedBytes()
        {
            var available = GetBytes(UnreadedLength);

            Reader.BaseStream.SetLength(available.Length);
            Position = 0;
            LastRealBytePosition = 0;
            if (available.Length > 0)
                Fill(available);

            return this;
        }

        private bool IsUpdatable(int length)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaReader));
            return UnreadedLength >= length;
        }

        public bool GetBool()
        {
            if (IsUpdatable(1))
                return Reader.ReadBoolean();

            return default;
        }
        public string GetString()
        {
            return GetString(InertiaConfiguration.BaseEncodage);
        }
        public string GetString(Encoding encodage)
        {
            var data = GetBytes();
            if (data.Length == 0)
                return string.Empty;
            return encodage.GetString(data);
        }
        public byte GetByte()
        {
            if (IsUpdatable(1))
                return Reader.ReadByte();

            return default;
        }
        public sbyte GetSByte()
        {
            if (IsUpdatable(1))
                return Reader.ReadSByte();

            return default;
        }
        public char GetChar()
        {
            if (IsUpdatable(2))
                return Reader.ReadChar();
            return default;
        }
        public short GetShort()
        {
            if (IsUpdatable(2))
                return Reader.ReadInt16();

            return default;
        }
        public ushort GetUShort()
        {
            if (IsUpdatable(2))
                return Reader.ReadUInt16();

            return default;
        }
        public float GetFloat()
        {
            if (IsUpdatable(4))
                return Reader.ReadSingle();

            return default;
        }
        public double GetDouble()
        {
            if (IsUpdatable(8))
                return Reader.ReadDouble();

            return default;
        }
        public decimal GetDecimal()
        {
            if (IsUpdatable(16))
                return Reader.ReadDecimal();

            return default;
        }
        public int GetInt()
        {
            if (IsUpdatable(4))
                return Reader.ReadInt32();

            return default;
        }
        public uint GetUInt()
        {
            if (IsUpdatable(4))
                return Reader.ReadUInt32();

            return default;
        }
        public long GetLong()
        {
            if (IsUpdatable(8))
                return Reader.ReadInt64();

            return default;
        }
        public ulong GetULong()
        {
            if (IsUpdatable(8))
                return Reader.ReadUInt64();

            return default;
        }
        public byte[] GetBytes()
        {
            if (IsUpdatable(4))
            {
                var length = GetInt();
                return GetBytes(length);
            }

            return new byte[] { };
        }
        public byte[] GetBytes(int length)
        {
            if (IsUpdatable(length))
                return Reader.ReadBytes(length);

            return new byte[] { };
        }
        public T GetSerializableObject<T>() where T : ISerializableObject
        {
            return (T)GetSerializableObject(typeof(T));
        }
        private ISerializableObject GetSerializableObject(Type serializableType)
        {
            var constructor = serializableType.GetConstructors()[0];
            var final = (ISerializableObject)constructor.Invoke(new object[constructor.GetParameters().Length]);
            var finalReader = new InertiaReader(GetBytes());

            final.Deserialize(finalReader);
            finalReader.Dispose();

            return final;
        }

        public object GetValue(Type valueType)
        {
            object value;
            if (valueType == typeof(float))
                value = GetFloat();
            else if (valueType == typeof(decimal))
                value = GetDecimal();
            else if (valueType == typeof(double))
                value = GetDouble();
            else if (valueType == typeof(short))
                value = GetShort();
            else if (valueType == typeof(ushort))
                value = GetUShort();
            else if (valueType == typeof(int))
                value = GetInt();
            else if (valueType == typeof(uint))
                value = GetUInt();
            else if (valueType == typeof(string))
                value = GetString();
            else if (valueType == typeof(bool))
                value = GetBool();
            else if (valueType == typeof(byte))
                value = GetByte();
            else if (valueType == typeof(sbyte))
                value = GetSByte();
            else if (valueType == typeof(long))
                value = GetLong();
            else if (valueType == typeof(ulong))
                value = GetULong();
            else if (valueType == typeof(byte[]))
                value = GetBytes();
            else if (valueType.GetInterface(nameof(ISerializableObject)) != null)
                value = GetSerializableObject(valueType);
            else
                value = GetString();

            return value;
        }
        public IList GetValues(params Type[] valuesType)
        {
            var values = new object[valuesType.Length];

            var i = 0;
            foreach (var type in valuesType)
                values[i++] = GetValue(type);

            return values;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            Reader.Close();
            Reader.Dispose();
            Reader = null;
            IsDisposed = true;
        }
    }
}
