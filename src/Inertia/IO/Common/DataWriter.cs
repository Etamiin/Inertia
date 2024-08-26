using Inertia.IO;
using Inertia.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class DataWriter : BinaryWriter, IDisposable
    {
        private static Dictionary<Type, Action<DataWriter, object>> _writingDefinitions = new Dictionary<Type, Action<DataWriter, object>>
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
            _writingDefinitions[type] = onSerialize;
        }

        public bool IsDisposed { get; private set; }
        public CompressionAlgorithm CompressionAlgorithm { get; private set; }
        public string? EncryptionKey { get; private set; }
        public long TotalLength
        {
            get
            {
                return BaseStream.Length;
            }
        }

        public DataWriter() : this(Encoding.UTF8, 0)
        {
        }
        public DataWriter(string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(Encoding.UTF8, 0, encryptionKey, compressionAlgorithm)
        {
        }
        public DataWriter(int capacity) : this(Encoding.UTF8, capacity)
        {
        }
        public DataWriter(int capacity, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(Encoding.UTF8, capacity, encryptionKey, compressionAlgorithm)
        {
        }
        public DataWriter(Encoding encoding) : this(encoding, 0)
        {
        }
        public DataWriter(Encoding encoding, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(encoding, 0, encryptionKey, compressionAlgorithm)
        {
        }
        public DataWriter(Encoding encoding, int capacity) : base(new MemoryStream(capacity), encoding)
        {
        }
        public DataWriter(Encoding encoding, int capacity, string encryptionKey, CompressionAlgorithm compressionAlgorithm) : this(encoding, capacity)
        {
            EncryptionKey = encryptionKey;
            CompressionAlgorithm = compressionAlgorithm;
        }

        public DataWriter SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);

            BaseStream.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            return BaseStream.Position;
        }

        public DataWriter SetEmpty(int size)
        {
            if (size <= 0) throw new ArgumentNullException(nameof(size));

            base.Write(new byte[size]);
            return this;
        }
        public DataWriter Write(ByteBits value)
        {
            Write(value.Byte);

            return this;
        }
        public new DataWriter Write(bool value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            base.Write(value);
            return this;
        }
        public new DataWriter Write(float value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(decimal value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(double value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(byte value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(sbyte value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(char value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(short value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(ushort value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(int value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(uint value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(long value)
        {
            base.Write(value);
            return this;
        }
        public new DataWriter Write(ulong value)
        {
            base.Write(value);
            return this;
        }
        public DataWriter Write(DateTime value)
        {
            base.Write(value.Ticks);
            return this;
        }
        public DataWriter WriteBlock(byte[] value)
        {
            var length = value?.Length ?? 0;

            base.Write(length);
            if (length > 0)
            {
                base.Write(value);
            }

            return this;
        }
        public DataWriter Write(ISerializable value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            value.Serialize(this);
            return this;
        }
        public DataWriter Write(IEnumerable value)
        {
            if (value == null)
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
        public DataWriter Write(IDictionary value)
        {
            if (value == null)
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
        public DataWriter Write(Enum value)
        {
            var underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Write(value, underlyingType);
        }
        public DataWriter Write(object value)
        {
            if (value == null) return this;

            return Write(value, value.GetType());
        }
        public DataWriter Write(object value, Type type)
        {
            if (type == null) return this;
            
            var writerMethodFound = _writingDefinitions.TryGetValue(type, out Action<DataWriter, object> action);
            if (!writerMethodFound && type.IsEnum)
            {
                writerMethodFound = _writingDefinitions.TryGetValue(typeof(Enum), out action);
            }

            if (writerMethodFound)
            {
                action(this, value);
            }
            else
            {
                if (type.GetCustomAttribute<AutoSerializableAttribute>() != null) WriteAutoSerializable(value);
                else if (typeof(ISerializable).IsAssignableFrom(type)) Write((ISerializable)value);
                else if (typeof(IDictionary).IsAssignableFrom(type)) Write((IDictionary)value);
                else if (typeof(IEnumerable).IsAssignableFrom(type)) Write((IEnumerable)value);
            }

            return this;
        }
        public DataWriter Write(params object[] values)
        {
            if (values == null) return this;

            foreach (var obj in values)
            {
                Write(obj, obj.GetType());
            }

            return this;
        }
        public new DataWriter Write(byte[] data)
        {
            if (data == null)
            {
                data = new byte[0];
            }

            base.Write(data);
            return this;
        }
        public DataWriter WriteAutoSerializable(object value)
        {
            if (value == null) return this;
            if (ReflectionProvider.TryGetSerializableProperties(value.GetType(), out var propertiesDict))
            {
                if (propertiesDict.Count > byte.MaxValue) throw new InvalidDataException($"AutoSerializable object can't have more than {byte.MaxValue} properties.");

                byte writedCount = 0;
                var currPos = GetPosition();

                Write((byte)0);
                foreach (var pair in propertiesDict)
                {
                    if (pair.Value.WriteTo(value, this))
                    {
                        writedCount++;
                    }
                }

                SetPosition(currPos).Write(writedCount);
            }

            return this;
        }
        
        public byte[] ToArray()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DataWriter));
            }

            var binary = (BaseStream as MemoryStream).ToArray();

            if (CompressionAlgorithm == CompressionAlgorithm.Deflate)
            {
                using (var deflateResult = binary.DeflateCompress())
                {
                    binary = deflateResult.GetDataOrThrow();
                }
            }
            else if (CompressionAlgorithm == CompressionAlgorithm.GZip)
            {
                using (var gzipResult = binary.GzipCompress())
                {
                    binary = gzipResult.GetDataOrThrow();
                }
            }

            if (!string.IsNullOrWhiteSpace(EncryptionKey))
            {
                using (var encryptionResult = binary.AesEncrypt(EncryptionKey))
                {
                    binary = encryptionResult.GetDataOrThrow();
                }
            }

            return binary;
        }
        public async Task<byte[]> ToArrayAsync()
        {
            return await Task.Run(ToArray).ConfigureAwait(false);
        }
        public byte[] ToArrayAndDispose()
        {
            var data = ToArray();
            Dispose();

            return data;
        }
        public async Task<byte[]> ToArrayAndDisposeAsync()
        {
            return await Task.Run(ToArrayAndDispose).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            base.Dispose(disposing);

            IsDisposed = true;
        }
    }
}