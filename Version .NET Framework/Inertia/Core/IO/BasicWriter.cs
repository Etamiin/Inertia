﻿using System;
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
        private static Dictionary<Type, BasicAction<BasicWriter, object>> _typageDefinitions;

        /// <summary>
        /// Return true is the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Total length of the stream
        /// </summary>
        public long TotalLength
        {
            get
            {
                return _writer.BaseStream.Length;
            }
        }
        /// <summary>
        /// Get or set the current position in the stream
        /// </summary>
        public long Position
        {
            get
            {
                return _writer.BaseStream.Position;
            }
            set
            {
                _writer.BaseStream.Position = value;
            }
        }

        private BinaryWriter _writer;
        private Encoding _encoding;

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

            if (_typageDefinitions != null)
                return;

            _typageDefinitions = new Dictionary<Type, BasicAction<BasicWriter, object>>()
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

        /// <summary>
        /// Clear the current instance data
        /// </summary>
        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicWriter));

            _writer.Dispose();
            _writer = new BinaryWriter(new MemoryStream(), _encoding);
        }

        /// <summary>
        /// Write empty data of specified size
        /// </summary>
        /// <param name="size">Target byte array size</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetEmpty(uint size)
        {
            return SetBytes(new byte[size - 4]);
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
            var length = values.Length;
            if (length > 8)
                throw new BoolFlagTooLargeException();

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
        /// <param name="value">Value to write</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetString(string value)
        {
            return SetBytes(_encoding.GetBytes(value));
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
            _writer.Write(value.LongLength);
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
        /// Write customized serializable object in the stream
        /// </summary>
        /// <param name="obj"><see cref="ISerializableObject"/> to serialize</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetSerializableObject(ISerializableObject obj)
        {
            obj.Serialize(this);
            return this;
        }
        /// <summary>
        /// Write customized serializable objects in the stream
        /// </summary>
        /// <param name="objs"><see cref="ISerializableObject"/> array to serialize</param>
        /// <returns>Returns the current instance</returns>
        public BasicWriter SetSerializableObjects(IEnumerable<ISerializableObject> objs)
        {
            foreach (var obj in objs)
                obj.Serialize(this);

            return this;
        }
        /// <summary>
        /// Write the specified serializable value in the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The object to serialize</param>
        /// <returns></returns>
        /// <exception cref="TypeNonSerializableException"></exception>
        public BasicWriter SetObject<T>(T value)
        {
            if (!value.GetType().IsSerializable)
                throw new TypeNonSerializableException(typeof(T));

            var binaryFormatter = new BinaryFormatter()
            {
                TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded
            };

            binaryFormatter.Serialize(_writer.BaseStream, value);
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
                _typageDefinitions[objType](this, value);
            else
                SetObject(value);

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
                SetValue(obj);

            return this;
        }

        /// <summary>
        /// Export all writed data as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicWriter));

            var data = ((MemoryStream)_writer.BaseStream).ToArray();
            return data;
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
        
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
            _writer = null;
            _encoding = null;
            IsDisposed = true;
        }
    }
}