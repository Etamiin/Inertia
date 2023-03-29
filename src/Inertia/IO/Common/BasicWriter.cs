using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Inertia
{
    public sealed class BasicWriter : IDisposable
    {
        private static Dictionary<Type, BasicAction<BasicWriter, object>> _typageDefinitions = new Dictionary<Type, BasicAction<BasicWriter, object>>
        {
            { typeof(bool), (writer, value) => writer.SetBool((bool)value) },
            { typeof(string), (writer, value) => writer.SetString((string)value) },
            { typeof(float), (writer, value) => writer.SetFloat((float)value) },
            { typeof(decimal), (writer, value) => writer.SetDecimal((decimal)value) },
            { typeof(double), (writer, value) => writer.SetDouble((double)value) },
            { typeof(byte), (writer, value) => writer.SetByte((byte)value) },
            { typeof(sbyte), (writer, value) => writer.SetSByte((sbyte)value) },
            { typeof(char), (writer, value) => writer.SetChar((char)value) },
            { typeof(short), (writer, value) => writer.SetShort((short)value) },
            { typeof(ushort), (writer, value) => writer.SetUShort((ushort)value) },
            { typeof(int), (writer, value) => writer.SetInt((int)value) },
            { typeof(uint), (writer, value) => writer.SetUInt((uint)value) },
            { typeof(long), (writer, value) => writer.SetLong((long)value) },
            { typeof(ulong), (writer, value) => writer.SetULong((ulong)value) },
            { typeof(DateTime), (writer, value) => writer.SetDateTime((DateTime)value) },
            { typeof(byte[]), (writer, value) => writer.SetBytes((byte[])value) }
        };

        public static void AddSerializableType(Type type, BasicAction<BasicWriter, object> onSerialize)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, onSerialize);
            }
            else
            {
                _typageDefinitions[type] = onSerialize;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                return _writer.Length;
            }
        }
        
        private MemoryStream _writer;
        private readonly Encoding _encoding;
        
        public BasicWriter() : this(Encoding.UTF8)
        {
        }
        public BasicWriter(int size) : this(size, Encoding.UTF8)
        {
        }
        public BasicWriter(Encoding encoding) : this(256, encoding)
        {
        }
        public BasicWriter(int size, Encoding encoding)
        {
            _encoding = encoding;
            _writer = new MemoryStream(size);
        }

        public BasicWriter SetPosition(long position)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }

            _writer.Position = Math.Min(position, TotalLength);
            return this;
        }
        public long GetPosition()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }

            return _writer.Position;
        }

        public BasicWriter SetEmpty(int size)
        {
            if (size <= 0) throw new ArgumentNullException(nameof(size));
            return SetBytesWithoutHeader(new byte[size]);
        }
        public BasicWriter SetBool(bool value)
        {
            _writer.WriteByte(Convert.ToByte(value));
            return this;
        }
        public BasicWriter SetString(string value)
        {
            return SetBytes(!string.IsNullOrWhiteSpace(value) ? _encoding.GetBytes(value) : new byte[0]);
        }
        public BasicWriter SetFloat(float value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetDecimal(decimal value)
        {
            var bits = decimal.GetBits(value);
            for (var i = 0; i < bits.Length; i++)
            {
                _writer.Write(BitConverter.GetBytes(bits[i]));
            }

            return this;
        }
        public BasicWriter SetDouble(double value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetByte(byte value)
        {
            _writer.WriteByte(value);
            return this;
        }
        public BasicWriter SetSByte(sbyte value)
        {
            _writer.WriteByte(unchecked((byte)value));
            return this;
        }
        public BasicWriter SetChar(char value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetShort(short value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetUShort(ushort value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetInt(int value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetUInt(uint value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetLong(long value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetULong(ulong value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetDateTime(DateTime value)
        {
            if (value == null)
            {
                value = DateTime.Now;
            }

            return SetLong(value.Ticks);
        }
        public BasicWriter SetBytes(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _writer.Write(BitConverter.GetBytes(value.Length));
            _writer.Write(value);

            return this;
        }
        public BasicWriter SetBytesWithoutHeader(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _writer.Write(value);
            return this;
        }
        public BasicWriter SetSerializableObject(ISerializableObject value)
        {
            value.Serialize(this);
            return this;
        }
        public BasicWriter SetAutoSerializable(IAutoSerializable value)
        {
            if (ReflectionProvider.TryGetProperties(value.GetType(), out ReflectionProvider.SerializablePropertyMemory[] properties))
            {
                foreach (var property in properties)
                {
                    property.Write(value, this);
                }
            }

            return this;
        }
        public BasicWriter SetArray(Array array)
        {
            SetInt(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                SetValue(array.GetValue(i));
            }

            return this;
        }
        public BasicWriter SetIDictionary(IDictionary dictionary)
        {
            SetInt(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
            {
                SetValue(entry.Key);
                SetValue(entry.Value);
            }

            return this;
        }

        public BasicWriter SetValue(object value, Type precisedType = null)
        {
            if (value == null) return this;
            if (precisedType == null)
            {
                precisedType = value.GetType();
            }

            if (_typageDefinitions.TryGetValue(precisedType, out BasicAction<BasicWriter, object> action))
            {
                action(this, value);
            }
            else
            {
                if (precisedType.GetInterface(nameof(IAutoSerializable)) != null)
                {
                    SetAutoSerializable((IAutoSerializable)value);
                }
                else if (precisedType.GetInterface(nameof(ISerializableObject)) != null)
                {
                    SetSerializableObject((ISerializableObject)value);
                }
                else if (precisedType.IsArray)
                {
                    SetArray((Array)value);
                }
                else if (precisedType.GetInterface(nameof(IDictionary)) != null)
                {
                    SetIDictionary((IDictionary)value);
                }
            }

            return this;
        }        
        public BasicWriter SetValues(params object[] values)
        {
            foreach (var obj in values)
            {
                SetValue(obj);
            }

            return this;
        }
        
        public byte[] ToArray()
        {
            if (IsDisposed || _writer == null)
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }

            return _writer.ToArray();
        }
        public byte[] ToArrayAndDispose()
        {
            var data = ToArray();
            Dispose();

            return data;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();

                IsDisposed = true;
            }
        }
    }
}