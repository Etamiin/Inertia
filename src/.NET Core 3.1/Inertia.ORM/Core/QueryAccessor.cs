using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// A help tool for faster access to queries
    /// </summary>
    /// <typeparam name="TableKey"></typeparam>
    public sealed class QueryAccessor<TableKey> where TableKey : Table
    {
        private readonly Database _database;

        /// <summary>
        /// Initialize a new instance of the class <see cref="QueryAccessor{TableKey}"/>
        /// </summary>
        public QueryAccessor()
        {
            var attachedTo = typeof(TableKey).GetCustomAttribute<TableLink>();
            if (attachedTo != null)
            {
                if (!SqlManager.TrySearchDatabase(attachedTo.DatabaseType, out _database))
                {
                    throw new ArgumentNullException(attachedTo.DatabaseType.Name, "Invalid Database for the QueryAccessor.");
                }
            }
            else
            {
                var tableName = typeof(TableKey).Name;
                throw new ArgumentNullException(tableName, $"No Database attached to { tableName } table.");
            }
        }

        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public TableKey Select(params string[] columnsToSelect)
        {
            return Select(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
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
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey Select(SqlCondition condition, bool distinct, params string[] columnsToSelect)
        {
            return _database.Select<TableKey>(condition, distinct, columnsToSelect);
        }

        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(params string[] columnsToSelect)
        {
            return SelectAll(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
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
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(SqlCondition condition, bool distinct, params string[] columnsToSelect)
        {
            return _database.SelectAll<TableKey>(condition, distinct, columnsToSelect);
        }

        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/> based on the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Delete(SqlCondition condition)
        {
            return _database.Delete<TableKey>(condition);
        }
        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/>
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            return _database.DeleteAll<TableKey>();
        }

        /// <summary>
        /// Update all the elements in the specified <see cref="Table"/> with the reference's values
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns></returns>
        public bool UpdateAll(TableKey reference, params string[] columnsToUpdate)
        {
            return _database.UpdateAll(reference, columnsToUpdate);
        }

        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return Count(string.Empty, null, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count(bool distinct)
        {
            return Count(string.Empty, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count(SqlCondition condition)
        {
            return Count(string.Empty, condition, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count(SqlCondition condition, bool distinct)
        {
            return Count(string.Empty, condition, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count(string columnName)
        {
            return Count(columnName, null, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count(string columnName, bool distinct)
        {
            return Count(columnName, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count(string columnName, SqlCondition condition)
        {
            return _database.Count<TableKey>(columnName, condition, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count(string columnName, SqlCondition condition, bool distinct)
        {
            return _database.Count<TableKey>(columnName, condition, distinct);
        }

        /// <summary>
        /// Return true if a row exist in the database based on the specified conditions
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Exist(SqlCondition condition)
        {
            return _database.Count<TableKey>(condition, false) > 0;
        }
        /// <summary>
        /// Return true if a row exist in the database based on the specified conditions
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public bool Exist(SqlCondition condition, bool distinct)
        {
            return _database.Count<TableKey>(condition, distinct) > 0;
        }

        public long Average(string columnName)
        {
            return Average(columnName, null);
        }        
        public long Average(string columnName, SqlCondition condition)
        {
            return _database.Average<TableKey>(columnName, condition);
        }

        public TableKey Max(string columnName)
        {
            return Max(columnName, null);
        }        
        public TableKey Max(string columnName, SqlCondition condition)
        {
            return _database.Max<TableKey>(columnName, condition);
        }

        public TableKey Min(string columnName)
        {
            return Min(columnName, null);
        }        
        public TableKey Min(string columnName, SqlCondition condition)
        {
            return _database.Min<TableKey>(columnName, condition);
        }

        public decimal Sum(string columnName)
        {
            return Sum(columnName, null);
        }
        public decimal Sum(string columnName, SqlCondition condition)
        {
            return _database.Sum<TableKey>(columnName, condition);
        }

        public void SelectAsync(BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, false, columnsToSelect);
        }
        public void SelectAsync(BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, distinct, columnsToSelect);
        }
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(condition, onSelected, false, columnsToSelect);
        }
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.PoolAsyncOperation(() => {
                var table = _database.Select<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(table);
            });
        }

        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, false, columnsToSelect);
        }
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, distinct, columnsToSelect);
        }

        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(condition, onSelected, false, columnsToSelect);
        }
        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.PoolAsyncOperation(() => {
                var tables = _database.SelectAll<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(tables);
            });
        }

        public void DeleteAsync(SqlCondition condition, BasicAction onDeleted)
        {
            SqlManager.PoolAsyncOperation(() => {
                var deleted = _database.Delete<TableKey>(condition);
                if (deleted)
                {
                    onDeleted?.Invoke();
                }
            });
        }
        public void DeleteAllAsync(BasicAction onDeleted)
        {
            SqlManager.PoolAsyncOperation(() => {
                var deleted = _database.DeleteAll<TableKey>();
                if (deleted)
                {
                    onDeleted?.Invoke();
                }
            });
        }

        public void UpdateAllAsync(TableKey reference, BasicAction onUpdated, params string[] columnsToUpdate)
        {
            SqlManager.PoolAsyncOperation(() => {
                var updated = _database.UpdateAll(reference, columnsToUpdate);
                if (updated)
                {
                    onUpdated?.Invoke();
                }
            });
        }

        public void CountAsync(BasicAction<long> onCounted)
        {
            CountAsync(string.Empty, null, onCounted, false);
        }
        public void CountAsync(BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(string.Empty, null, onCounted, distinct);
        }
        public void CountAsync(SqlCondition condition, BasicAction<long> onCounted)
        {
            CountAsync(string.Empty, condition, onCounted, false);
        }
        public void CountAsync(SqlCondition condition, BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(string.Empty, condition, onCounted, distinct);
        }
        public void CountAsync(string columnName, BasicAction<long> onCounted)
        {
            CountAsync(columnName, null, onCounted, false);
        }
        public void CountAsync(string columnName, BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(columnName, null, onCounted, distinct);
        }
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted)
        {
            CountAsync(columnName, condition, onCounted, false);
        }
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted, bool distinct)
        {
            SqlManager.PoolAsyncOperation(() => {
                var count = _database.Count<TableKey>(columnName, condition, distinct);
                onCounted?.Invoke(count);
            });
        }

        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist)
        {
            SqlManager.PoolAsyncOperation(() => {
                var count = _database.Count<TableKey>(condition, false);
                onExist?.Invoke(count > 0);
            });
        }
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist, bool distinct)
        {
            SqlManager.PoolAsyncOperation(() => {
                var count = _database.Count<TableKey>(condition, distinct);
                onExist?.Invoke(count > 0);
            });
        }

        public void AverageAsync(string columnName, BasicAction<long> onAverage)
        {
            AverageAsync(columnName, null, onAverage);
        }
        public void AverageAsync(string columnName, SqlCondition condition, BasicAction<long> onAverage)
        {
            SqlManager.PoolAsyncOperation(() => {
                var average = _database.Average<TableKey>(columnName, condition);
                onAverage?.Invoke(average);
            });
        }

        public void MaxAsync(string columnName, BasicAction<TableKey> onMax)
        {
            MaxAsync(columnName, null, onMax);
        }
        public void MaxAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMax)
        {
            SqlManager.PoolAsyncOperation(() => {
                var max = _database.Max<TableKey>(columnName, condition);
                onMax?.Invoke(max);
            });            
        }

        public void MinAsync(string columnName, BasicAction<TableKey> onMin)
        {
            MinAsync(columnName, null, onMin);
        }
        public void MinAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMin)
        {
            SqlManager.PoolAsyncOperation(() => {
                var min = _database.Min<TableKey>(columnName, condition);
                onMin?.Invoke(min);
            });            
        }

        public void SumAsync(string columnName, BasicAction<decimal> onSum)
        {
            SumAsync(columnName, null, onSum);
        }
        public void SumAsync(string columnName, SqlCondition condition, BasicAction<decimal> onSum)
        {
            SqlManager.PoolAsyncOperation(() => {
                var sum = _database.Sum<TableKey>(columnName, condition);
                onSum?.Invoke(sum);
            });            
        }
    }
}
