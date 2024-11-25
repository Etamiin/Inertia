using Inertia.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Inertia
{
    public class DataReader : IDisposable
    {
        protected static Dictionary<Type, Func<DataReader, Type, object>> Definitions { get; private set; } = new Dictionary<Type, Func<DataReader, Type, object>>
        {
            { typeof(bool), (reader, _) => reader.ReadBool() },
            { typeof(string), (reader, _) => reader.ReadString() },
            { typeof(float), (reader, _) => reader.ReadFloat() },
            { typeof(decimal), (reader, _) => reader.ReadDecimal() },
            { typeof(double), (reader, _) => reader.ReadDouble() },
            { typeof(byte), (reader, _) => reader.ReadByte() },
            { typeof(sbyte), (reader, _) => reader.ReadSByte() },
            { typeof(char), (reader, _) => reader.ReadChar() },
            { typeof(short), (reader, _) => reader.ReadShort() },
            { typeof(ushort), (reader, _) => reader.ReadUShort() },
            { typeof(int), (reader, _) => reader.ReadInt() },
            { typeof(uint), (reader, _) => reader.ReadUInt() },
            { typeof(long), (reader, _) => reader.ReadLong() },
            { typeof(ulong), (reader, _) => reader.ReadULong() },
            { typeof(DateTime), (reader, _) => reader.ReadDateTime(DateTimeKind.Local) },
            { typeof(byte[]), (reader, _) => reader.ReadBytes() },
            { typeof(Enum), (reader, type) => reader.ReadEnum(type) },
            { typeof(ByteBits), (reader, _) => reader.ReadByteBits() }
        };

        public static void AddDeserializableType(Type type, Func<DataReader, Type, object> onDeserialize)
        {
            Definitions[type] = onDeserialize;
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength => _binaryReader.BaseStream.Length;
        public long UnreadedLength => _binaryReader.BaseStream.Length - _binaryReader.BaseStream.Position;

        private BinaryReader _binaryReader;

        public DataReader() : this(Encoding.UTF8)
        {
        }
        public DataReader(Encoding encoding)
        {
            _binaryReader = new BinaryReader(new MemoryStream(), encoding);
        }
        public DataReader(byte[] data) : this(data, Encoding.UTF8, string.Empty, CompressionAlgorithm.None)
        {
        }
        public DataReader(byte[] data, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(data, Encoding.UTF8, encryptionKey, compressionAlgorithm)
        {
        }
        public DataReader(byte[] data, Encoding encoding) : this(data, encoding, null, CompressionAlgorithm.None)
        {
        }
        public DataReader(byte[] data, Encoding encoding, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(encoding)
        {
            Fill(data, encryptionKey, compressionAlgorithm);
        }

        public DataReader SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (position < 0 || position > TotalLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            _binaryReader.BaseStream.Position = position;
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            return _binaryReader.BaseStream.Position;
        }
        public void Clear()
        {
            this.ThrowIfDisposable(IsDisposed);

            RefreshStreamLength(0);
        }

        public virtual DataReader Fill(byte[] data)
        {
            return Fill(data, (int)TotalLength, null, CompressionAlgorithm.None);
        }
        public virtual DataReader Fill(byte[] data, string encryptionKey)
        {
            return Fill(data, (int)TotalLength, encryptionKey, CompressionAlgorithm.None);
        }
        public virtual DataReader Fill(byte[] data, CompressionAlgorithm compressionAlgorithm)
        {
            return Fill(data, (int)TotalLength, null, compressionAlgorithm);
        }
        public virtual DataReader Fill(byte[] data, string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            return Fill(data, (int)TotalLength, encryptionKey, compressionAlgorithm);
        }
        public virtual DataReader Fill(byte[] data, int offset)
        {
            return Fill(data, offset, null, CompressionAlgorithm.None);
        }
        public virtual DataReader Fill(byte[] data, int offset, string encryptionKey)
        {
            return Fill(data, offset, encryptionKey, CompressionAlgorithm.None);
        }
        public virtual DataReader Fill(byte[] data, int offset, CompressionAlgorithm compressionAlgorithm)
        {
            return Fill(data, offset, null, compressionAlgorithm);
        }
        public virtual DataReader Fill(byte[] data, int offset, string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {
                using (var decryptResult = data.AesDecrypt(encryptionKey))
                {
                    data = decryptResult.GetDataOrThrow();
                }
            }

            if (compressionAlgorithm != CompressionAlgorithm.None)
            {
                using (var decompressionResult = data.Decompress(compressionAlgorithm))
                {
                    data = decompressionResult.GetDataOrThrow();
                }
            }

            var newLength = offset + data.Length;
            if (newLength > _binaryReader.BaseStream.Length)
            {
                RefreshStreamLength(newLength);
            }

            var beforeFillPosition = GetPosition();

            SetPosition(offset);
            _binaryReader.BaseStream.Write(data);
            SetPosition(beforeFillPosition);

            return this;
        }
        public virtual DataReader Skip(int length)
        {
            return SetPosition(GetPosition() + length);
        }
        public virtual DataReader RemoveReadedBytes()
        {
            this.ThrowIfDisposable(IsDisposed);

            var available = ReadBytes((int)UnreadedLength);
            RefreshStreamLength(available.Length);

            if (available.Length > 0)
            {
                Fill(available, 0);
                SetPosition(0);
            }

            return this;
        }

        public virtual bool ReadBool() => _binaryReader.ReadBoolean();
        public virtual byte ReadByte() => _binaryReader.ReadByte();
        public virtual sbyte ReadSByte() => _binaryReader.ReadSByte();
        public virtual short ReadShort() => _binaryReader.ReadInt16();
        public virtual ushort ReadUShort() => _binaryReader.ReadUInt16();
        public virtual int ReadInt() => _binaryReader.ReadInt32();
        public virtual uint ReadUInt() => _binaryReader.ReadUInt32();
        public virtual long ReadLong() => _binaryReader.ReadInt64();
        public virtual ulong ReadULong() => _binaryReader.ReadUInt64();
        public virtual float ReadFloat() => _binaryReader.ReadSingle();
        public virtual double ReadDouble() => _binaryReader.ReadDouble();
        public virtual decimal ReadDecimal() => _binaryReader.ReadDecimal();
        public virtual char ReadChar() => _binaryReader.ReadChar();
        public virtual string ReadString() => _binaryReader.ReadString();
        public virtual ByteBits ReadByteBits() => _binaryReader.ReadByte();
        public virtual DateTime ReadDateTime() => new DateTime(_binaryReader.ReadInt64(), DateTimeKind.Utc);
        public virtual DateTime ReadDateTime(DateTimeKind kind) => new DateTime(_binaryReader.ReadInt64(), kind);
        public virtual byte[] ReadBytes() => ReadBytes(_binaryReader.ReadInt32());
        public virtual byte[] ReadBytes(int count) => _binaryReader.ReadBytes(count);
        public virtual object ReadSerializable(Type type)
        {
            if (!typeof(ISerializable).IsAssignableFrom(type))
            {
                throw new InvalidCastException($"The type '{type.Name}' has no '{nameof(ISerializable)}' interface assigned.");
            }

            var instance = (ISerializable)Activator.CreateInstance(type);
            instance.Deserialize(ReadByte(), this);

            return instance;
        }
        public virtual T[] ReadArray<T>()
        {
            var array = new T[ReadInt()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ReadValue<T>();
            }

            return array;
        }
        public virtual IDictionary ReadDictionary(Type dictionaryType)
        {
            if (!dictionaryType.IsGenericType || !typeof(IDictionary).IsAssignableFrom(dictionaryType))
            {
                throw new TypeAccessException($"The type '{dictionaryType.Name}' is not a valid generic dictionary type.");
            }

            var arguments = dictionaryType.GetGenericArguments();
            var dict = (IDictionary)Activator.CreateInstance(dictionaryType);
            var count = ReadInt();

            for (var i = 0; i < count; i++)
            {
                var key = ReadValue(arguments[0]);
                var value = ReadValue(arguments[1]);

                dict.Add(key, value);
            }

            return dict;
        }
        public virtual object ReadEnum(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new TypeAccessException($"The type '{enumType.Name}' is not a valid enum object.");
            }

            var underlyingType = Enum.GetUnderlyingType(enumType);
            return ReadValue(underlyingType);
        }
        public virtual object ReadAutoSerializable(Type type)
        {
            var instance = Activator.CreateInstance(type);
            if (!TryReadAutoSerializable(instance))
            {
                throw new InvalidOperationException($"The type '{type.Name}' is not registered as AutoSerializable.");
            }

            return instance;
        }
        public virtual bool TryReadAutoSerializable(object instance)
        {
            if (ReflectionProvider.TryGetSerializableProperties(instance.GetType(), out var propertiesDict))
            {
                var propertiesCount = ReadByte();
                for (var i = 0; i < propertiesCount; i++)
                {
                    var key = ReadString();
                    if (propertiesDict.TryGetValue(key, out var propertyMem))
                    {
                        propertyMem.ReadFromBinary(instance, this);
                    }
                }

                return true;
            }

            return false;
        }
        public virtual object ReadValue(Type valueType)
        {
            if (valueType is null) throw new ArgumentNullException(nameof(valueType));

            var readerMethodFound = Definitions.TryGetValue(valueType, out Func<DataReader, Type, object> action);
            if (!readerMethodFound && valueType.IsEnum)
            {
                readerMethodFound = Definitions.TryGetValue(typeof(Enum), out action);
            }

            if (readerMethodFound)
            {
                return action(this, valueType);
            }
            else
            {
                if (valueType.GetCustomAttribute<AutoSerializableAttribute>() != null) return ReadAutoSerializable(valueType);
                else if (typeof(ISerializable).IsAssignableFrom(valueType)) return ReadSerializable(valueType);
                else if (typeof(IDictionary).IsAssignableFrom(valueType)) return ReadDictionary(valueType);
                else if (typeof(IEnumerable).IsAssignableFrom(valueType)) return ReadIEnumerable(valueType);           

                return null;
            }
        }
        
        public T ReadSerializable<T>() where T : ISerializable
        {
            return (T)ReadSerializable(typeof(T));
        }
        public T ReadIEnumerable<T>() where T : IEnumerable
        {
            return (T)ReadIEnumerable(typeof(T));
        }
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            return (Dictionary<TKey, TValue>)ReadDictionary(typeof(Dictionary<TKey, TValue>));
        }
        public T ReadEnum<T>()
        {
            return (T)ReadEnum(typeof(T));
        }
        public T ReadAutoSerializable<T>()
        {
            return (T)ReadAutoSerializable(typeof(T));
        }
        public T ReadValue<T>()
        {
            return (T)ReadValue(typeof(T));
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        private void RefreshStreamLength(int length)
        {
            var mStream = _binaryReader.BaseStream as MemoryStream;

            mStream.SetLength(length);
            mStream.Capacity = length;
        }
        private object ReadIEnumerable(Type valueType)
        {
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

            if (elementType is null)
            {
                throw new NotSupportedException($"Type '{valueType}' cannot be converted to IEnumerable.");
            }

            if (valueType.IsArray)
            {
                var array = Array.CreateInstance(elementType, ReadInt());
                for (var i = 0; i < array.Length; i++)
                {
                    array.SetValue(ReadValue(elementType), i);
                }

                return array;
            }
            else
            {
                var concreteListType = typeof(List<>).MakeGenericType(elementType);
                var list = (IList)Activator.CreateInstance(concreteListType);
                var count = ReadInt();

                for (var i = 0; i < count; i++)
                {
                    list.Add(ReadValue(elementType));
                }

                return list;
            }
        }
        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _binaryReader.Dispose();

                IsDisposed = true;
            }
        }
    }
}