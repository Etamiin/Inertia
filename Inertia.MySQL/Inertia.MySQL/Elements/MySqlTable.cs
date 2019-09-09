using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class MySqlTable
    {
        private readonly string Table;
        private readonly uint Limit;

        public MySqlTable(string Table) : this(Table, 0)
        {
        }
        public MySqlTable(string Table, uint Limit)
        {
            this.Table = Table;
            this.Limit = Limit;
        }

        public Query.Delete Delete()
        {
            return new Query.Delete(Table, Limit);
        }

        public Query.Update Update()
        {
            return new Query.Update(Table, Limit);
        }
        public Query.Update Update(string[] fields, object[] values)
        {
            return Update().SetRange(fields, values);
        }

        public long Insert(string[] fields, object[] values)
        {
            return new Query.Insert(Table, Limit).AddRange(fields, values).Execute();
        }
        public void InsertAsync(string[] fields, object[] values, Action<long> OnExecuted)
        {
            RuntimeModule.ExecuteAsync(() => OnExecuted(Insert(fields, values)));
        }

        public Query.Select Select()
        {
            return new Query.Select(Table, Limit);
        }
        public Query.Select Select(params string[] fields)
        {
            return new Query.Select(Table, Limit).AddFields(fields);
        }
    }
}
