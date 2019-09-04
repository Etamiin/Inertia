using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public interface IReaderModel : IDisposable
    {
        bool GetBool();
        string GetString();
        string GetString(Encoding encoder);
        float GetFloat();
        decimal GetDecimal();
        double GetDouble();
        byte GetByte();
        sbyte GetSByte();
        char GetChar();
        short GetShort();
        ushort GetUShort();
        int GetInt();
        uint GetUInt();
        long GetLong();
        ulong GetULong();
        byte[] GetBytes();
        object GetValue(Type valueType);
        IList GetValues(params Type[] valuesType);
    }
}
