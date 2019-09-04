using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class WriterBase : IWriterModel
    {
        public long CurrentPosition
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
        public long Length
        {
            get
            {
                return _writer.BaseStream.Length;
            }
        }

        private BinaryWriter _writer;

        public WriterBase()
        {
            _writer = new BinaryWriter(new MemoryStream());
        }

        public void Clear()
        {
            _writer.Dispose();
            _writer = new BinaryWriter(new MemoryStream());
        }

        public IWriterModel SetEmpty(uint size)
        {
            return SetBytes(new byte[size - 4]);
        }
        public IWriterModel SetBool(bool value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetString(string value)
        {
            return SetString(value, Encoding.UTF8);
        }
        public IWriterModel SetString(string value, Encoding encoder)
        {
            var data = encoder.GetBytes(value);
            SetBytes(data);

            return this;
        }
        public IWriterModel SetFloat(float value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetDecimal(decimal value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetDouble(double value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetByte(byte value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetSByte(sbyte value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetChar(char value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetShort(short value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetUShort(ushort value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetInt(int value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetUInt(uint value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetLong(long value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetULong(ulong value)
        {
            _writer.Write(value);
            return this;
        }
        public IWriterModel SetBytes(byte[] value)
        {
            _writer.Write(value.Length);
            _writer.Write(value);

            return this;
        }
        public IWriterModel SetValue(object value)
        {
            var valueType = value.GetType();

            if (valueType == typeof(float))
                SetFloat((float)value);
            else if (valueType == typeof(decimal))
                SetDecimal((decimal)value);
            else if (valueType == typeof(double))
                SetDouble((double)value);
            else if (valueType == typeof(short))
                SetShort((short)value);
            else if (valueType == typeof(ushort))
                SetUShort((ushort)value);
            else if (valueType == typeof(int))
                SetInt((int)value);
            else if (valueType == typeof(uint))
                SetUInt((uint)value);
            else if (valueType == typeof(string))
                SetString((string)value);
            else if (valueType == typeof(bool))
                SetBool((bool)value);
            else if (valueType == typeof(byte))
                SetByte((byte)value);
            else if (valueType == typeof(sbyte))
                SetSByte((sbyte)value);
            else if (valueType == typeof(long))
                SetLong((long)value);
            else if (valueType == typeof(ulong))
                SetULong((ulong)value);
            else if (valueType == typeof(byte[]))
                SetBytes((byte[])value);
            else if (valueType == typeof(object[]))
                SetValues((object[])value);
            else
                SetString(value.ToString());

            return this;
        }
        public IWriterModel SetValues(params object[] values)
        {
            foreach (var obj in values)
                SetValue(obj);

            return this;
        }

        public byte[] ExportAndDispose()
        {
            var data = ((MemoryStream)_writer.BaseStream).ToArray();
            Dispose();

            return data;
        }
        public byte[] ExportAndClear()
        {
            var data = ((MemoryStream)_writer.BaseStream).ToArray();
            Clear();

            return data;
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
