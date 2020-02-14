using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal class FlexDictionaryValue<T> : IDisposable
    {
        #region Public variables

        public T Data { get { return (T)BaseData; } set { BaseData = value; } }
        public bool IsByteArray { get; private set; }

        #endregion

        #region Private variables

        private object Identifier;
        private object BaseData;

        #endregion

        #region Constructors

        public FlexDictionaryValue(object identifier, T data)
        {
            Identifier = identifier;
            BaseData = data;

            GetIdentifierTypeCode();
            GetDataTypeCode();
        }

        #endregion

        private TypeCode GetTypeCode(object value)
        {
            var code = value.GetType().ToTypeCode();
            return code == TypeCode.Object ? TypeCode.String : code;
        }

        public TypeCode GetIdentifierTypeCode()
        {
            return GetTypeCode(Identifier);
        }
        public TypeCode GetDataTypeCode()
        {
            if (BaseData.GetType() == typeof(byte[]))
            {
                IsByteArray = true;
                return TypeCode.Byte;
            }
            else
                return GetTypeCode(BaseData);
        }

        public void Dispose()
        {
            Identifier = null;
            BaseData = null;
        }

        public static implicit operator FlexDictionaryValue<object>(FlexDictionaryValue<T> v)
        {
            if (v == null)
                throw new ArgumentNullException($"Implicit conversion to value<object> invalid -null exception");

            return new FlexDictionaryValue<object>(v.Identifier, v.BaseData);
        }
        public static implicit operator FlexDictionaryValue<T>(FlexDictionaryValue<object> v)
        {
            if (v == null)
                throw new ArgumentNullException($"Implicit conversion to value<{ nameof(T) }> invalid -null exception");
            return new FlexDictionaryValue<T>(v.Identifier, (T)v.BaseData);
        }
    }
}
