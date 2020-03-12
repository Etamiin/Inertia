using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class InertiaWriter
    {
        #region Public variables

        public bool IsDisposed { get; private set; }
        public long Position
        {
            get
            {
                return Writer.BaseStream.Position;
            }
            set
            {
                Writer.BaseStream.Position = value;
            }
        }
        public long TotalLength
        {
            get
            {
                return Writer.BaseStream.Length;
            }
        }

        #endregion

        #region Private variables

        private BinaryWriter Writer;

        #endregion

        #region Constructors

        public InertiaWriter()
        {
            Writer = new BinaryWriter(new MemoryStream(), InertiaConfiguration.BaseEncodage);
        }

        #endregion

        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaWriter));

            Writer.Dispose();
            Writer = new BinaryWriter(new MemoryStream(), InertiaConfiguration.BaseEncodage);
        }

        public InertiaWriter SetEmpty(uint size)
        {
            return SetBytes(new byte[size - 4]);
        }
        public InertiaWriter SetBool(bool value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetString(string value)
        {
            return SetString(value, InertiaConfiguration.BaseEncodage);
        }
        public InertiaWriter SetString(string value, Encoding encodage)
        {
            if (string.IsNullOrEmpty(value)) {
                SetBytes(new byte[] { });
                return this;
            }

            SetBytes(encodage.GetBytes(value));
            return this;
        }
        public InertiaWriter SetFloat(float value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetDecimal(decimal value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetDouble(double value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetByte(byte value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetSByte(sbyte value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetChar(char value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetShort(short value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetUShort(ushort value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetInt(int value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetUInt(uint value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetLong(long value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetULong(ulong value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetBytes(byte[] value)
        {
            Writer.Write(value.Length);
            return SetBytesWithoutHeader(value);
        }
        public InertiaWriter SetBytesWithoutHeader(byte[] value)
        {
            Writer.Write(value);
            return this;
        }
        public InertiaWriter SetSerializableObject(ISerializableObject serializableObj)
        {
            var objWriter = new InertiaWriter();
            serializableObj.Serialize(objWriter);

            return SetBytes(objWriter.ExportAndDispose());
        }
        public InertiaWriter SetValue(object value)
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
            else if (valueType.GetInterface(nameof(ISerializableObject)) != null)
                SetSerializableObject((ISerializableObject)value);
            else if (valueType == typeof(object[]))
                SetValues((object[])value);
            else
                SetString(value.ToString());

            return this;
        }
        public InertiaWriter SetValues(params object[] values)
        {
            foreach (var obj in values)
                SetValue(obj);

            return this;
        }

        public byte[] Export()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InertiaWriter));

            var data = ((MemoryStream)Writer.BaseStream).ToArray();
            return data;
        }
        public byte[] ExportAndClear()
        {
            var data = Export();
            Clear();

            return data;
        }
        public byte[] ExportAndDispose()
        {
            var data = Export();
            Dispose();

            return data;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
            Writer = null;
            IsDisposed = true;
        }
    }
}
