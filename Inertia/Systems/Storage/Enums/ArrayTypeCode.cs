using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal enum ArrayTypeCode : byte
    {
        BooleanArray = 1,
        CharArray = 2,
        SByteArray = 3,
        ByteArray = 4,
        Int16Array = 5,
        UInt16Array = 6,
        Int32Array = 7,
        UInt32Array = 8,
        Int64Array = 9,
        UInt64Array = 10,
        SingleArray = 11,
        DoubleArray = 12,
        DecimalArray = 13,
        StringArray = 14,
        SerializableArray = 15,
        ObjectArray = 16,
        InvalidType = 17
    }
}
