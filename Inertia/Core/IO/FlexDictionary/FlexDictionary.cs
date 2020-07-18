using Inertia.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Inertia.Storage;

namespace Inertia
{
    /// <summary>
    /// Represents a collection of keys associated with values that can be of any type
    /// </summary>
    /// <typeparam name="TKey">The type of object used for dictionary keys</typeparam>
    [Serializable]
    public class FlexDictionary<TKey> : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Returns the number of values that are stored in the dictionnary.
        /// </summary>
        public int Count => m_memory.Count;

        #endregion

        #region Private variables

        private Dictionary<TKey, FlexDictionaryValue<object>> m_memory;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of a <see cref="FlexDictionary{TKey}"/> class
        /// </summary>
        public FlexDictionary()
        {
            m_memory = new Dictionary<TKey, FlexDictionaryValue<object>>();
        }

        #endregion

        /// <summary>
        /// Retrieves the object stored in the dictionary with to the provided key
        /// </summary>
        /// <param name="key">The key associated to the target object</param>
        /// <returns>The object associated to the specified <paramref name="key"/> or null</returns>
        public object this[TKey key]
        {
            get
            {
                return GetDataExtension<object>(key)?.Data;
            }
        }

        /// <summary>
        /// Retrieves all the keys from the dictionnary
        /// </summary>
        /// <returns>An array of <typeparamref name="TKey"/> stored in the dictionnary</returns>
        public TKey[] GetKeys()
        {
            lock (m_memory)
                return m_memory.Keys.ToArray();
        }

        /// <summary>
        /// Add a new object associated to the specified key to the dictionnary
        /// </summary>
        /// <typeparam name="TData">Type of the object that will be added</typeparam>
        /// <param name="identifier">The key associated</param>
        /// <param name="data">The object to add</param>
        /// <returns>The current <see cref="FlexDictionary{TKey}"/> instance</returns>
        public FlexDictionary<TKey> Add<TData>(TKey identifier, TData data)
        {
            if (identifier == null)
                throw new NullReferenceException();

            if (!m_memory.ContainsKey(identifier))
                m_memory.Add(identifier, new FlexDictionaryValue<TData>(identifier, data));

            return this;
        }
        /// <summary>
        /// Remove the object from the dictionnary associated to the specified key
        /// </summary>
        /// <param name="identifier">The key to remove</param>
        /// <returns>The current <see cref="FlexDictionary{TKey}"/> instance</returns>
        public FlexDictionary<TKey> Remove(TKey identifier)
        {
            if (m_memory.ContainsKey(identifier))
            {
                GetDataExtension<object>(identifier).Dispose();
                m_memory.Remove(identifier);
            }

            return this;
        }
        /// <summary>
        /// Replace a stored object (nothing happend if the specified key don't exist)
        /// </summary>
        /// <typeparam name="TData">The type of the object to add</typeparam>
        /// <param name="identifier">The key that already exist</param>
        /// <param name="value">The new object</param>
        /// <returns>The current <see cref="FlexDictionary{TKey}"/> instance</returns>
        public FlexDictionary<TKey> Replace<TData>(TKey identifier, TData value)
        {
            if (m_memory.ContainsKey(identifier))
                m_memory[identifier] = new FlexDictionaryValue<TData>(identifier, value);

            return this;
        }

