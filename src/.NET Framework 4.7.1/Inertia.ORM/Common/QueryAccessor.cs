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
    public sealed class QueryAccessor<TableKey> : IDisposable where TableKey : Table
    {
        private Database _database;
        private SqlCondition _cd;

        public QueryAccessor()
        {
            var link = typeof(TableKey).GetCustomAttribute<TableLink>();
            if (link != null)
            {
                if (!SqlManager.TrySearchDatabase(link.DatabaseType, out _database))
                {
                    throw new ArgumentNullException(link.DatabaseType.Name, "Invalid Database for the QueryAccessor.");
                }
            }
            else
            {
                var tableName = typeof(TableKey).Name;
                throw new ArgumentNullException(tableName, $"No Database attached to { tableName } table.");
            }
        }

        /// <summary>
        /// Create a new instance of <see cref="SqlCondition"/> and attach to the current instance of <see cref="QueryAccessor{TableKey}"/>
        /// </summary>
        /// <param name="onCondition"></param>
        /// <returns></returns>
        public QueryAccessor<TableKey> UseCondition(BasicAction<SqlCondition> onCondition)
        {
            if (_cd == null)
            {
                _cd = new SqlCondition();
            }

            onCondition(_cd);
            return this;
        }
        /// <summary>
        /// If any <see cref="SqlCondition"/> attached, remove it and return the current instance
        /// </summary>
        /// <returns></returns>
        public QueryAccessor<TableKey> ClearCondition()
        {
            _cd = null;
            return this;
        }

        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public TableKey Select(params string[] columnsToSelect)
        {
            return Select(_cd, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey Select(bool distinct, params string[] columnsToSelect)
        {
            return Select(_cd, distinct, columnsToSelect);
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
            return SelectAll(_cd, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="TableKey"/>.
        /// </summary>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public TableKey[] SelectAll(bool distinct, params string[] columnsToSelect)
        {
            return SelectAll(_cd, distinct, columnsToSelect);
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
        /// Delete all the elements from the specified <see cref="Table"/> based on the linked <see cref="SqlCondition"/>
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            return _database.Delete<TableKey>(_cd);
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
            return Count(string.Empty, _cd, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count(bool distinct)
        {
            return Count(string.Empty, _cd, distinct);
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
            return Count(columnName, _cd, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count(string columnName, bool distinct)
        {
            return Count(columnName, _cd, distinct);
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
        /// Return true if a row exist in the database based on the linked conditions
        /// </summary>
        /// <returns></returns>
        public bool Exist()
        {
            return Exist(_cd);
        }
        /// <summary>
        /// Return true if a row exist in the database based on the linked conditions
        /// </summary>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public bool Exist(bool distinct)
        {
            return Exist(_cd, distinct);
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
            return Average(columnName, _cd);
        }        
        public long Average(string columnName, SqlCondition condition)
        {
            return _database.Average<TableKey>(columnName, condition);
        }

        public TableKey Max(string columnName)
        {
            return Max(columnName, _cd);
        }        
        public TableKey Max(string columnName, SqlCondition condition)
        {
            return _database.Max<TableKey>(columnName, condition);
        }

        public TableKey Min(string columnName)
        {
            return Min(columnName, _cd);
        }        
        public TableKey Min(string columnName, SqlCondition condition)
        {
            return _database.Min<TableKey>(columnName, condition);
        }

        public decimal Sum(string columnName)
        {
            return Sum(columnName, _cd);
        }
        public decimal Sum(string columnName, SqlCondition condition)
        {
            return _database.Sum<TableKey>(columnName, condition);
        }

        public void SelectAsync(BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(_cd, onSelected, false, columnsToSelect);
        }
        public void SelectAsync(BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAsync(_cd, onSelected, distinct, columnsToSelect);
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
            SelectAllAsync(_cd, onSelected, false, columnsToSelect);
        }
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAllAsync(_cd, onSelected, distinct, columnsToSelect);
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

        public void DeleteAsync(BasicAction onDeleted)
        {
            DeleteAsync(_cd, onDeleted);
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
            CountAsync(string.Empty, _cd, onCounted, false);
        }
        public void CountAsync(BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(string.Empty, _cd, onCounted, distinct);
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
            CountAsync(columnName, _cd, onCounted, false);
        }
        public void CountAsync(string columnName, BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(columnName, _cd, onCounted, distinct);
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

        public void ExistAsync(BasicAction<bool> onExist)
        {
            ExistAsync(_cd, onExist);
        }
        public void ExistAsync(BasicAction<bool> onExist, bool distinct)
        {
            ExistAsync(_cd, onExist, distinct);
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
            MaxAsync(columnName, _cd, onMax);
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
            MinAsync(columnName, _cd, onMin);
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
            SumAsync(columnName, _cd, onSum);
        }
        public void SumAsync(string columnName, SqlCondition condition, BasicAction<decimal> onSum)
        {
            SqlManager.PoolAsyncOperation(() => {
                var sum = _database.Sum<TableKey>(columnName, condition);
                onSum?.Invoke(sum);
            });            
        }
    
        public void Dispose()
        {
            _database = null;
            _cd.Dispose();
        }
    }
}
