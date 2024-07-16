using Inertia.IO;
using Inertia.Logging;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia
{
    public sealed class DataReader : IDisposable
    {
        private static Dictionary<Type, Func<DataReader, Type, object>> _readingDefinitions = new Dictionary<Type, Func<DataReader, Type, object>>
        {
            { typeof(bool), (reader, type) => reader.ReadBool() },
            { typeof(string), (reader, type) => reader.ReadString() },
            { typeof(float), (reader, type) => reader.ReadFloat() },
            { typeof(decimal), (reader, type) => reader.ReadDecimal() },
            { typeof(double), (reader, type) => reader.ReadDouble() },
            { typeof(byte), (reader, type) => reader.ReadByte() },
            { typeof(sbyte), (reader, type) => reader.ReadSByte() },
            { typeof(char), (reader, type) => reader.ReadChar() },
            { typeof(short), (reader, type) => reader.ReadShort() },
            { typeof(ushort), (reader, type) => reader.ReadUShort() },
            { typeof(int), (reader, type) => reader.ReadInt() },
            { typeof(uint), (reader, type) => reader.ReadUInt() },
            { typeof(long), (reader, type) => reader.ReadLong() },
            { typeof(ulong), (reader, type) => reader.ReadULong() },
            { typeof(DateTime), (reader, type) => reader.ReadDateTime() },
            { typeof(byte[]), (reader, type) => reader.ReadBytes() },
            { typeof(Enum), (reader, type) => reader.ReadEnum(type) }
        };

        public static void AddDeserializableType(Type type, Func<DataReader, Type, object> onDeserialize)
        {
            if (!_readingDefinitions.ContainsKey(type))
            {
                _readingDefinitions.Add(type, onDeserialize);
            }
            else
            {
                _readingDefinitions[type] = onDeserialize;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                return _stream != null ? _stream.Length : 0;
            }
        }
        public long UnreadedLength
        {
            get
            {
                return _stream != null ? (_stream.Length - _stream.Position) : 0;
            }
        }

        private BinaryReader _reader;
        private MemoryStream _stream;
        private DataReaderParameters _parameters;

        public DataReader() : this(new DataReaderParameters())
        {
        }
        public DataReader(DataReaderParameters parameters)
        {
            _parameters = parameters;
            _stream = new MemoryStream();
            _reader = new BinaryReader(_stream, parameters.Encoding);
        }
        public DataReader(byte[] data) : this(data, new DataReaderParameters())
        {
        }
        public DataReader(byte[] data, DataReaderParameters parameters) : this(parameters)
        {
            Fill(data);
        }

        public DataReader SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);
            
            if (position < 0) return this;

            _stream.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_stream == null) return 0;

            return _stream.Position;
        }
        public void Clear()
        {
            if (!IsDisposed && _stream != null)
            {
                _stream.Dispose();
                _stream = new MemoryStream();
            }
        }

        public DataReader Fill(byte[] data)
        {
            return Fill(data, TotalLength);
        }
        public DataReader Fill(byte[] data, long offset)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!string.IsNullOrWhiteSpace(_parameters.EncryptionKey))
            {
                using (var decryptResult = data.AesDecrypt(_parameters.EncryptionKey))
                {
                    data = decryptResult.GetDataOrThrow();
                }
            }

            if (_parameters.CompressionAlgorithm != CompressionAlgorithm.None)
            {
                var decompressionResult = _parameters.CompressionAlgorithm == CompressionAlgorithm.Deflate ?
                    data.DeflateDecompress() : data.GzipDecompress();

                using (decompressionResult)
                {
                    data = decompressionResult.GetDataOrThrow();
                }
            }

            var newLength = offset + data.Length;
            if (newLength > _stream.Length)
            {
                _stream.SetLength(newLength);
                _stream.Capacity = (int)newLength;
            }            

            var oldPosition = GetPosition();

            SetPosition(offset);
            _stream.Write(data);
            SetPosition(oldPosition);

            return this;
        }
        public DataReader Skip(int length)
        {
            return SetPosition(GetPosition() + length);
        }
        public DataReader RemoveReadedBytes()
        {
            this.ThrowIfDisposable(IsDisposed);

            var available = ReadBytes((int)UnreadedLength);

            _stream.SetLength(available.Length);
            _stream.Capacity = available.Length;

            if (available.Length > 0)
            {
                Fill(available, 0);
                SetPosition(0);
            }

            return this;
        }

        public bool ReadBool()
        {
            return _reader.ReadBoolean();
        }
        public string ReadString()
        {
            return _reader.ReadString();
        }
        public byte ReadByte()
        {
            return _reader.ReadByte();
        }
        public ByteBits ReadByteBits()
        {
            return new ByteBits(_reader.ReadByte());
        }
        public sbyte ReadSByte()
        {
            return _reader.ReadSByte();
        }
        public char ReadChar()
        {
            return _reader.ReadChar();
        }
        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }
        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }
        public decimal ReadDecimal()
        {
            return _reader.ReadDecimal();
        }
        public short ReadShort()
        {
            return _reader.ReadInt16();
        }
        public ushort ReadUShort()
        {
            return _reader.ReadUInt16();
        }
        public int ReadInt()
        {
            return _reader.ReadInt32();
        }
        public uint ReadUInt()
        {
            return _reader.ReadUInt32();
        }
        public long ReadLong()
        {
            return _reader.ReadInt64();
        }
        public ulong ReadULong()
        {
            return _reader.ReadUInt64();
        }
        public DateTime ReadDateTime(DateTimeKind kind = DateTimeKind.Local)
        {
            return new DateTime(_reader.ReadInt64(), kind);
        }
        public byte[] ReadBytes()
        {
            var length = _reader.ReadInt32();
            return _reader.ReadBytes(length);
        }
        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }
        public T ReadSerializable<T>() where T : ISerializable
        {
            return (T)ReadSerializable(typeof(T));
        }
        public object ReadSerializable(Type type)
        {
            if (!typeof(ISerializable).IsAssignableFrom(type)) return null;

            var cache = ReflectionProvider.GetSerializableObjectCache(type);
            if (cache == null) return null;

            var instance = cache.CreateInstance();
            if (instance != null)
            {
                ((ISerializable)instance).Deserialize(this);
                return instance;
            }

            return null;
        }
        public T ReadIEnumerable<T>() where T : IEnumerable
        {
            return (T)ReadIEnumerable(typeof(T));
        }
        public T[] ReadArray<T>()
        {
            var array = new T[_reader.ReadInt32()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ReadValue<T>();
            }

            return array;
        }
        public IDictionary ReadDictionary(Type dictionaryType)
        {
            if (!dictionaryType.IsGenericType || !typeof(IDictionary).IsAssignableFrom(dictionaryType)) throw new ArgumentNullException(nameof(dictionaryType));

            var count = ReadInt();
            var arguments = dictionaryType.GetGenericArguments();
            var dict = (IDictionary)Activator.CreateInstance(dictionaryType);

            for (var i = 0; i < count; i++)
            {
                var key = ReadValue(arguments[0]);
                var value = ReadValue(arguments[1]);

                dict.Add(key, value);
            }

            return dict;
        }
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            var dict = ReadDictionary(typeof(Dictionary<TKey, TValue>));
            return (Dictionary<TKey, TValue>)dict;
        }
        public T ReadEnum<T>()
        {
            return (T)ReadEnum(typeof(T));
        }
        public object ReadEnum(Type enumType)
        {
            if (!enumType.IsEnum) return null;

            var underlyingType = Enum.GetUnderlyingType(enumType);
            return ReadValue(underlyingType);
        }
        public T ReadAutoSerializable<T>()
        {
            return (T)ReadAutoSerializable(typeof(T));
        }
        public bool ReadAutoSerializable(object instance)
        {
            if (ReflectionProvider.TryGetSerializableProperties(instance.GetType(), out var propertiesDict))
            {
                var propertiesCount = _reader.ReadByte();
                for (var i = 0; i < propertiesCount; i++)
                {
                    var key = _reader.ReadString();
                    if (propertiesDict.TryGetValue(key, out var propertyMem))
                    {
                        propertyMem.ReadFrom(instance, this);
                    }
                }

                return true;
            }

            return false;
        }
        public object ReadAutoSerializable(Type type)
        {
            var instance = Activator.CreateInstance(type);
            if (!ReadAutoSerializable(instance))
            {
                throw new InvalidOperationException($"The type '{type.Name}' is not registered as AutoSerializable.");
            }

            return instance;
        }
        public T ReadValue<T>()
        {
            return (T)ReadValue(typeof(T));
        }
        public object ReadValue(Type valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));

            var readerMethodFound = _readingDefinitions.TryGetValue(valueType, out Func<DataReader, Type, object> action);
            if (!readerMethodFound && valueType.IsEnum)
            {
                readerMethodFound = _readingDefinitions.TryGetValue(typeof(Enum), out action);
            }

            if (readerMethodFound)
            {
                return action(this, valueType);
            }
            else
            {
                if (valueType.GetCustomAttribute<AutoSerializableAttribute>() != null)
                {
                    return ReadAutoSerializable(valueType);
                }
                else if (typeof(ISerializable).IsAssignableFrom(valueType))
                {
                    return ReadSerializable(valueType);
                }
                else if (typeof(IDictionary).IsAssignableFrom(valueType))
                {
                    return ReadDictionary(valueType);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(valueType))
                {
                    return ReadIEnumerable(valueType);
                }                

                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private object ReadIEnumerable(Type valueType)
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

            var array = Array.CreateInstance(elementType, ReadInt());
            for (var i = 0; i < array.Length; i++)
            {
                array.SetValue(ReadValue(elementType), i);
            }

            if (!isArray && isList)
            {
                var concreteListType = typeof(List<>).MakeGenericType(elementType);
                return Activator.CreateInstance(concreteListType, new object[] { array });
            }

            return array;
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _reader.Dispose();
                _reader = null;
                _stream = null;
            }

            IsDisposed = true;
        }
    }
}