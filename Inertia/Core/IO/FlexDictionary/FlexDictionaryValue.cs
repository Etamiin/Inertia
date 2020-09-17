using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    [Serializable]
    internal class FlexDictionaryValue<T> : IDisposable
    {
        #region Public variables

        public T Data { get { return (T)m_baseData; } set { m_baseData = value; } }
        public bool isByteArray;

        #endregion

        #region Private variables

        private object m_identifier;
        private object m_baseData;

        #endregion

        #region Constructors

        public FlexDictionaryValue(object identifier, T data)
        {
            m_identifier = identifier;
            m_baseData = data;

            if (GetDataTypeCode() == TypeCode.Byte && data.GetType().IsArray)
                isByteArray = true;
        }

        #endregion

        public TypeCode GetDataTypeCode()
        {
            return m_baseData.GetType().ToTypeCode();
        }

        public void Dispose()
        {
            m_identifier = null;
            m_baseData = null;
        }

        public static implicit operator FlexDictionaryValue<object>(FlexDictionaryValue<T> v)
        {
            if (v == null)
                throw new ArgumentNullException($"Implicit conversion to value<object> invalid -null exception");

            return new FlexDictionaryValue<object>(v.m_identifier, v.m_baseData);
        }
        public static implicit operator FlexDictionaryValue<T>(FlexDictionaryValue<object> v)
        {
            if (v == null)
                throw new ArgumentNullException($"Implicit conversion to value<{ typeof(T).Name }> invalid -null exception");
            return new FlexDictionaryValue<T>(v.m_identifier, (T)v.m_baseData);
        }
    }
}
