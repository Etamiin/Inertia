using Inertia.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class DataWriter : IDisposable
    {
        protected static Dictionary<Type, Action<DataWriter, object>> Definitions { get; private set; } = new Dictionary<Type, Action<DataWriter, object>>
        {
            { typeof(bool), (writer, value) => writer.Write((bool)value) },
            { typeof(string), (writer, value) => writer.Write((string)value) },
            { typeof(float), (writer, value) => writer.Write((float)value) },
            { typeof(decimal), (writer, value) => writer.Write((decimal)value) },
            { typeof(double), (writer, value) => writer.Write((double)value) },
            { typeof(byte), (writer, value) => writer.Write((byte)value) },
            { typeof(sbyte), (writer, value) => writer.Write((sbyte)value) },
            { typeof(char), (writer, value) => writer.Write((char)value) },
            { typeof(short), (writer, value) => writer.Write((short)value) },
            { typeof(ushort), (writer, value) => writer.Write((ushort)value) },
            { typeof(int), (writer, value) => writer.Write((int)value) },
            { typeof(uint), (writer, value) => writer.Write((uint)value) },
            { typeof(long), (writer, value) => writer.Write((long)value) },
            { typeof(ulong), (writer, value) => writer.Write((ulong)value) },
            { typeof(DateTime), (writer, value) => writer.Write((DateTime)value) },
            { typeof(byte[]), (writer, value) => writer.Write((byte[])value) },
            { typeof(Enum), (writer, value) => writer.Write((Enum)value) },
            { typeof(ByteBits), (writer, value) => writer.Write((ByteBits)value) }
        };

        public static void AddSerializableType(Type type, Action<DataWriter, object> onSerialize)
        {
            Definitions[type] = onSerialize;
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength => _binaryWriter.BaseStream.Length;

        private readonly BinaryWriter _binaryWriter;

        public DataWriter() : this(Encoding.UTF8, 0)
        {
        }
        public DataWriter(int capacity) : this(Encoding.UTF8, capacity)
        {
        }
        public DataWriter(Encoding encoding) : this(encoding, 0)
        {
        }
        public DataWriter(Encoding encoding, int capacity)
        {
            _binaryWriter = new BinaryWriter(new MemoryStream(capacity), encoding);
        }

        public DataWriter SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);

            _binaryWriter.BaseStream.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            return _binaryWriter.BaseStream.Position;
        }

        public virtual DataWriter SetEmpty(int size)
        {
            if (size <= 0) throw new ArgumentNullException(nameof(size));

            _binaryWriter.Write(new byte[size]);
            return this;
        }
        public virtual DataWriter Write(bool value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(string value)
        {
            _binaryWriter.Write(value ?? string.Empty);
            return this;
        }
        public virtual DataWriter Write(float value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(double value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(decimal value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(byte value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(sbyte value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(char value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(short value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(ushort value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(int value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(uint value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(long value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(ulong value)
        {
            _binaryWriter.Write(value);
            return this;
        }
        public virtual DataWriter Write(DateTime value)
        {
            _binaryWriter.Write(value.Ticks);
            return this;
        }
        public virtual DataWriter Write(byte[] data)
        {
            _binaryWriter.Write(data ?? new byte[0]);
            return this;
        }
        public virtual DataWriter Write(ISerializable value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            value.Serialize(this);
            return this;
        }
        public virtual DataWriter Write(IEnumerable value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var count = 0;
            var sizePos = GetPosition();

            SetEmpty(sizeof(int));
            foreach (var v in value)
            {
                Write(v);
                count++;
            }

            return SetPosition(sizePos)
                .Write(count)
                .SetPosition(TotalLength);
        }
        public virtual DataWriter Write(IDictionary value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Write(value.Count);

            foreach (DictionaryEntry entry in value)
            {
                Write(entry.Key);
                Write(entry.Value);
            }

            return this;
        }
        public virtual DataWriter Write(Enum value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Write(value, underlyingType);
        }
        public virtual DataWriter Write(object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Write(value, value?.GetType());
        }
        public virtual DataWriter Write(object value, Type type)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            var writerMethodFound = Definitions.TryGetValue(type, out Action<DataWriter, object> action);
            if (!writerMethodFound && type.IsEnum)
            {
                writerMethodFound = Definitions.TryGetValue(typeof(Enum), out action);
            }

            if (writerMethodFound)
            {
                action(this, value);
                
                return this;
            }
            else
            {
                if (type.GetCustomAttribute<AutoSerializableAttribute>() != null) return WriteAutoSerializable(value);
                if (typeof(ISerializable).IsAssignableFrom(type)) return Write((ISerializable)value);
                if (typeof(IDictionary).IsAssignableFrom(type)) return Write((IDictionary)value);
                if (typeof(IEnumerable).IsAssignableFrom(type)) return Write((IEnumerable)value);
            }

            throw new NotSupportedException($"Type '{type.Name}' not supported, you can build a custom serializer for this type by using '{nameof(AddSerializableType)}' method.");
        }
        public virtual DataWriter Write(params object[] values)
        {
            if (values is null || values.Length == 0) return this;
            
            foreach (var obj in values)
            {
                Write(obj, obj?.GetType());
            }

            return this;
        }
        public virtual DataWriter WriteWithHeader(byte[] value)
        {
            var length = value?.Length ?? 0;

            _binaryWriter.Write(length);
            if (length > 0)
            {
                _binaryWriter.Write(value);
            }

            return this;
        }
        public virtual DataWriter WriteAutoSerializable(object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ReflectionProvider.TryGetSerializableProperties(value.GetType(), out var propertiesDict))
            {
                if (propertiesDict.Count > byte.MaxValue) throw new InvalidDataException($"AutoSerializable object can't have more than {byte.MaxValue} properties.");

                byte writedCount = 0;
                var countPosition = GetPosition();

                Write((byte)0);
                foreach (var pair in propertiesDict)
                {
                    if (pair.Value.TryWriteInBinary(value, this))
                    {
                        writedCount++;
                    }
                }

                if (writedCount > 0)
                {
                    SetPosition(countPosition);
                    Write(writedCount);

                    return SetPosition(TotalLength);
                }

                return this;
            }

            throw new InvalidDataException($"AutoSerializable type '{value.GetType().Name}' don't have any valid properties to serialize.");
        }
        
        public byte[] ToArray()
        {
            return ToArray(null, CompressionAlgorithm.None);
        }
        public byte[] ToArray(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DataWriter));
            }

            var binary = (_binaryWriter.BaseStream as MemoryStream).ToArray();

            if (compressionAlgorithm != CompressionAlgorithm.None)
            {
                using (var compressionResult = binary.Compress(compressionAlgorithm))
                {
                    binary = compressionResult.GetDataOrThrow();
                }
            }

            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {
                using (var encryptionResult = binary.AesEncrypt(encryptionKey))
                {
                    binary = encryptionResult.GetDataOrThrow();
                }
            }

            return binary;
        }
        public Task<byte[]> ToArrayAsync()
        {
            return Task.Run(ToArray);
        }
        public Task<byte[]> ToArrayAsync(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            return Task.Run(() => ToArray(encryptionKey, compressionAlgorithm));
        }
        public byte[] ToArrayAndDispose()
        {
            return ToArrayAndDispose(null, CompressionAlgorithm.None);
        }
        public byte[] ToArrayAndDispose(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            var data = ToArray(encryptionKey, compressionAlgorithm);
            Dispose();

            return data;
        }
        public Task<byte[]> ToArrayAndDisposeAsync()
        {
            return Task.Run(ToArrayAndDispose);
        }
        public Task<byte[]> ToArrayAndDisposeAsync(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            return Task.Run(() => ToArrayAndDispose(encryptionKey, compressionAlgorithm));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _binaryWriter.Dispose();

                IsDisposed = true;
            }
        }
    }
}