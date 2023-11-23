using Inertia.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Inertia
{
    public sealed class BasicReader : IDisposable
    {
        private static Dictionary<Type, Func<BasicReader, object>> _typageDefinitions = new Dictionary<Type, Func<BasicReader, object>>
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

        public static void AddDeserializableType(Type type, Func<BasicReader, object> onDeserialize)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, onDeserialize);
            }
            else
            {
                _typageDefinitions[type] = onDeserialize;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                return _reader != null ? _reader.Length : 0;
            }
        }
        public long UnreadedLength
        {
            get
            {
                return _reader != null ? (_reader.Length - _reader.Position) : 0;
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
        public BasicReader(ReaderFilling fillingObject) : this(fillingObject, Encoding.UTF8)
        {
        }
        public BasicReader(ReaderFilling fillingObject, Encoding encoding) : this(encoding)
        {
            Fill(fillingObject);
        }

        public BasicReader SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);
            
            if (position < 0) return this;

            _reader.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_reader == null) return 0;

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

        public BasicReader Fill(ReaderFilling fillingObject)
        {
            return Fill(fillingObject, TotalLength);
        }
        public BasicReader Fill(ReaderFilling fillingObject, long offset)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!string.IsNullOrWhiteSpace(fillingObject.EncryptionKey))
            {
                using (var decryptResult = fillingObject.Data.AesDecrypt(fillingObject.EncryptionKey))
                {
                    if (decryptResult.Success)
                    {
                        fillingObject.Data = decryptResult.Data;
                    }
                    else throw decryptResult.Error;
                }
            }

            if (fillingObject.CompressionAlgorithm != CompressionAlgorithm.None)
            {
                var transformResult =
                    fillingObject.CompressionAlgorithm == CompressionAlgorithm.Deflate ? fillingObject.Data.DeflateDecompress() :
                    fillingObject.Data.GzipDecompress();

                using (transformResult)
                {
                    if (transformResult.Success)
                    {
                        fillingObject.Data = transformResult.Data;
                    }
                    else throw transformResult.Error;
                }
            }

            var newLength = offset + fillingObject.Data.Length;
            if (newLength > _reader.Length)
            {
                _reader.SetLength(newLength);
                _reader.Capacity = (int)newLength;
            }            

            var oldPosition = GetPosition();

            SetPosition(offset);
            _reader.Write(fillingObject.Data);
            SetPosition(oldPosition);

            fillingObject.Dispose();
            return this;
        }

        public BasicReader Skip(int length)
        {
            return SetPosition(GetPosition() + length);
        }
        public BasicReader RemoveReadedBytes()
        {
            this.ThrowIfDisposable(IsDisposed);

            var available = GetBytes((int)UnreadedLength);

            _reader.SetLength(available.Length);
            _reader.Capacity = available.Length;

            if (available.Length > 0)
            {
                Fill(new ReaderFilling(available), 0);
                SetPosition(0);
            }

            return this;
        }

        public bool GetBool()
        {
            if (IsReadable(1))
            {
                return Convert.ToBoolean(_reader.ReadByte());
            }
            else return default;
        }
        public string GetString()
        {
            var data = GetBytes();
            if (data.Length > 0)
            {
                return _encoding.GetString(data);
            }
            else return string.Empty;
        }
        public byte GetByte()
        {
            if (IsReadable(1))
            {
                return (byte)_reader.ReadByte();
            }
            else return default;
        }
        public BitByte GetBitByte()
        {
            return new BitByte(GetByte());
        }
        public sbyte GetSByte()
        {
            if (IsReadable(1))
            {
                return unchecked((sbyte)_reader.ReadByte());
            }
            else return default;
        }
        public char GetChar()
        {
            if (TryReadSize(1, out var data))
            {
                return BitConverter.ToChar(data);
            }
            else return default;
        }
        public float GetFloat()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToSingle(data);
            }
            else return default;
        }
        public double GetDouble()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToDouble(data);
            }
            else return default;
        }
        public decimal GetDecimal()
        {
            if (TryReadSize(16, out var data))
            {
                var bits = new[]
                {
                    BitConverter.ToInt32(data, 0),
                    BitConverter.ToInt32(data, 4),
                    BitConverter.ToInt32(data, 8),
                    BitConverter.ToInt32(data, 12)
                };

                return new decimal(bits);
            }
            else return default;
        }
        public short GetShort()
        {
            if (TryReadSize(2, out var data))
            {
                return BitConverter.ToInt16(data);
            }
            else return default;
        }
        public ushort GetUShort()
        {
            if (TryReadSize(2, out var data))
            {
                return BitConverter.ToUInt16(data);
            }
            else return default;
        }
        public int GetInt()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToInt32(data);
            }
            else return default;
        }
        public uint GetUInt()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToUInt32(data);
            }
            else return default;
        }
        public long GetLong()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToInt64(data);
            }
            else return default;
        }
        public ulong GetULong()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToUInt64(data);
            }
            else return default;
        }
        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }
        public DateTime GetDateTime(DateTimeKind kind)
        {
            return DateTime.SpecifyKind(GetDateTime(), kind);
        }
        public byte[] GetBytes()
        {
            if (TryReadSize(4, out var data))
            {
                var length = BitConverter.ToInt32(data);
                return GetBytes(length);
            }
            else return new byte[0];
        }
        public byte[] GetBytes(int length)
        {
            if (TryReadSize(length, out var data))
            {
                return data;
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
                .Select(p => null as object)
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
                throw new ArgumentNullException($"Type '{ type.Name }' is not '{ nameof(IAutoSerializable) }'");
            }

            var instance = Activator.CreateInstance(type);
            return GetAutoSerializable((IAutoSerializable)instance);
        }
        public IAutoSerializable GetAutoSerializable(IAutoSerializable instance)
        {
            if (ReflectionProvider.TryGetProperties(instance.GetType(), out ReflectionProvider.SerializablePropertyMemory[] properties))
            {
                foreach (var property in properties)
                {
                    property.Read(instance, this);
                }
            }

            return instance;
        }
        public IEnumerable GetIEnumerable(Type valueType)
        {
            var isList = typeof(IList).IsAssignableFrom(valueType);
            var isArray = valueType.IsArray;
            Type elementType = null;

            if (valueType.IsGenericType)
            {
                var genericArgTypes = valueType.GetGenericArguments();
                if (genericArgTypes.Length > 0)
                {
                    elementType = genericArgTypes[0];
                }
            }
            else
            {
                elementType = valueType.GetElementType();
            }

            if (elementType == null)
            {
                throw new NotSupportedException($"Type '{valueType}' cannot be converted to IEnumerable. Only array and list types can be converted.");
            }

            var array = Array.CreateInstance(elementType, GetInt());
            for (var i = 0; i < array.Length; i++)
            {
                array.SetValue(GetValue(elementType), i);
            }

            if (!isArray && isList)
            {
                var concreteListType = typeof(List<>).MakeGenericType(elementType);
                return (IEnumerable)Activator.CreateInstance(concreteListType, new object[] { array });
            }

            return array;
        }
        public IEnumerable<T> GetIEnumerable<T>()
        {
            var array = new T[GetInt()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = GetValue<T>();
            }

            return array;
        }
        public IDictionary GetDictionary(Type dictionaryType)
        {
            if (!dictionaryType.IsGenericType) throw new ArgumentNullException(nameof(dictionaryType));

            var count = GetInt();
            var arguments = dictionaryType.GetGenericArguments();
            var dict = (IDictionary)Activator.CreateInstance(dictionaryType);

            for (var i = 0; i < count; i++)
            {
                var key = GetValue(arguments[0]);
                var value = GetValue(arguments[1]);

                dict.Add(key, value);
            }

            return dict;
        }
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            var dict = GetDictionary(typeof(Dictionary<TKey, TValue>));
            return (Dictionary<TKey, TValue>)dict;
        }
        public T GetValue<T>()
        {
            return (T)GetValue(typeof(T));
        }
        public object GetValue(Type valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));

            if (_typageDefinitions.TryGetValue(valueType, out Func<BasicReader, object> action))
            {
                return action(this);
            }
            else
            {
                if (typeof(IAutoSerializable).IsAssignableFrom(valueType))
                {
                    return GetAutoSerializable(valueType);
                }
                else if (typeof(ISerializableObject).IsAssignableFrom(valueType))
                {
                    return GetSerializableObject(valueType);
                }
                else if (typeof(IDictionary).IsAssignableFrom(valueType))
                {
                    return GetDictionary(valueType);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(valueType))
                {
                    return GetIEnumerable(valueType);
                }                

                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool IsReadable(int length)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (length <= 0) throw new ArgumentNullException(nameof(length));

            return UnreadedLength >= length;
        }
        private bool TryReadSize(int length, out byte[] data)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (length < 0) throw new ArgumentNullException(nameof(length));

            if (length == 0)
            {
                data = new byte[0];
                return true;
            }

            if (UnreadedLength >= length)
            {
                data = new byte[length];
                var readedLength = _reader.Read(data);

                return readedLength == data.Length;
            }

            data = null;
            return false;
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }

            IsDisposed = true;
        }
    }
}