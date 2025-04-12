using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.IO
{
    public class DataWriter : IDisposable
    {
        protected static Dictionary<Type, Action<DataWriter, object>> Definitions { get; } = new Dictionary<Type, Action<DataWriter, object>>
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

        public static void AddDefinition<T>(Action<DataWriter, object> onSerialize)
        {
            Definitions[typeof(T)] = onSerialize;
        }

        private readonly BinaryWriter _binaryWriter;
        private readonly object _lock;

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
            _lock = new object();
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength => _binaryWriter.BaseStream.Length;
        public long Position
        {
            get
            {
                Check.ThrowsIfDisposable(this, IsDisposed);

                return _binaryWriter.BaseStream.Position;
            }
            set
            {
                Check.ThrowsIfDisposable(this, IsDisposed);

                if (value < 0 || value > TotalLength)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _binaryWriter.BaseStream.Position = value;
            }
        }

        public virtual void Lock(Action<DataWriter> onLockProcess)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);
            Check.ThrowsIfNull(onLockProcess, nameof(onLockProcess));

            lock (_lock)
            {
                onLockProcess.Invoke(this);
            }
        }
        public virtual DataWriter Clear()
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            var stream = _binaryWriter.BaseStream as MemoryStream;

            stream.SetLength(0);
            stream.Position = 0;

            return this;
        }

        public virtual DataWriter SetEmpty(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

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
        public virtual DataWriter Write(IEnumerable value)
        {
            Check.ThrowsIfNull(value, nameof(value));

            var count = 0;
            var sizePosition = Position;

            SetEmpty(sizeof(int));
            foreach (var element in value)
            {
                Write(element);
                count++;
            }

            Position = sizePosition;

            Write(count);

            Position = TotalLength;

            return this;
        }
        public virtual DataWriter Write(IDictionary value)
        {
            Check.ThrowsIfNull(value, nameof(value));

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
            Check.ThrowsIfNull(value, nameof(value));

            return Write(value, Enum.GetUnderlyingType(value.GetType()));
        }
        public virtual DataWriter Write(ISerializable value)
        {
            Check.ThrowsIfNull(value, nameof(value));

            Write(value.Version);
            value.Serialize(this);

            return this;
        }
        public virtual DataWriter Write(object value)
        {
            return Write(value, value?.GetType());
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
        public virtual DataWriter Write(object value, Type valueType)
        {
            Check.ThrowsIfNull(value, nameof(value));
            Check.ThrowsIfNull(valueType, nameof(valueType));

            if (Definitions.TryGetValue(valueType, out var action))
            {
                action(this, value);    
                return this;
            }
            else
            {
                if (typeof(ISerializable).IsAssignableFrom(valueType)) return Write((ISerializable)value);
                if (typeof(IDictionary).IsAssignableFrom(valueType)) return Write((IDictionary)value);
                if (typeof(IEnumerable).IsAssignableFrom(valueType)) return Write((IEnumerable)value);
                if (valueType.IsEnum) return Write((Enum)value);

                var nullableType = Nullable.GetUnderlyingType(valueType);
                if (nullableType != null) return Write(value, nullableType);

                if (valueType.IsClass && !valueType.IsAbstract) return WriteSerializedObject(value);
            }

            throw new NotSupportedException($"Type {valueType} cannot be serialized, consider using custom serialization definition.");
        }

        public byte[] ToArray()
        {
            return ToArray(null);
        }
        public Task<byte[]> ToArrayAsync()
        {
            return Task.Run(ToArray);
        }
        public byte[] ToArray(BinaryTransformationProcessor binaryProcessor)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            var binary = (_binaryWriter.BaseStream as MemoryStream).ToArray();

            if (binaryProcessor != null)
            {
                binary = binaryProcessor.Transform(binary);
            }

            return binary;
        }
        public Task<byte[]> ToArrayAsync(BinaryTransformationProcessor binaryProcessor)
        {
            return Task.Run(() => ToArray(binaryProcessor));
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

        private DataWriter WriteSerializedObject(object value)
        {
            var metadata = SerializationManager.GetSerializableObjectMetadata(value.GetType());

            if (metadata.Properties.Count > byte.MaxValue)
            {
                throw new InvalidDataException($"AutoSerializable object can't have more than {byte.MaxValue} properties.");
            }

            byte writedCount = 0;
            var propertiesCountPosition = Position;

            Write((byte)0);
            foreach (var pair in metadata.Properties)
            {
                var property = pair.Value;
                var propertyValue = property.GetValue(value);

                if (propertyValue != null)
                {
                    Write(pair.Key);
                    Write(propertyValue, property.PropertyType);

                    writedCount++;
                }
            }

            if (writedCount > 0)
            {
                Position = propertiesCountPosition;

                Write(writedCount);

                Position = TotalLength;
            }

            return this;
        }
    }
}