using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia
{
    public sealed class BasicReader : IDisposable
    {
        private static Dictionary<Type, BasicReturnAction<BasicReader, object>> _typageDefinitions = new Dictionary<Type, BasicReturnAction<BasicReader, object>>
        {
            { typeof(bool), (reader) => reader.GetBool() },
            { typeof(string), (reader) => reader.GetString() },
            { typeof(float), (reader) => reader.GetFloat() },
            { typeof(decimal), (reader) => reader.GetDecimal() },
            { typeof(double), (reader) => reader.GetDouble() },
            { typeof(byte), (reader) => reader.GetByte() },
            { typeof(sbyte), (reader) => reader.GetSByte() },
            { typeof(char), (reader) => reader.GetChar() },
            { typeof(short), (reader) => reader.GetShort() },
            { typeof(ushort), (reader) => reader.GetUShort() },
            { typeof(int), (reader) => reader.GetInt() },
            { typeof(uint), (reader) => reader.GetUInt() },
            { typeof(long), (reader) => reader.GetLong() },
            { typeof(ulong), (reader) => reader.GetULong() },
            { typeof(DateTime), (reader) => reader.GetDateTime() },
            { typeof(byte[]), (reader) => reader.GetBytes() },
        };

        public static void SetDeserializableType(Type type, BasicReturnAction<BasicReader, object> deserialization)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, deserialization);
            }
            else
            {
                _typageDefinitions[type] = deserialization;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                if (_reader != null) return _reader.Length;

                return 0;
            }
        }
        public long UnreadedLength
        {
            get
            {
                if (_reader != null) return _reader.Length - _reader.Position;

                return 0;
            }
        }

        private MemoryStream _reader;
        private readonly Encoding _encoding;

        public BasicReader() : this(Encoding.UTF8)
        {
        }
        public BasicReader(Encoding encoding)
        {
            _encoding = encoding;
            _reader = new MemoryStream();
        }
        public BasicReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
        public BasicReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data, data.Length);
        }

        public BasicReader SetPosition(long position)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            if (position < 0 || position > TotalLength) return this;

            _reader.Position = position;
            return this;
        }
        public long GetPosition()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            return _reader.Position;
        }

        public void Clear()
        {
            if (!IsDisposed && _reader != null)
            {
                _reader.Dispose();
                _reader = new MemoryStream();
            }
        }

        public BasicReader Fill(byte[] buffer, int length)
        {
            return Fill(buffer, length, TotalLength);
        }
        public BasicReader Fill(byte[] buffer, int length, long positionOffset)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            var newLength = positionOffset + length;
            if (newLength > _reader.Length)
            {
                _reader.SetLength(newLength);
                _reader.Capacity = (int)newLength;
            }            

            var oldPosition = GetPosition();

            SetPosition(positionOffset);
            _reader.Write(buffer, 0, length);
            SetPosition(oldPosition);

            return this;
        }

        public BasicReader Skip(int length)
        {
            return SetPosition(GetPosition() + length);
        }
        public BasicReader RemoveReadedBytes()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            var available = GetBytes((int)UnreadedLength);

            _reader.SetLength(available.Length);
            _reader.Capacity = available.Length;

            if (available.Length > 0)
            {
                Fill(available, available.Length, 0);
                SetPosition(0);
            }

            return this;
        }

        public bool GetBool()
        {
            if (IsUpdatable(1))
            {
                return Convert.ToBoolean(_reader.ReadByte());
            }
            else return default;
        }
        public bool[] GetBoolFlag(int length)
        {
            return GetByte().GetBits(length);
        }
        public string GetString()
        {
            var b = GetBytes();
            if (b.Length > 0)
            {
                return _encoding.GetString(b);
            }
            else return string.Empty;
        }
        public byte GetByte()
        {
            if (IsUpdatable(1))
            {
                return (byte)_reader.ReadByte();
            }
            else return default;
        }
        public sbyte GetSByte()
        {
            if (IsUpdatable(1))
            {
                return unchecked((sbyte)_reader.ReadByte());
            }
            else return default;
        }
        public char GetChar()
        {
            if (IsUpdatable(1))
            {
                return BitConverter.ToChar(ReadSize(1), 0);
            }
            else return default;
        }
        public float GetFloat()
        {
            if (IsUpdatable(4))
            {
                return BitConverter.ToSingle(ReadSize(4), 0);
            }
            else return default;
        }
        public double GetDouble()
        {
            if (IsUpdatable(8))
            {
                return BitConverter.ToDouble(ReadSize(8), 0);
            }
            else return default;
        }
        public decimal GetDecimal()
        {
            return (decimal)GetDouble();
        }
        public short GetShort()
        {
            if (IsUpdatable(2))
            {
                return BitConverter.ToInt16(ReadSize(2), 0);
            }
            else return default;
        }
        public ushort GetUShort()
        {
            if (IsUpdatable(2))
            {
                return BitConverter.ToUInt16(ReadSize(2), 0);
            }
            else return default;
        }
        public int GetInt()
        {
            if (IsUpdatable(4))
            {
                return BitConverter.ToInt32(ReadSize(4), 0);
            }
            else return default;
        }
        public uint GetUInt()
        {
            if (IsUpdatable(4))
            {
                return BitConverter.ToUInt32(ReadSize(4), 0);
            }
            else return default;
        }
        public long GetLong()
        {
            if (IsUpdatable(8))
            {
                return BitConverter.ToInt64(ReadSize(8), 0);
            }
            else return default;
        }
        public ulong GetULong()
        {
            if (IsUpdatable(8))
            {
                return BitConverter.ToUInt64(ReadSize(8), 0);
            }
            else return default;
        }
        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }
        public byte[] GetBytes()
        {
            if (IsUpdatable(4))
            {
                var length = BitConverter.ToInt32(ReadSize(4), 0);
                return GetBytes(length);
            }
            else return new byte[0];
        }
        public byte[] GetBytes(int length)
        {
            if (IsUpdatable(length))
            {
                return ReadSize(length);
            }
            else return new byte[0];
        }
        public T GetSerializableObject<T>() where T : ISerializableObject
        {
            return (T)GetSerializableObject(typeof(T));
        }
        public object GetSerializableObject(Type type)
        {
            if (!typeof(ISerializableObject).IsAssignableFrom(type)) return null;

            var parameters = type
                .GetConstructors()[0].GetParameters()
                .Select(p => (object)null)
                .ToArray();
            var instance = Activator.CreateInstance(type, parameters);
            if (instance != null)
            {
                ((ISerializableObject)instance).Deserialize(this);
                return instance;
            }

            return null;
        }
        public T GetAutoSerializable<T>() where T : IAutoSerializable
        {
            return (T)GetAutoSerializable(typeof(T));
        }
        public void GetAutoSerializable<T>(T instance) where T : IAutoSerializable
        {
            GetAutoSerializable((IAutoSerializable)instance);
        }
        public IAutoSerializable GetAutoSerializable(Type type)
        {
            if (!typeof(IAutoSerializable).IsAssignableFrom(type))
            {
                throw new Exception($"Type '{ type.Name }' isn't '{ nameof(IAutoSerializable) }'");
            }

            var instance = Activator.CreateInstance(type);
            return GetAutoSerializable((IAutoSerializable)instance);
        }
        public IAutoSerializable GetAutoSerializable(IAutoSerializable instance)
        {
            if (ReflectionProvider.TryGetFields(instance.GetType(), out ReflectionProvider.SerializableFieldMemory[] fields))
            {
                foreach (var field in fields)
                {
                    field.Read(instance, this);
                }
            }

            return instance;
        }

        public T GetValue<T>()
        {
            return (T)GetValue(typeof(T));
        }
        public object GetValue(Type type)
        {
            if (_typageDefinitions.TryGetValue(type, out BasicReturnAction<BasicReader, object> action))
            {
                return action(this);
            }
            else
            {
                if (type.GetInterface(nameof(IAutoSerializable)) != null)
                {
                    return GetAutoSerializable(type);
                }
                else if (type.GetInterface(nameof(ISerializableObject)) != null)
                {
                    return GetSerializableObject(type);
                }
                else return null;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _reader.Close();
                _reader.Dispose();

                IsDisposed = true;
            }
        }

        private bool IsUpdatable(long length)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            return UnreadedLength >= length;
        }
        private byte[] ReadSize(int size)
        {
            var buffer = new byte[size];
            _reader.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}