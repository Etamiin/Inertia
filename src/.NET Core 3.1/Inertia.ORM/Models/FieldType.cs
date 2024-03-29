﻿using System;
using System.Collections.Generic;

namespace Inertia.ORM
{
    internal sealed class FieldType
    {
        private static Dictionary<Type, FieldType> FieldTypes;

        private static void GenerateFields()
        {
            FieldTypes = new Dictionary<Type, FieldType>
            {
                { typeof(byte), new FieldType("TINYINT", true, TypeCode.Byte) },
                { typeof(ushort), new FieldType("SMALLINT", true, TypeCode.UInt16) },
                { typeof(uint), new FieldType("INT", true, TypeCode.UInt32) },
                { typeof(ulong), new FieldType("BIGINT", true, TypeCode.UInt64) },
                { typeof(sbyte), new FieldType("TINYINT", false, TypeCode.SByte) },
                { typeof(short), new FieldType("SMALLINT", false, TypeCode.Int16) },
                { typeof(int), new FieldType("INT", false, TypeCode.Int32) },
                { typeof(long), new FieldType("BIGINT", false, TypeCode.Int64) },
                { typeof(float), new FieldType("FLOAT", false, TypeCode.Single) },
                { typeof(double), new FieldType("DOUBLE", false, TypeCode.Double) },
                { typeof(decimal), new FieldType("DECIMAL", false, TypeCode.Decimal) },
                { typeof(char), new FieldType("CHAR", false, TypeCode.Char) },
                { typeof(DateTime), new FieldType("DATETIME", false, TypeCode.DateTime) },
                { typeof(byte[]), new FieldType("BLOB", false, TypeCode.Byte) },
                { typeof(string), new FieldType("TEXT", false, TypeCode.String) },
                { typeof(bool), new FieldType("BIT", false, TypeCode.Boolean) },
                { typeof(object), new FieldType("OBJECT", false, TypeCode.Object) }
            };
        }

        internal static FieldType GetFieldType(Type type)
        {
            if (FieldTypes == null)
            {
                GenerateFields();
            }
            if (!FieldTypes.ContainsKey(type))
            {
                return FieldTypes[typeof(object)];
            }

            return FieldTypes[type];
        }

        internal readonly TypeCode Code;
        internal readonly string SqlType;
        internal readonly bool Unsigned;

        internal FieldType(string type, bool unsigned, TypeCode code)
        {
            SqlType = type;
            Unsigned = unsigned;
            Code = code;
        }
    }
}