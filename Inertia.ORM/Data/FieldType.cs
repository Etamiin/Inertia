using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal class FieldType
    {
        #region Static variables

        private static Dictionary<Type, FieldType> FieldTypes;

        #endregion

        #region Static methods

        private static void GenerateFields()
        {
            FieldTypes = new Dictionary<Type, FieldType>
            {
                { typeof(byte), new FieldType("tinyint", true, TypeCode.Byte) },
                { typeof(ushort), new FieldType("smallint", true, TypeCode.UInt16) },
                { typeof(uint), new FieldType("int", true, TypeCode.UInt32) },
                { typeof(ulong), new FieldType("bigint", true, TypeCode.UInt64) },
                { typeof(sbyte), new FieldType("tinyint", false, TypeCode.SByte) },
                { typeof(short), new FieldType("smallint", false, TypeCode.Int16) },
                { typeof(int), new FieldType("int", false, TypeCode.Int32) },
                { typeof(long), new FieldType("bigint", false, TypeCode.Int64) },
                { typeof(float), new FieldType("float", false, TypeCode.Single) },
                { typeof(double), new FieldType("double", false, TypeCode.Double) },
                { typeof(decimal), new FieldType("decimal", false, TypeCode.Decimal) },
                { typeof(char), new FieldType("char", false, TypeCode.Char) },
                { typeof(DateTime), new FieldType("datetime", false, TypeCode.DateTime) },
                { typeof(byte[]), new FieldType("blob", false, TypeCode.Byte) },
                { typeof(string), new FieldType("text", false, TypeCode.String) },
                { typeof(object), new FieldType("object", false, TypeCode.Object) },
                { typeof(bool), new FieldType("bit", false, TypeCode.Boolean) }
            };
        }

        public static FieldType GetFieldType(Type type)
        {
            if (FieldTypes == null)
                GenerateFields();

            if (!FieldTypes.ContainsKey(type))
                return FieldTypes[typeof(object)];

            return FieldTypes[type];
        }

        #endregion

        #region Internal variables

        internal readonly TypeCode Code;
        internal readonly string SqlType;
        internal readonly bool Unsigned;

        #endregion

        #region Constructors

        public FieldType(string type, bool unsigned, TypeCode code)
        {
            SqlType = type;
            Unsigned = unsigned;
            Code = code;
        }

        #endregion
    }
}
