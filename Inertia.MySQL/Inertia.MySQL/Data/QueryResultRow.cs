using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class QueryResultRow : IDisposable
    {
        public int FieldCount
        {
            get
            {
                return fields.Count;
            }
        }

        private Dictionary<string, object> fields;

        public QueryResultRow()
        {
            fields = new Dictionary<string, object>();
        }

        internal void AddField(string name, object value)
        {
            fields.Add(name, value);
        }

        public T Get<T>(string name)
        {
            fields.TryGetValue(name, out object value);
            return (T)value;
        }

        public void Dispose()
        {
            fields.Clear();
            fields = null;
        }
    }
}
