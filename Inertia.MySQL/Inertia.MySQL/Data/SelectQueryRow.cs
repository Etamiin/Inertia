using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class SelectQueryRow : IDisposable
    {
        public int FieldCount
        {
            get
            {
                return fields.Count;
            }
        }

        private Dictionary<string, object> fields;

        public SelectQueryRow()
        {
            fields = new Dictionary<string, object>();
        }

        public object this[string field]
        {
            set {
                if (!fields.ContainsKey(field))
                    fields.Add(field, value);
                else
                    fields[field] = value;
            }
            get {
                if (!fields.ContainsKey(field))
                    return null;
                return fields[field];
            }
        }

        public T Select<T>(string field)
        {
            fields.TryGetValue(field, out object value);
            return (T)value;
        }

        public void Dispose()
        {
            fields.Clear();
            fields = null;
        }
    }
}
