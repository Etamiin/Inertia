using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// A help tool for faster access to queries
    /// </summary>
    /// <typeparam name="TableKey"></typeparam>
    /// <typeparam name="DatabaseKey"></typeparam>
    public sealed class QueryAccessor<TableKey, DatabaseKey> where TableKey : Table where DatabaseKey : Database
    {
        private DatabaseKey m_database;

        /// <summary>
        /// Initialize a new instance of the class <see cref="QueryAccessor{TableKey, DatabaseKey}"/>
        /// </summary>
        public QueryAccessor()
        {
            SqlManager.TrySearchDatabase(out m_database);

            //if (!SqlManager.TrySearchDatabase(out m_database))
            if (m_database == null)
                throw new ArgumentNullException(typeof(DatabaseKey).Name, "Invalid Database for the QueryAccessor.");
        }

        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public TableKey Select(params string[] columnsToSelect)
        {
            return Select(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey Select(bool distinct, params string[] columnsToSelect)
        {
            return Select(null, distinct, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TableKey Select(SqlCondition condition, params string[] columnsToSelect)
        {
            return Select(condition, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey Select(SqlCondition condition, bool distinct, params string[] columnsToSelect)
        {
            return m_database.Select<TableKey>(condition, distinct, columnsToSelect);
        }

        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(params string[] columnsToSelect)
        {
            return SelectAll(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(bool distinct, params string[] columnsToSelect)
        {
            return SelectAll(null, distinct, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(SqlCondition condition, params string[] columnsToSelect)
        {
            return SelectAll(condition, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(SqlCondition condition, bool distinct, params string[] columnsToSelect)
        {
            return m_database.SelectAll<TableKey>(condition, distinct, columnsToSelect);
        }


        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/> based on the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Delete(SqlCondition condition)
        {
            return m_database.Delete<TableKey>(condition);
        }
        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/>
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <returns></returns>
        public bool DeleteAll()
        {
            return m_database.DeleteAll<TableKey>();
        }

        /// <summary>
        /// Update all the elements in the specified <see cref="Table"/> with the reference's values
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="reference"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns></returns>
        public bool UpdateAll(TableKey reference, params string[] columnsToUpdate)
        {
            return m_database.UpdateAll(reference, columnsToUpdate);
        }

        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count(bool distinct = false)
        {
            return Count(string.Empty, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count(SqlCondition condition, bool distinct = false)
        {
            return Count(string.Empty, condition, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count(string columnName, bool distinct = false)
        {
            return Count(columnName, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count(string columnName, SqlCondition condition, bool distinct = false)
        {
            return m_database.Count<TableKey>(columnName, condition, distinct);
        }

        /// <summary>
        /// Return true if a row exist in the database based on the specified conditions
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public bool Exist(SqlCondition condition, bool distinct = false)
        {
            return m_database.Count<TableKey>(condition, distinct) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Average(string columnName)
        {
            return Average(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Average(string columnName, SqlCondition condition)
        {
            return m_database.Average<TableKey>(columnName, condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public TableKey Max(string columnName)
        {
            return Max(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TableKey Max(string columnName, SqlCondition condition)
        {
            return m_database.Max<TableKey>(columnName, condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public TableKey Min(string columnName)
        {
            return Min(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TableKey Min(string columnName, SqlCondition condition)
        {
            return m_database.Min<TableKey>(columnName, condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public decimal Sum(string columnName)
        {
            return Sum(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TableKey"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public decimal Sum(string columnName, SqlCondition condition)
        {
            return m_database.Sum<TableKey>(columnName, condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAsync(BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, false, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="distinct"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAsync(BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, distinct, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(condition, onSelected, false, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var table = m_database.Select<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(table);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, false, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="distinct"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, distinct, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(condition, onSelected, false, columnsToSelect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onSelected"></param>
        /// <param name="distinct"></param>
        /// <param name="columnsToSelect"></param>
        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var tables = m_database.SelectAll<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(tables);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onDeleted"></param>
        public void DeleteAsync(SqlCondition condition, BasicAction onDeleted)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var deleted = m_database.Delete<TableKey>(condition);
                if (deleted) onDeleted?.Invoke();
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onDeleted"></param>
        public void DeleteAllAsync(BasicAction onDeleted)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var deleted = m_database.DeleteAll<TableKey>();
                if (deleted) onDeleted?.Invoke();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="onUpdated"></param>
        /// <param name="columnsToUpdate"></param>
        public void UpdateAllAsync(TableKey reference, BasicAction onUpdated, params string[] columnsToUpdate)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var updated = m_database.UpdateAll(reference, columnsToUpdate);
                if (updated) onUpdated?.Invoke();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
        public void CountAsync(BasicAction<long> onCounted, bool distinct = false)
        {
            CountAsync(string.Empty, null, onCounted, distinct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
        public void CountAsync(SqlCondition condition, BasicAction<long> onCounted, bool distinct = false)
        {
            CountAsync(string.Empty, condition, onCounted, distinct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
        public void CountAsync(string columnName, BasicAction<long> onCounted, bool distinct = false)
        {
            CountAsync(columnName, null, onCounted, distinct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted, bool distinct = false)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var count = m_database.Count<TableKey>(columnName, condition, distinct);
                onCounted?.Invoke(count);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onExist"></param>
        /// <param name="distinct"></param>
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist, bool distinct = false)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var count = m_database.Count<TableKey>(condition, distinct);
                onExist?.Invoke(count > 0);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onAverage"></param>
        public void AverageAsync(string columnName, BasicAction<long> onAverage)
        {
            AverageAsync(columnName, null, onAverage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onAverage"></param>
        public void AverageAsync(string columnName, SqlCondition condition, BasicAction<long> onAverage)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var average = m_database.Average<TableKey>(columnName, condition);
                onAverage?.Invoke(average);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onMax"></param>
        public void MaxAsync(string columnName, BasicAction<TableKey> onMax)
        {
            MaxAsync(columnName, null, onMax);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onMax"></param>
        public void MaxAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMax)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var max = m_database.Max<TableKey>(columnName, condition);
                onMax?.Invoke(max);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onMin"></param>
        public void MinAsync(string columnName, BasicAction<TableKey> onMin)
        {
            MinAsync(columnName, null, onMin);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onMin"></param>
        public void MinAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMin)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var min = m_database.Min<TableKey>(columnName, condition);
                onMin?.Invoke(min);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onSum"></param>
        public void SumAsync(string columnName, BasicAction<decimal> onSum)
        {
            SumAsync(columnName, null, onSum);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onSum"></param>
        public void SumAsync(string columnName, SqlCondition condition, BasicAction<decimal> onSum)
        {
            SqlManager.EnqueueAsyncOperation(() => {
                var sum = m_database.Sum<TableKey>(columnName, condition);
                onSum?.Invoke(sum);
            });
        }
    }
}
