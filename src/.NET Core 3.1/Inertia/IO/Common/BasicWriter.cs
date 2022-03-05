using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia
{
    public class BasicWriter : IDisposable
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
            { typeof(byte[]), (writer, value) => writer.SetBytes((byte[])value) }
        };

        public static void SetSerializableType(Type type, BasicAction<BasicWriter, object> serialization)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, serialization);
            }
            else
            {
                _typageDefinitions[type] = serialization;
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
            if (_writer != null)
            {
                _writer.Position = position;
            }

            return this;
        }
        public long GetPosition()
        {
            if (_writer != null) return _writer.Position;

            return 0;
        }

        public BasicWriter SetEmpty(int size)
        {
            return SetBytesWithoutHeader(new byte[size]);
        }
        public BasicWriter SetBool(bool value)
        {
            _writer.WriteByte(Convert.ToByte(value));
            return this;
        }
        public BasicWriter SetBoolFlag(params bool[] values)
        {
            return SetByte(values.CreateFlag());
        }
        public BasicWriter SetString(string value)
        {
            return SetBytes(!string.IsNullOrEmpty(value) ? _encoding.GetBytes(value) : new byte[0]);
        }
        public BasicWriter SetFloat(float value)
        {
            _writer.Write(BitConverter.GetBytes(value));
            return this;
        }
        public BasicWriter SetDecimal(decimal value)
        {
            return SetDouble((double)value);
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
            return SetLong(value.Ticks);
        }
        public BasicWriter SetBytes(byte[] value)
        {
            _writer.Write(BitConverter.GetBytes(value.Length));
            return SetBytesWithoutHeader(value);
        }
        public BasicWriter SetBytesWithoutHeader(byte[] value)
        {
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
            var type = value.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy((x) => x.MetadataToken);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreInProcess>() == null)
                {
                    var fieldValue = field.GetValue(value);

                    if (!typeof(IAutoSerializable).IsAssignableFrom(field.FieldType))
                    {
                        var customization = field.GetCustomAttribute<CustomSerialization>();
                        if (customization == null)
                        {
                            SetValue(fieldValue);
                        }
                        else
                        {
                            var method = type.GetMethod(customization.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                            var parameters = method?.GetParameters();

                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(BasicWriter))
                            {
                                method.Invoke(value, new object[] { this });
                            }
                        }
                    }
                    else
                    {
                        if (fieldValue == null)
                        {
                            throw new MissingFieldException($"{ field.Name } is null");
                        } 
                        
                        SetAutoSerializable(fieldValue as IAutoSerializable);
                    }
                }
            }

            return this;
        }

        public BasicWriter SetValue(object value)
        {
            var objType = value.GetType();
            if (_typageDefinitions.ContainsKey(objType))
            {
                _typageDefinitions[objType](this, value);
            }
            else
            {
                if (objType.GetInterface(nameof(IAutoSerializable)) != null)
                {
                    SetAutoSerializable((IAutoSerializable)value);
                }
                else if (objType.GetInterface(nameof(ISerializableObject)) != null)
                {
                    SetSerializableObject((ISerializableObject)value);
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
            if (!IsDisposed && _writer != null)
            {
                return _writer.ToArray();
            }
            else
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }
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