using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class SelectionResult : IDisposable
    {
        #region Public variables

        public int RowCount
        {
            get
            {
                return Rows.Length;
            }
        }

        #endregion

        #region Private variables

        private SelectionRow[] Rows;
        private int Position;

        #endregion

        #region Constructors

        internal SelectionResult(MySqlDataReader reader)
        {
            var rowList = new List<SelectionRow>();
            while (reader.Read())
            {
                var row = new SelectionRow();
                for (var i = 0; i < reader.FieldCount; i++)
                    row.AddField(reader.GetName(i), reader.GetValue(i));

                rowList.Add(row);
            }

            reader.Dispose();
            Rows = rowList.ToArray();
        }

        #endregion

        public SelectionRow GetCurrentRow()
        {
            return Rows[Position];
        }
        public SelectionRow GetNextRow()
        {
            if (Position + 1 >= RowCount)
                return null;

            Position++;
            return GetCurrentRow();
        }
        public SelectionRow GetRowAt(int index)
        {
            if (index < 0 || index >= RowCount)
                throw new IndexOutOfRangeException();

            return Rows[index];
        }

        public void ExecuteOnSelection(OrmRowExecutionHandler executor)
        {
            Position = -1;

            SelectionRow row;
            while ((row = GetNextRow()) != null)
                executor(row);
        }

        public void Dispose()
        {
            foreach (var row in Rows)
                row.Dispose();

            Rows = null;
        }
    }
}
