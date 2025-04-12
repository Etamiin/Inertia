using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Inertia.IO
{
    public class DataReader : IDisposable
    {
        private static Dictionary<Type, Func<DataReader, Type, object>> _definitions { get; } = new Dictionary<Type, Func<DataReader, Type, object>>
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
            { typeof(byte[]), (reader, _) => reader.ReadBytesWithHeader() },
            { typeof(ByteBits), (reader, _) => reader.ReadByteBits() }
        };

        public static void AddDefinition<T>(Func<DataReader, Type, object> onDeserialize)
        {
            _definitions[typeof(T)] = onDeserialize;
        }

        private BinaryReader _binaryReader;
        private object _lock;

        public DataReader() : this(Encoding.UTF8)
        {
        }
        public DataReader(Encoding encoding)
        {
            _binaryReader = new BinaryReader(new MemoryStream(), encoding);
            _lock = new object();
        }
        public DataReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
        public DataReader(byte[] data, BinaryTransformationProcessor binaryProcessor) : this(data, Encoding.UTF8, binaryProcessor)
        {
        }
        public DataReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data);
        }
        public DataReader(byte[] data, Encoding encoding, BinaryTransformationProcessor binaryProcessor) : this(encoding)
        {
            Check.ThrowsIfNull(data, nameof(data));
            Check.ThrowsIfNull(binaryProcessor, nameof(binaryProcessor));

            Fill(data, binaryProcessor);
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength => _binaryReader.BaseStream.Length;
        public long UnreadedLength => _binaryReader.BaseStream.Length - _binaryReader.BaseStream.Position;
        public long Position
        {
            get
            {
                Check.ThrowsIfDisposable(this, IsDisposed);

                return _binaryReader.BaseStream.Position;
            }
            set
            {
                Check.ThrowsIfDisposable(this, IsDisposed);

                if (value < 0 || value > TotalLength)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _binaryReader.BaseStream.Position = value;
            }
        }

        public virtual DataReader Fill(byte[] data)
        {
            return Fill(data, data.Length);
        }
        public virtual DataReader Fill(byte[] data, BinaryTransformationProcessor binaryProcessor)
        {
            Check.ThrowsIfNull(binaryProcessor, nameof(binaryProcessor));

            data = binaryProcessor.Revert(data);

            return Fill(data, data.Length);
        }
        public virtual DataReader Fill(byte[] buffer, int length)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Length cannot be greater than buffer length.");
            }
            
            var beforeFillPosition = Position;
            var newLength = TotalLength + length;

            if (newLength > _binaryReader.BaseStream.Length)
            {
                RefreshStreamLength(newLength);
            }

            _binaryReader.BaseStream.Write(buffer, 0, length);
            Position = beforeFillPosition;

            return this;
        }
        public virtual DataReader Skip(int length)
        {
            Position += length;

            return this;
        }
        public virtual DataReader RemoveReadedBytes()
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            var buffer = (_binaryReader.BaseStream as MemoryStream).GetBuffer();
            var availableBytes = (int)UnreadedLength;
            var position = (int)Position;

            if (availableBytes > 0 && position > 0)
            {
                Buffer.BlockCopy(buffer, position, buffer, 0, availableBytes);
                Position = 0;
            }

            RefreshStreamLength(availableBytes);

            return this;
        }
        public virtual DataReader Clear()
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            RefreshStreamLength(0);

            return this;
        }
        public virtual void Lock(Action<DataReader> onLockProcess)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);
            Check.ThrowsIfNull(onLockProcess, nameof(onLockProcess));

            lock (_lock)
            {
                onLockProcess.Invoke(this);
            }
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
        public virtual byte[] ReadBytesWithHeader() => ReadBytes(_binaryReader.ReadInt32());
        public virtual byte[] ReadBytes(int count) => _binaryReader.ReadBytes(count);
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

            return ReadValue(Enum.GetUnderlyingType(enumType));
        }
        public virtual ISerializable ReadSerializable(Type type)
        {
            if (!typeof(ISerializable).IsAssignableFrom(type))
            {
                throw new InvalidCastException($"The type '{type.Name}' has no '{nameof(ISerializable)}' interface assigned.");
            }

            var instance = (ISerializable)Activator.CreateInstance(type);
            instance.Deserialize(ReadByte(), this);

            return instance;
        }        
        public virtual object ReadValue(Type valueType)
        {
            Check.ThrowsIfNull(valueType, nameof(valueType));

            if (_definitions.TryGetValue(valueType, out var action)) return action(this, valueType);
            if (typeof(ISerializable).IsAssignableFrom(valueType)) return ReadSerializable(valueType);
            if (typeof(IDictionary).IsAssignableFrom(valueType)) return ReadDictionary(valueType);
            if (typeof(IEnumerable).IsAssignableFrom(valueType)) return ReadIEnumerable(valueType);
            if (valueType.IsEnum) return ReadEnum(valueType);

            var nullableType = Nullable.GetUnderlyingType(valueType);
            if (nullableType != null) return ReadValue(nullableType);

            if (valueType.IsClass && !valueType.IsAbstract) return ReadSerializedObject(valueType);

            throw new NotSupportedException($"Type '{valueType}' cannot be deserialized, consider using custom serialization definition.");
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
        public T ReadValue<T>()
        {
            return (T)ReadValue(typeof(T));
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        private void RefreshStreamLength(long length)
        {
            var mStream = _binaryReader.BaseStream as MemoryStream;

            mStream.SetLength(length);
            mStream.Capacity = (int)length;
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
        private object ReadSerializedObject(Type type)
        {
            var metadata = SerializationManager.GetSerializableObjectMetadata(type);
            var instance = Activator.CreateInstance(type);
            var propertiesCount = ReadByte();

            for (var i = 0; i < propertiesCount; i++)
            {
                var name = ReadString();

                if (metadata.Properties.TryGetValue(name, out var propertyMetadata))
                {
                    propertyMetadata.SetValue(instance, ReadValue(propertyMetadata.PropertyType));
                }
                else
                {
                    throw new KeyNotFoundException($"The property '{name}' was not found in the type '{type.Name}'.");
                }
            }

            return instance;
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