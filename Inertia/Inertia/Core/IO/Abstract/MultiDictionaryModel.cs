using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class MultiDictionaryBase<TKey> : IDisposable
    {
        public int Count => _memory.Count;

        private Dictionary<TKey, MultiDictionaryValue<object>> _memory = new Dictionary<TKey, MultiDictionaryValue<object>>();

        public void Add<TDatas>(TKey identifier, TDatas data)
        {
            if (!_memory.ContainsKey(identifier))
                _memory.Add(identifier, new MultiDictionaryValue<TDatas>(identifier, data));
        }
        public void Remove(TKey identifier)
        {
            if (_memory.ContainsKey(identifier))
            {
                GetDatasExtension<object>(identifier).Dispose();
                _memory.Remove(identifier);
            }
        }
        public void Replace<TDatas>(TKey identifier, TDatas value)
        {
            if (_memory.ContainsKey(identifier))
                _memory[identifier] = new MultiDictionaryValue<TDatas>(identifier, value);
        }

        public TDatas Get<TDatas>(TKey identifier)
        {
            var value = Get(identifier);
            if (value == null || value.GetType() != typeof(TDatas))
                return default(TDatas);
            return (TDatas)value;
        }
        public object Get(TKey identifier)
        {
            var datasExt = GetDatasExtension<object>(identifier);
            return datasExt?.Data;
        }

        public bool Exist(TKey identifier)
        {
            return _memory.ContainsKey(identifier);
        }
        internal MultiDictionaryValue<T> GetDatasExtension<T>(TKey identifier)
        {
            return _memory.ContainsKey(identifier) ? _memory[identifier] : null;
        }

        public TKey[] GetKeys()
        {
            lock (_memory)
                return _memory.Keys.ToArray();
        }

        public void Dispose()
        {
            _memory.Clear();
            _memory = null;
        }
    }
}
