using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia
{
    public static class InertiaExtensions
    {
        #region Storage Extensions
        
        internal static TypeCode ToTypeCode(this Type type)
        {
            if (type == typeof(bool))
                return TypeCode.Boolean;
            else if (type == typeof(char))
                return TypeCode.Char;
            else if (type == typeof(sbyte))
                return TypeCode.SByte;
            else if (type == typeof(byte))
                return TypeCode.Byte;
            else if (type == typeof(short))
                return TypeCode.Int16;
            else if (type == typeof(ushort))
                return TypeCode.UInt16;
            else if (type == typeof(int))
                return TypeCode.Int32;
            else if (type == typeof(uint))
                return TypeCode.UInt32;
            else if (type == typeof(long))
                return TypeCode.Int64;
            else if (type == typeof(ulong))
                return TypeCode.UInt64;
            else if (type == typeof(float))
                return TypeCode.Single;
            else if (type == typeof(double))
                return TypeCode.Double;
            else if (type == typeof(decimal))
                return TypeCode.Decimal;
            else if (type == typeof(string))
                return TypeCode.String;
            else
                return TypeCode.Object;
        }
        internal static ArrayTypeCode ToArrayTypeCode(this Type type)
        {
            if (!type.IsArray)
                return ArrayTypeCode.InvalidType;

            if (type == typeof(bool[]))
                return ArrayTypeCode.BooleanArray;
            else if (type == typeof(char[]))
                return ArrayTypeCode.CharArray;
            else if (type == typeof(sbyte[]))
                return ArrayTypeCode.SByteArray;
            else if (type == typeof(byte[]))
                return ArrayTypeCode.ByteArray;
            else if (type == typeof(short[]))
                return ArrayTypeCode.Int16Array;
            else if (type == typeof(ushort[]))
                return ArrayTypeCode.UInt16Array;
            else if (type == typeof(int[]))
                return ArrayTypeCode.Int32Array;
            else if (type == typeof(uint[]))
                return ArrayTypeCode.UInt32Array;
            else if (type == typeof(long[]))
                return ArrayTypeCode.Int64Array;
            else if (type == typeof(ulong[]))
                return ArrayTypeCode.UInt64Array;
            else if (type == typeof(float[]))
                return ArrayTypeCode.SingleArray;
            else if (type == typeof(double[]))
                return ArrayTypeCode.DoubleArray;
            else if (type == typeof(decimal[]))
                return ArrayTypeCode.DecimalArray;
            else if (type == typeof(string[]))
                return ArrayTypeCode.StringArray;
            else if (type == typeof(object[]))
                return ArrayTypeCode.ObjectArray;
            else
            {
                var eType = type.GetElementType();
                if (eType.IsArray)
                    return eType.ToArrayTypeCode();
                else
                {
                    var instance = GetStorageInterfaceInstanceFromType(eType);
                    if (instance == null)
                        return ArrayTypeCode.InvalidType;
                    else
                        return ArrayTypeCode.SerializableArray;
                }
            }
        }
        internal static ISerializableObject GetStorageInterfaceInstanceFromTypeName(string typeName)
        {
            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.Name == typeName && type.GetInterface(nameof(ISerializableObject)) != null)
                        return GetStorageInterfaceInstanceFromType(type);
                }
            }

            return null;
        }
        internal static ISerializableObject GetStorageInterfaceInstanceFromType(Type type)
        {
            if (type.GetInterface(nameof(ISerializableObject)) != null)
            {
                var ctor = type.GetConstructors()[0];
                return (ISerializableObject)ctor.Invoke(new object[ctor.GetParameters().Length]);
            }

            return null;
        }
        internal static Type ToType(this TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Char:
                    return typeof(char);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.UInt16:
                    return typeof(ushort);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.UInt64:
                    return typeof(uint);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.String:
                    return typeof(string);
            }

            return typeof(object);
        }
        
        #endregion
    }
}
