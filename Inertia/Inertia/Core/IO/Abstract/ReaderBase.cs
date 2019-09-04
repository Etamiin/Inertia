using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class ReaderBase : IReaderModel
    {
        public bool IsReaded
        {
            get
            {
                if (_reader == null)
                    return true;

                return CurrentPosition == _reader.BaseStream.Length;
            }
        }
        public long LengthAvailable
        {
            get
            {
                if (_reader == null)
                    return 0;

                return _reader.BaseStream.Length - CurrentPosition;
            }
        }
        public long CurrentPosition
        {
            get
            {
                if (_reader == null)
                    return 0;

                return _reader.BaseStream.Position;
            }
            set
            {
                _reader.BaseStream.Position = value;
            }
        }

        private BinaryReader _reader;

        public ReaderBase()
        {
        }
        public ReaderBase(byte[] data)
        {
            Fill(data);
        }

        public void Clear()
        {
            _reader.Dispose();
            _reader = null;
        }
        public IReaderModel Fill(byte[] data)
        {
            if (_reader == null)
                _reader = new BinaryReader(new MemoryStream(data));
            else {
                var position = CurrentPosition;
                var newBuffer = new byte[_reader.BaseStream.Length + data.Length];

                ((MemoryStream)_reader.BaseStream).ToArray().CopyTo(newBuffer, 0);
                data.CopyTo(newBuffer, (int)_reader.BaseStream.Length);

                _reader.Dispose();

                _reader = new BinaryReader(new MemoryStream(newBuffer));
                _reader.BaseStream.Position = position;
            }

            return this;
        }

        private bool UpdateLength(long length)
        {
            if (_reader == null)
                return false;

            return LengthAvailable >= length;
        }

        public IReaderModel SkipEmpty(long size)
        {
            CurrentPosition += size;
            return this;
        }
        public bool GetBool()
        {
            if (UpdateLength(1))
                return _reader.ReadBoolean();

            return default(bool);
        }
        public string GetString()
        {
            return GetString(Encoding.UTF8);
        }
        public string GetString(Encoding encoder)
        {
            var data = GetBytes();
            if (data.Length != 0)
                return encoder.GetString(data);
            return default(string);
        }
        public byte GetByte()
        {
            if (UpdateLength(1))
                return _reader.ReadByte();

            return default(byte);
        }
        public sbyte GetSByte()
        {
            if (UpdateLength(1))
                return _reader.ReadSByte();

            return default(sbyte);
        }
        public char GetChar()
        {
            if (UpdateLength(2))
                return _reader.ReadChar();
            return default(char);
        }
        public short GetShort()
        {
            if (UpdateLength(2))
                return _reader.ReadInt16();

            return default(short);
        }
        public ushort GetUShort()
        {
            if (UpdateLength(2))
                return _reader.ReadUInt16();

            return default(ushort);
        }
        public float GetFloat()
        {
            if (UpdateLength(4))
                return _reader.ReadSingle();

            return default(float);
        }
        public double GetDouble()
        {
            if (UpdateLength(8))
                return _reader.ReadDouble();

            return default(double);
        }
        public decimal GetDecimal()
        {
            if (UpdateLength(16))
                return _reader.ReadDecimal();

            return default(decimal);
        }
        public int GetInt()
        {
            if (UpdateLength(4))
                return _reader.ReadInt32();

            return default(int);
        }
        public uint GetUInt()
        {
            if (UpdateLength(4))
                return _reader.ReadUInt32();

            return default(uint);
        }
        public long GetLong()
        {
            if (UpdateLength(8))
                return _reader.ReadInt64();

            return default(long);
        }
        public ulong GetULong()
        {
            if (UpdateLength(8))
                return _reader.ReadUInt64();

            return default(ulong);
        }
        public byte[] GetBytes()
        {
            var count = GetInt();
            if (count != default(int) && UpdateLength(count))
                return _reader.ReadBytes(count);
            return new byte[0];
        }
        public object GetValue(Type valueType)
        {
            var value = (object)null;

            if (valueType == typeof(float))
                value = GetFloat();
            else if (valueType == typeof(decimal))
                value = GetDecimal();
            else if (valueType == typeof(double))
                value = GetDouble();
            else if (valueType == typeof(short))
                value = GetShort();
            else if (valueType == typeof(ushort))
                value = GetUShort();
            else if (valueType == typeof(int))
                value = GetInt();
            else if (valueType == typeof(uint))
                value = GetUInt();
            else if (valueType == typeof(string))
                value = GetString();
            else if (valueType == typeof(bool))
                value = GetBool();
            else if (valueType == typeof(byte))
                value = GetByte();
            else if (valueType == typeof(sbyte))
                value = GetSByte();
            else if (valueType == typeof(long))
                value = GetLong();
            else if (valueType == typeof(ulong))
                value = GetULong();
            else if (valueType == typeof(byte[]))
                value = GetBytes();
            else
                value = GetString();

            return value;
        }
        public IList GetValues(params Type[] valuesType)
        {
            var values = new object[valuesType.Length];

            var i = 0;
            foreach (var type in valuesType)
                values[i++] = GetValue(type);

            return values;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