        /// <summary>
        /// Try to retrieve a stored object with the specified key
        /// </summary>
        /// <typeparam name="TData">The type of the object to get</typeparam>
        /// <param name="identifier">The key associated</param>
        /// <param name="value">The variable that will store the object</param>
        /// <returns>Return true if the object was found and referenced to <paramref name="value"/> or false if not</returns>
        /// <remarks>If the object wasn't found, the result will be the default value of <typeparamref name="TData"/></remarks>
        public bool TryGetValue<TData>(TKey identifier, out TData value)
        {
            if (Exist(identifier))
            {
                value = (TData)this[identifier];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Retrieve a stored object and return it
        /// </summary>
        /// <typeparam name="TData">The type of the target object</typeparam>
        /// <param name="identifier">The key associated to the target object</param>
        /// <returns>The target object that is stored</returns>
        /// <exception cref="NullReferenceException">Thrown if the specified key don't exist</exception>
        public TData GetValue<TData>(TKey identifier)
        {
            if (Exist(identifier))
                return (TData)this[identifier];

            return default;
        }
        /// <summary>
        /// Retrieve a list of object without type and return them
        /// </summary>
        /// <param name="identifiers">An array of key associated to the target objects</param>
        /// <returns>An array of objects that are stored</returns>
        /// <remarks>If a key don't exist in the dictionnary, the object in the returned array will be null</remarks>
        public object[] GetValues(TKey[] identifiers)
        {
            var values = new object[identifiers.Length];
            for (var i = 0; i < identifiers.Length; i++)
                values[i] = this[identifiers[i]];

            return values;
        }
        /// <summary>
        /// Retrieve a list of object with type and return them
        /// </summary>
        /// <typeparam name="TData">The target type for the objects</typeparam>
        /// <param name="identifiers">An array of key associated to the target objects</param>
        /// <returns>An array of objects of type <typeparamref name="TData"/></returns>
        /// <remarks>If an object wasn't found, the result will be the default value of <typeparamref name="TData"/></remarks>
        public TData[] GetValues<TData>(TKey[] identifiers)
        {
            var values = new TData[identifiers.Length];
            for (var i = 0; i < identifiers.Length; i++)
                TryGetValue(identifiers[i], out values[i]);

            return values;
        }

        /// <summary>
        /// Check if a key exist in the dictionnary
        /// </summary>
        /// <param name="identifier">The target key to find</param>
        /// <returns>Return true if the specified key exist in the dictionnary, or false if not</returns>
        public bool Exist(TKey identifier)
        {
            return m_memory.ContainsKey(identifier);
        }

        /// <summary>
        /// Clear the data stored in the current instance
        /// </summary>
        public void Clear()
        {
            m_memory.Clear();
        }
        
        /// <summary>
        /// Dispose all the resources used by the <see cref="FlexDictionary{TKey}"/> instance
        /// </summary>
        public void Dispose()
        {
            m_memory.Clear();
            m_memory = null;
        }

        internal virtual byte[] Serialize(BasicAction<StorageProgressionEventArgs> progressCallback)
        {
            var serializer = new BasicWriter();
            var keys = GetKeys();
            var progression = new StorageProgressionEventArgs(keys.Length);

            serializer.SetInt(keys.Length);

            foreach (var key in keys)
            {
                var value = GetDataExtension<object>(key);
                if (value == null)
                    continue;

                var typeCode = value.GetDataTypeCode();

                serializer
                    .SetValue(key)
                    .SetByte((byte)typeCode);

                if (typeCode == TypeCode.Byte)
                    serializer.SetBool(value.isByteArray);

                serializer.SetValue(value.Data);
                progressCallback(progression.Progress());
            }

            return serializer.ToArrayAndDispose().Compress(out _);
        }
        internal virtual void Deserialize(byte[] data, BasicAction<StorageProgressionEventArgs> progressCallback)
        {
            var deserializer = new BasicReader(data.Decompress());
            var length = deserializer.GetInt();
            var progression = new StorageProgressionEventArgs(length);

            for (var i = 0; i < length; i++)
            {
                var key = (TKey)deserializer.GetValue(typeof(TKey));
                var typeCode = (TypeCode)deserializer.GetByte();
                object value;
                if (typeCode == TypeCode.Byte && deserializer.GetBool())
                    value = deserializer.GetBytes();
                else
                    value = deserializer.GetValue(typeCode.ToType());

                Add(key, value);
                progressCallback(progression.Progress());
            }

            deserializer.Dispose();
        }

        internal FlexDictionaryValue<T> GetDataExtension<T>(TKey identifier)
        {
            return m_memory.ContainsKey(identifier) ? m_memory[identifier] : null;
        }
    }
}
