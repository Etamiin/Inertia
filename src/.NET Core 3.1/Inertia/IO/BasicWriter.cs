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

        public static void SetTypeSerialization(Type type, BasicAction<BasicWriter, object> serialization)
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
        /// <summary>
        /// Returns the total length of the stream.
        /// </summary>
        public long TotalLength
        {
            get
            {
                return _writer.BaseStream.Length;
            }
        }
        /// <summary>
        /// Get or Set the position in the stream.
        /// </summary>
        public long Position
        {
            get
            {
                if (_writer != null)
                {
                    return _writer.BaseStream.Position;
                }
                else
                { 
                    return 0; 
                }
            }
            set
            {
                if (_writer != null)
                {
                    _writer.BaseStream.Position = value;
                }                
            }
        }

        private BinaryWriter _writer;
        private readonly Encoding _encoding;

        /// <summary>
        /// Initialize a new instance based on <see cref="Encoding.UTF8"/> algorithm
        /// </summary>
        public BasicWriter() : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Initialize a new instance based on the specified <see cref="Encoding"/> algorithm
        /// </summary>
        /// <param name="encoding"></param>
        public BasicWriter(Encoding encoding)
        {
            _encoding = encoding;
            _writer = new BinaryWriter(new MemoryStream(), encoding);
        }

        public void Clear()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }

            if (_writer != null)
            {
                _writer.Dispose();
            }

            _writer = new BinaryWriter(new MemoryStream(), _encoding);
        }

        /// <summary>
        /// Write empty data of specified size
        /// </summary>
        /// <param name="size">Target byte array size</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetEmpty(uint size)
        {
            return SetBytesWithoutHeader(new byte[size]);
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetBool(bool value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write a bool flag
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="BoolFlagTooLargeException"></exception>
        public BasicWriter SetBoolFlag(params bool[] values)
        {
            return SetByte(values.CreateFlag());
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return SetBytes(new byte[] { });
            }
            else
            {
                return SetBytes(_encoding.GetBytes(value));
            }
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetFloat(float value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetDecimal(decimal value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetDouble(double value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetByte(byte value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetSByte(sbyte value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetChar(char value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetShort(short value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetUShort(ushort value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetInt(int value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetUInt(uint value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetLong(long value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetULong(ulong value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetBytes(byte[] value)
        {
            _writer.Write((uint)value.Length);
            return SetBytesWithoutHeader(value);
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetBytesWithoutHeader(byte[] value)
        {
            _writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetDateTime(DateTime value)
        {
            return SetLong(value.Ticks);
        }
        /// <summary>
        /// Write an instance of <see cref="ISerializableObject"/> in the stream
        /// </summary>
        /// <param name="value"><see cref="ISerializableObject"/> to serialize</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetSerializableObject(ISerializableObject value)
        {
            value.Serialize(this);
            return this;
        }
        /// <summary>
        /// Write an instance of <see cref="IAutoSerializable"/> in the stream
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BasicWriter SetAutoSerializable(IAutoSerializable value)
        {
            var type = value.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy((x) => x.MetadataToken);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreField>() == null)
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

        /// <summary>
        /// Automatically write the specified value in the stream
        /// </summary>
        /// <param name="value">Serializable value</param>
        /// <returns>Returns the current instance</returns>
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
        /// <summary>
        /// Automatically write the specified values in the stream
        /// </summary>
        /// <param name="values">Serializable values to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetValues(params object[] values)
        {
            foreach (var obj in values)
            {
                SetValue(obj);
            }

            return this;
        }

        /// <summary>
        /// Export all writed data as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            if (!IsDisposed && _writer != null)
            {
                return ((MemoryStream)_writer.BaseStream).ToArray();
            }
            else
            {
                throw new ObjectDisposedException(nameof(BasicWriter));
            }
        }
        /// <summary>
        /// Export all writed data as byte array and clear the current instance's data
        /// </summary>
        /// <returns></returns>
        public byte[] ToArrayAndClear()
        {
            var data = ToArray();
            Clear();

            return data;
        }
        /// <summary>
        /// Export all writed data as byte array and dispose the current instance
        /// </summary>
        /// <returns></returns>
        public byte[] ToArrayAndDispose()
        {
            var data = ToArray();
            Dispose();

            return data;
        }

        public void Dispose()
        {
            Dispose(true);
        }        
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                if (disposing)
                {
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}