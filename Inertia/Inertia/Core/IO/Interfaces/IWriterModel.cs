using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public interface IWriterModel : IDisposable
    {
        IWriterModel SetEmpty(uint length);
        IWriterModel SetBool(bool value);
        IWriterModel SetString(string value);
        IWriterModel SetString(string value, Encoding encoder);
        IWriterModel SetFloat(float value);
        IWriterModel SetDecimal(decimal value);
        IWriterModel SetDouble(double value);
        IWriterModel SetByte(byte value);
        IWriterModel SetSByte(sbyte value);
        IWriterModel SetChar(char value);
        IWriterModel SetShort(short value);
        IWriterModel SetUShort(ushort value);
        IWriterModel SetInt(int value);
        IWriterModel SetUInt(uint value);
        IWriterModel SetLong(long value);
        IWriterModel SetULong(ulong value);
        IWriterModel SetBytes(byte[] value);
        IWriterModel SetValue(object value);
        IWriterModel SetValues(params object[] values);
    }
}
