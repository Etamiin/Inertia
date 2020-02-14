using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class SelectionRow : IDisposable
    {
        #region Public variables

        public int FieldCount
        {
            get
            {
                return Fields.Count;
            }
        }

        #endregion

        #region Private variables

        private Dictionary<string, object> Fields;

        #endregion

        #region Constructors

        internal SelectionRow()
        {
            Fields = new Dictionary<string, object>();
        }

        #endregion

        public object this[string field]
        {
            get
            {
                Fields.TryGetValue(field, out object value);
                return value;
            }
        }

        public bool TryGetValue<T>(string field, out T value)
        {
            value = default;
            if (!Fields.ContainsKey(field))
                return false;

            var obj = Fields[field];
            if (obj is T)
            {
                value = (T)obj;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Fields.Clear();
            Fields = null;
        }
    
        internal void AddField(string key, object value)
        {
            if (Fields.ContainsKey(key))
                return;

            Fields.Add(key, value);
        }
    }
}
