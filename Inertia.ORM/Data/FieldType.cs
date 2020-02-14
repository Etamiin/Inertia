using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
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
                { typeof(byte), new FieldType("tinyint", true) },
                { typeof(ushort), new FieldType("smallint", true) },
                { typeof(uint), new FieldType("int", true) },
                { typeof(ulong), new FieldType("bigint", true) },
                { typeof(sbyte), new FieldType("tinyint", false) },
                { typeof(short), new FieldType("smallint", false) },
                { typeof(int), new FieldType("int", false) },
                { typeof(long), new FieldType("bigint", false) },
                { typeof(float), new FieldType("float", false) },
                { typeof(double), new FieldType("double", false) },
                { typeof(decimal), new FieldType("decimal", false) },
                { typeof(char), new FieldType("char", false) },
                { typeof(DateTime), new FieldType("datetime", false) },
                { typeof(byte[]), new FieldType("blob", false) },
                { typeof(object), new FieldType("text", false) }
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

        #region Public variables

        public readonly string SqlType;
        public readonly bool Unsigned;

        #endregion

        #region Constructors

        public FieldType(string type, bool unsigned)
        {
            SqlType = type;
            Unsigned = unsigned;
        }

        #endregion
    }
}
