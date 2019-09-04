using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class QueryResultSelector : IDisposable
    {
        public int RowCount
        {
            get
            {
                return rows.Length;
            }
        }

        public QueryResultRow CurrentRow { get; private set; }

        private QueryResultRow[] rows;
        private int currentPosition;

        public QueryResultSelector(MySqlDataReader reader)
        {
            var rowList = new List<QueryResultRow>();

            while (reader.Read())
            {
                var row = new QueryResultRow();

                for (var i = 0; i < reader.FieldCount; i++) {
                    var name = reader.GetName(i);
                    row.AddField(name, reader.GetValue(i));
                }
                rowList.Add(row);
            }

            rows = rowList.ToArray();
            NextRow();
        }

        public QueryResultRow NextRow()
        {
            if (currentPosition >= rows.Length) {
                CurrentRow = null;
                return null;
            }

            CurrentRow = rows[currentPosition++];
            return CurrentRow;
        }

        public void Dispose()
        {
            foreach (var row in rows)
                row.Dispose();

            rows = null;
            CurrentRow = null;
        }
    }
}
