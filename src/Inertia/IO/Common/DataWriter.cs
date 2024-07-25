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
    public sealed class DataWriter : IDisposable
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
            { typeof(Enum), (writer, value) => writer.Write((Enum)value) }
        };

        public static void AddSerializableType(Type type, Action<DataWriter, object> onSerialize)
        {
            if (!_writingDefinitions.ContainsKey(type))
            {
                _writingDefinitions.Add(type, onSerialize);
            }
            else
            {
                _writingDefinitions[type] = onSerialize;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                return _stream.Length;
            }
        }

        private BinaryWriter _writer;
        private MemoryStream _stream;
        private readonly DataWriterSettings _settings;

        public DataWriter() : this(new DataWriterSettings())
        {
        }
        public DataWriter(DataWriterSettings settings)
        {
            _settings = settings;
            _stream = new MemoryStream(settings.Capacity);
            _writer = new BinaryWriter(_stream, settings.Encoding);
        }

        public DataWriter SetPosition(long position)
        {
            this.ThrowIfDisposable(IsDisposed);

            _stream.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            this.ThrowIfDisposable(IsDisposed);

            return _stream.Position;
        }

        public DataWriter SetEmpty(int size)
        {
            if (size <= 0) throw new ArgumentNullException(nameof(size));

            _writer.Write(new byte[size]);
            return this;
        }
        public DataWriter Write(bool value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(string value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(float value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(decimal value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(double value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(byte value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(sbyte value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(char value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(short value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(ushort value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(int value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(uint value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(long value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(ulong value)
        {
            _writer.Write(value);
            return this;
        }
        public DataWriter Write(DateTime value)
        {
            _writer.Write(value.Ticks);
            return this;
        }
        public DataWriter Write(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            _writer.Write(value.Length);
            if (value.Length > 0)
            {
                _writer.Write(value);
            }

            return this;
        }
        public DataWriter Write(ISerializable value)
        {
            value.Serialize(this);
            return this;
        }
        public DataWriter Write(IEnumerable value)
        {
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
            if (value == null && type != typeof(string)) return this;

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
            foreach (var obj in values)
            {
                Write(obj, obj.GetType());
            }

            return this;
        }
        public DataWriter WriteRaw(byte[] data)
        {
            _writer.Write(data);
            return this;
        }
        public DataWriter WriteAutoSerializable(object value)
        {
            if (ReflectionProvider.TryGetSerializableProperties(value.GetType(), out var propertiesDict))
            {
                if (propertiesDict.Count > byte.MaxValue) throw new InvalidDataException($"AutoSerializable object can't have more than {byte.MaxValue} properties.");

                Write((byte)propertiesDict.Count);
                foreach (var pair in propertiesDict)
                {
                    pair.Value.WriteTo(value, this);
                }
            }

            return this;
        }
        
        public byte[] ToArray()
        {
            if (IsDisposed || _stream == null)
            {
                throw new ObjectDisposedException(nameof(DataWriter));
            }

            var binary = _stream.ToArray();

            if (_settings.CompressionAlgorithm == CompressionAlgorithm.Deflate)
            {
                using (var deflateResult = binary.DeflateCompress())
                {
                    binary = deflateResult.GetDataOrThrow();
                }
            }
            else if (_settings.CompressionAlgorithm == CompressionAlgorithm.GZip)
            {
                using (var gzipResult = binary.GzipCompress())
                {
                    binary = gzipResult.GetDataOrThrow();
                }
            }

            if (!string.IsNullOrWhiteSpace(_settings.EncryptionKey))
            {
                using (var encryptionResult = binary.AesEncrypt(_settings.EncryptionKey))
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

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _writer.Dispose();
                _writer = null;
                _stream = null;
            }

            IsDisposed = true;
        }
    }
}