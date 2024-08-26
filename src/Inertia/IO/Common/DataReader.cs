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
    public sealed class DataReader : BinaryReader, IDisposable
    {
        private static Dictionary<Type, Func<DataReader, Type, object>> _readingDefinitions = new Dictionary<Type, Func<DataReader, Type, object>>
        {
            { typeof(bool), (reader, _) => reader.ReadBoolean() },
            { typeof(string), (reader, _) => reader.ReadString() },
            { typeof(float), (reader, _) => reader.ReadSingle() },
            { typeof(decimal), (reader, _) => reader.ReadDecimal() },
            { typeof(double), (reader, _) => reader.ReadDouble() },
            { typeof(byte), (reader, _) => reader.ReadByte() },
            { typeof(sbyte), (reader, _) => reader.ReadSByte() },
            { typeof(char), (reader, _) => reader.ReadChar() },
            { typeof(short), (reader, _) => reader.ReadInt16() },
            { typeof(ushort), (reader, _) => reader.ReadUInt16() },
            { typeof(int), (reader, _) => reader.ReadInt32() },
            { typeof(uint), (reader, _) => reader.ReadUInt32() },
            { typeof(long), (reader, _) => reader.ReadInt64() },
            { typeof(ulong), (reader, _) => reader.ReadUInt64() },
            { typeof(DateTime), (reader, _) => reader.ReadDateTime(DateTimeKind.Local) },
            { typeof(byte[]), (reader, _) => reader.ReadBytes() },
            { typeof(Enum), (reader, type) => reader.ReadEnum(type) },
            { typeof(ByteBits), (reader, _) => reader.ReadByteBits() }
        };

        public static void AddDeserializableType(Type type, Func<DataReader, Type, object> onDeserialize)
        {
            _readingDefinitions[type] = onDeserialize;
        }

        public bool IsDisposed { get; private set; }
        public string? EncryptionKey { get; private set; }
        public CompressionAlgorithm CompressionAlgorithm { get; private set; }
        public long TotalLength
        {
            get
            {
                return BaseStream.Length;
            }
        }
        public long UnreadedLength
        {
            get
            {
                return BaseStream.Length - BaseStream.Position;
            }
        }

        public DataReader() : this(Encoding.UTF8)
        {
        }
        public DataReader(string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(Encoding.UTF8, encryptionKey, compressionAlgorithm)
        {
        }
        public DataReader(Encoding encoding) : base(new MemoryStream(), encoding)
        {
        }
        public DataReader(Encoding encoding, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(encoding)
        {
            EncryptionKey = encryptionKey;
            CompressionAlgorithm = compressionAlgorithm;
        }
        public DataReader(byte[] data) : this(data, string.Empty, CompressionAlgorithm.None)
        {
        }
        public DataReader(byte[] data, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(data, Encoding.UTF8, encryptionKey, compressionAlgorithm)
        {
        }
        public DataReader(byte[] data, Encoding encoding) : this(data, encoding, null, CompressionAlgorithm.None)
        {
        }
        public DataReader(byte[] data, Encoding encoding, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(encoding, encryptionKey, compressionAlgorithm)
        {
            Fill(data);
        }

        public DataReader SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);
            
            if (position < 0) return this;

            BaseStream.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            return BaseStream.Position;
        }
        public void Clear()
        {
            this.ThrowIfDisposable(IsDisposed);

            BaseStream.SetLength(0);
            (BaseStream as MemoryStream).Capacity = 0;
        }

        public DataReader Fill(byte[] data)
        {
            return Fill(data, TotalLength);
        }
        public DataReader Fill(byte[] data, long offset)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!string.IsNullOrWhiteSpace(EncryptionKey))
            {
                using (var decryptResult = data.AesDecrypt(EncryptionKey))
                {
                    data = decryptResult.GetDataOrThrow();
                }
            }

            if (CompressionAlgorithm != CompressionAlgorithm.None)
            {
                var decompressionResult = CompressionAlgorithm == CompressionAlgorithm.Deflate ? data.DeflateDecompress() : data.GzipDecompress();

                using (decompressionResult)
                {
                    data = decompressionResult.GetDataOrThrow();
                }
            }

            var newLength = offset + data.Length;
            if (newLength > BaseStream.Length)
            {
                BaseStream.SetLength(newLength);
                (BaseStream as MemoryStream).Capacity = (int)newLength;
            }            

            var oldPosition = GetPosition();

            SetPosition(offset);
            BaseStream.Write(data);
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

            BaseStream.SetLength(available.Length);
            (BaseStream as MemoryStream).Capacity = available.Length;

            if (available.Length > 0)
            {
                Fill(available, 0);
                SetPosition(0);
            }

            return this;
        }

        public ByteBits ReadByteBits()
        {
            return new ByteBits(ReadByte());
        }
        public DateTime ReadDateTime(DateTimeKind kind)
        {
            return new DateTime(ReadInt64(), kind);
        }
        public byte[] ReadBytes()
        {
            return ReadBytes(ReadInt32());
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
            var array = new T[ReadInt32()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ReadValue<T>();
            }

            return array;
        }
        public IDictionary ReadDictionary(Type dictionaryType)
        {
            if (!dictionaryType.IsGenericType || !typeof(IDictionary).IsAssignableFrom(dictionaryType)) throw new ArgumentNullException(nameof(dictionaryType));

            var count = ReadInt32();
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
                var propertiesCount = ReadByte();
                for (var i = 0; i < propertiesCount; i++)
                {
                    var key = ReadString();
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

        private object ReadIEnumerable(Type valueType)
        {
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

            if (isArray)
            {
                var array = Array.CreateInstance(elementType, ReadInt32());
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
                var count = ReadInt32();

                for (var i = 0; i < count; i++)
                {
                    list.Add(ReadValue(elementType));
                }

                return list;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            base.Dispose(disposing);

            IsDisposed = true;
        }
    }
}