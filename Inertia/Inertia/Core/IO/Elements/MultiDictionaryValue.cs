using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    internal class MultiDictionaryValue<T> : IDisposable
    {
        public T Data { get { return (T)_data; } set { _data = value; } }

        public bool IsByteArray { get; private set; }

        private object _identifier;
        private object _data;

        public MultiDictionaryValue(object identifier, T data)
        {
            this._identifier = identifier;
            this._data = data;

            GetIdentifierTypeCode();
            GetDatasTypeCode();
        }

        private TypeCode GetTypeCode(object value)
        {
            var code = TypesConfig.GetTypeCode(value.GetType());
            return code == TypeCode.Object ? TypeCode.String : code;
        }

        public TypeCode GetIdentifierTypeCode()
        {
            return GetTypeCode(_identifier);
        }
        public TypeCode GetDatasTypeCode()
        {
            if (_data.GetType() == typeof(byte[]))
            {
                IsByteArray = true;
                return TypeCode.Byte;
            }
            else
                return GetTypeCode(_data);
        }

        public void Dispose()
        {
            _identifier = null;
            _data = null;
        }

        public static implicit operator MultiDictionaryValue<object>(MultiDictionaryValue<T> v)
        {
            if (v == null) {
                Logger.Error($"Implicit convertion to value<object> invalid -null exception");
                return null;
            }

            return new MultiDictionaryValue<object>(v._identifier, v._data);
        }
        public static implicit operator MultiDictionaryValue<T>(MultiDictionaryValue<object> v)
        {
            if (v == null) {
                Logger.Error($"Implicit convertion to value<T> invalid -null exception");
                return null;
            }
            return new MultiDictionaryValue<T>(v._identifier, (T)v._data);
        }
    }
}
