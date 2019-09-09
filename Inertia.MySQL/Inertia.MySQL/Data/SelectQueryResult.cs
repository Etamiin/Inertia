using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class SelectQueryResult : IDisposable
    {
        public int RowCount
        {
            get
            {
                return rows.Length;
            }
        }

        public SelectQueryRow CurrentRow { get; private set; }

        private SelectQueryRow[] rows;
        private int currentPosition;

        public SelectQueryResult(MySqlDataReader reader)
        {
            var rowList = new List<SelectQueryRow>();

            while (reader.Read())
            {
                var row = new SelectQueryRow();

                for (var i = 0; i < reader.FieldCount; i++) {
                    var field = reader.GetName(i);
                    row[field] = reader.GetValue(i);
                }
                rowList.Add(row);
            }

            rows = rowList.ToArray();
            NextRow();
        }

        public SelectQueryRow NextRow()
        {
            if (currentPosition >= rows.Length) {
                CurrentRow = null;
                return null;
            }

            CurrentRow = rows[currentPosition++];
            return CurrentRow;
        }
        public SelectQueryRow GetRow(int index)
        {
            if (index < 0 || index >= RowCount)
                throw new ArgumentOutOfRangeException("index", "SelectQueryRow index was out of range, choose an index in the range 0 to RowCount");

            return rows[index];
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
