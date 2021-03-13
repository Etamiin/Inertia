using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Inertia
{
    /// <summary>
    /// Provides methods for writing data
    /// </summary>
    public class BasicWriter : IDisposable
    {
        #region Static variables

        internal static Dictionary<Type, BasicAction<BasicWriter, object>> TypageDefinitions { get; private set; }

        #endregion

        #region Public variables

        /// <summary>
        /// Return true is the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Get the total length currently in the stream
        /// </summary>
        public long TotalLength
        {
            get
            {
                return m_writer.BaseStream.Length;
            }
        }
        /// <summary>
        /// Get or set the current position in the stream
        /// </summary>
        public long Position
        {
            get
            {
                return m_writer.BaseStream.Position;
            }
            set
            {
                m_writer.BaseStream.Position = value;
            }
        }

        #endregion

        #region Private variables

        private BinaryWriter m_writer;
        private Encoding m_encoding;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance based on <see cref="Encoding.UTF8"/> algorithm
        /// </summary>
        public BasicWriter() : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Create a new instance based on the specified <see cref="Encoding"/> algorithm
        /// </summary>
        /// <param name="encoding"></param>
        public BasicWriter(Encoding encoding)
        {
            m_encoding = encoding;
            m_writer = new BinaryWriter(new MemoryStream(), encoding);

            if (TypageDefinitions != null)
                return;

            TypageDefinitions = new Dictionary<Type, BasicAction<BasicWriter, object>>()
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
        }

        #endregion

        /// <summary>
        /// Clear the current instance and create a new empty stream
        /// </summary>
        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicWriter));

            m_writer.Dispose();
            m_writer = new BinaryWriter(new MemoryStream(), m_encoding);
        }

        /// <summary>
        /// Write an empty (0) byte array without header of specified size
        /// </summary>
        /// <param name="size">Target byte array size</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetEmpty(uint size)
        {
            return SetBytes(new byte[size - 4]);
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetBool(bool value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public BasicWriter SetBoolFlag(params bool[] values)
        {
            var length = values.Length;
            if (length > 8)
                length = 8;

            var flag = (byte)0;
            for (var i = 0; i < length; i++)
            {
                flag = values[i] ? (byte)(flag | (1 << i)) : (byte)(flag & 255 - (1 << i));
            }

            return SetByte(flag);
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetString(string value)
        {
            return SetBytes(m_encoding.GetBytes(value));
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetFloat(float value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetDecimal(decimal value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetDouble(double value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetByte(byte value)
        {
            m_writer.Write(value);
            return this;
        }

        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetSByte(sbyte value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetChar(char value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetShort(short value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetUShort(ushort value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetInt(int value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetUInt(uint value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetLong(long value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetULong(ulong value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream (with size)
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetBytes(byte[] value)
        {
            m_writer.Write(value.LongLength);
            return SetBytesWithoutHeader(value);
        }
        /// <summary>
        /// Write the specified value in the stream (without size)
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetBytesWithoutHeader(byte[] value)
        {
            m_writer.Write(value);
            return this;
        }
        /// <summary>
        /// Write the specified value in the stream (without size)
        /// </summary>
        /// <param name="value">Target value to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetDateTime(DateTime value)
        {
            return SetLong(value.Ticks);
        }
        /// <summary>
        /// Write customized serializable object in the stream
        /// </summary>
        /// <param name="obj"><see cref="ISerializableObject"/> to serialize</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetSerializableObject(ISerializableObject obj)
        {
            obj.Serialize(this);
            return this;
        }
        /// <summary>
        /// Write customized serializable objects in the stream
        /// </summary>
        /// <param name="objs"><see cref="ISerializableObject"/> array to serialize</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetSerializableObjects(IEnumerable<ISerializableObject> objs)
        {
            foreach (var obj in objs)
                obj.Serialize(this);

            return this;
        }
        /// <summary>
        /// Write the specified serializable value in the stream
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="value">The object to serialize</param>
        /// <returns></returns>
        public BasicWriter SetObject<T>(T value)
        {
            if (!value.GetType().IsSerializable)
                throw new TypeNonSerializableException<T>(value);

            var binaryFormatter = new BinaryFormatter()
            {
                TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded
            };

            binaryFormatter.Serialize(m_writer.BaseStream, value);
            return this;
        }
        /// <summary>
        /// Automatically write the specified value in the stream (if serializable)
        /// </summary>
        /// <param name="value">Serializable value</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetValue(object value)
        {
            var objType = value.GetType();
            if (TypageDefinitions.ContainsKey(objType))
                TypageDefinitions[objType](this, value);
            else
                SetObject(value);

            return this;
        }
        /// <summary>
        /// Automatically write the specified values in the stream (if serializable)
        /// </summary>
        /// <param name="values">All serializable values to write</param>
        /// <returns>Return the current instance</returns>
        public BasicWriter SetValues(params object[] values)
        {
            foreach (var obj in values)
                SetValue(obj);

            return this;
        }

        /// <summary>
        /// Export all writed data as byte array
        /// </summary>
        /// <returns>A byte array representing the data written</returns>
        public byte[] ToArray()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicWriter));

            var data = ((MemoryStream)m_writer.BaseStream).ToArray();
            return data;
        }
        /// <summary>
        /// Export all writed data as byte array and clear the current instance
        /// </summary>
        /// <returns>A byte array representing the data written</returns>
        public byte[] ToArrayAndClear()
        {
            var data = ToArray();
            Clear();

            return data;
        }
        /// <summary>
        /// Export all writed data as byte array and dispose the current instance
        /// </summary>
        /// <returns>A byte array representing the data written</returns>
        public byte[] ToArrayAndDispose()
        {
            var data = ToArray();
            Dispose();

            return data;
        }
        
        /// <summary>
        /// Dispose the reader (can't be used anymore)
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            m_writer.Flush();
            m_writer.Close();
            m_writer.Dispose();
            m_writer = null;
            m_encoding = null;
            IsDisposed = true;
        }
    }
}
