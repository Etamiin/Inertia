using System;
using System.Collections.Generic;
using System.Linq;
<<<<<<< HEAD
<<<<<<< HEAD
=======
using System.Reflection;
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
using System.Reflection;
>>>>>>> premaster
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// A help tool for faster access to queries
    /// </summary>
    /// <typeparam name="TableKey"></typeparam>
<<<<<<< HEAD
<<<<<<< HEAD
    /// <typeparam name="DatabaseKey"></typeparam>
    public sealed class QueryAccessor<TableKey, DatabaseKey> where TableKey : Table where DatabaseKey : Database
    {
        private readonly DatabaseKey _database;

        /// <summary>
        /// Initialize a new instance of the class <see cref="QueryAccessor{TableKey, DatabaseKey}"/>
        /// </summary>
        public QueryAccessor()
        {
            if (!SqlManager.TrySearchDatabase(out _database))
            {
                throw new ArgumentNullException(typeof(DatabaseKey).Name, "Invalid Database for the QueryAccessor.");
=======
=======
>>>>>>> premaster
    public sealed class QueryAccessor<TableKey> where TableKey : Table
    {
        private readonly Database _database;

        /// <summary>
        /// Initialize a new instance of the class <see cref="QueryAccessor{TableKey}"/>
        /// </summary>
        public QueryAccessor()
        {
<<<<<<< HEAD
            var attachedTo = typeof(TableKey).GetCustomAttribute<AttachTo>();
=======
            var attachedTo = typeof(TableKey).GetCustomAttribute<TableLink>();
>>>>>>> premaster
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
<<<<<<< HEAD
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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

<<<<<<< HEAD

=======
>>>>>>> premaster
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

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Average(string columnName)
        {
            return Average(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
=======
        public long Average(string columnName)
        {
            return Average(columnName, null);
        }        
>>>>>>> premaster
        public long Average(string columnName, SqlCondition condition)
        {
            return _database.Average<TableKey>(columnName, condition);
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public TableKey Max(string columnName)
        {
            return Max(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
=======
        public TableKey Max(string columnName)
        {
            return Max(columnName, null);
        }        
>>>>>>> premaster
        public TableKey Max(string columnName, SqlCondition condition)
        {
            return _database.Max<TableKey>(columnName, condition);
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public TableKey Min(string columnName)
        {
            return Min(columnName, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
=======
        public TableKey Min(string columnName)
        {
            return Min(columnName, null);
        }        
>>>>>>> premaster
        public TableKey Min(string columnName, SqlCondition condition)
        {
            return _database.Min<TableKey>(columnName, condition);
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
=======
>>>>>>> premaster
        public decimal Sum(string columnName)
        {
            return Sum(columnName, null);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
=======
>>>>>>> premaster
        public decimal Sum(string columnName, SqlCondition condition)
        {
            return _database.Sum<TableKey>(columnName, condition);
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
=======
>>>>>>> premaster
        public void SelectAsync(BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, false, columnsToSelect);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="distinct"></param>
        /// <param name="columnsToSelect"></param>
=======
>>>>>>> premaster
        public void SelectAsync(BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAsync(null, onSelected, distinct, columnsToSelect);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
=======
>>>>>>> premaster
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, params string[] columnsToSelect)
        {
            SelectAsync(condition, onSelected, false, columnsToSelect);
        }
<<<<<<< HEAD
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
=======
        public void SelectAsync(SqlCondition condition, BasicAction<TableKey> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var table = _database.Select<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(table);
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
=======
>>>>>>> premaster
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, false, columnsToSelect);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSelected"></param>
        /// <param name="distinct"></param>
        /// <param name="columnsToSelect"></param>
=======
>>>>>>> premaster
        public void SelectAllAsync(BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SelectAllAsync(null, onSelected, distinct, columnsToSelect);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onSelected"></param>
        /// <param name="columnsToSelect"></param>
=======

>>>>>>> premaster
        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, params string[] columnsToSelect)
        {
            SelectAllAsync(condition, onSelected, false, columnsToSelect);
        }
<<<<<<< HEAD
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
=======
        public void SelectAllAsync(SqlCondition condition, BasicAction<TableKey[]> onSelected, bool distinct, params string[] columnsToSelect)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var tables = _database.SelectAll<TableKey>(condition, distinct, columnsToSelect);
                onSelected?.Invoke(tables);
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onDeleted"></param>
        public void DeleteAsync(SqlCondition condition, BasicAction onDeleted)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void DeleteAsync(SqlCondition condition, BasicAction onDeleted)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var deleted = _database.Delete<TableKey>(condition);
                if (deleted)
                {
                    onDeleted?.Invoke();
                }
            });
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onDeleted"></param>
        public void DeleteAllAsync(BasicAction onDeleted)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void DeleteAllAsync(BasicAction onDeleted)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var deleted = _database.DeleteAll<TableKey>();
                if (deleted)
                {
                    onDeleted?.Invoke();
                }
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="onUpdated"></param>
        /// <param name="columnsToUpdate"></param>
        public void UpdateAllAsync(TableKey reference, BasicAction onUpdated, params string[] columnsToUpdate)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void UpdateAllAsync(TableKey reference, BasicAction onUpdated, params string[] columnsToUpdate)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var updated = _database.UpdateAll(reference, columnsToUpdate);
                if (updated)
                {
                    onUpdated?.Invoke();
                }
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCounted"></param>
=======
>>>>>>> premaster
        public void CountAsync(BasicAction<long> onCounted)
        {
            CountAsync(string.Empty, null, onCounted, false);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
=======
>>>>>>> premaster
        public void CountAsync(BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(string.Empty, null, onCounted, distinct);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
=======
>>>>>>> premaster
        public void CountAsync(SqlCondition condition, BasicAction<long> onCounted)
        {
            CountAsync(string.Empty, condition, onCounted, false);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
=======
>>>>>>> premaster
        public void CountAsync(SqlCondition condition, BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(string.Empty, condition, onCounted, distinct);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onCounted"></param>
=======
>>>>>>> premaster
        public void CountAsync(string columnName, BasicAction<long> onCounted)
        {
            CountAsync(columnName, null, onCounted, false);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
=======
>>>>>>> premaster
        public void CountAsync(string columnName, BasicAction<long> onCounted, bool distinct)
        {
            CountAsync(columnName, null, onCounted, distinct);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
<<<<<<< HEAD
=======
=======
>>>>>>> premaster
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted)
        {
            CountAsync(columnName, condition, onCounted, false);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onCounted"></param>
        /// <param name="distinct"></param>
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted, bool distinct)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void CountAsync(string columnName, SqlCondition condition, BasicAction<long> onCounted, bool distinct)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var count = _database.Count<TableKey>(columnName, condition, distinct);
                onCounted?.Invoke(count);
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onExist"></param>
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var count = _database.Count<TableKey>(condition, false);
                onExist?.Invoke(count > 0);
            });
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onExist"></param>
        /// <param name="distinct"></param>
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist, bool distinct)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void ExistAsync(SqlCondition condition, BasicAction<bool> onExist, bool distinct)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var count = _database.Count<TableKey>(condition, distinct);
                onExist?.Invoke(count > 0);
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onAverage"></param>
=======
>>>>>>> premaster
        public void AverageAsync(string columnName, BasicAction<long> onAverage)
        {
            AverageAsync(columnName, null, onAverage);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onAverage"></param>
        public void AverageAsync(string columnName, SqlCondition condition, BasicAction<long> onAverage)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void AverageAsync(string columnName, SqlCondition condition, BasicAction<long> onAverage)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var average = _database.Average<TableKey>(columnName, condition);
                onAverage?.Invoke(average);
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onMax"></param>
=======
>>>>>>> premaster
        public void MaxAsync(string columnName, BasicAction<TableKey> onMax)
        {
            MaxAsync(columnName, null, onMax);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onMax"></param>
        public void MaxAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMax)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void MaxAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMax)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var max = _database.Max<TableKey>(columnName, condition);
                onMax?.Invoke(max);
            });            
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onMin"></param>
=======
>>>>>>> premaster
        public void MinAsync(string columnName, BasicAction<TableKey> onMin)
        {
            MinAsync(columnName, null, onMin);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onMin"></param>
        public void MinAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMin)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void MinAsync(string columnName, SqlCondition condition, BasicAction<TableKey> onMin)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var min = _database.Min<TableKey>(columnName, condition);
                onMin?.Invoke(min);
            });            
        }

<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="onSum"></param>
=======
>>>>>>> premaster
        public void SumAsync(string columnName, BasicAction<decimal> onSum)
        {
            SumAsync(columnName, null, onSum);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <param name="onSum"></param>
        public void SumAsync(string columnName, SqlCondition condition, BasicAction<decimal> onSum)
        {
            SqlManager.EnqueueAsyncOperation(() => {
=======
        public void SumAsync(string columnName, SqlCondition condition, BasicAction<decimal> onSum)
        {
            SqlManager.PoolAsyncOperation(() => {
>>>>>>> premaster
                var sum = _database.Sum<TableKey>(columnName, condition);
                onSum?.Invoke(sum);
            });            
        }
    }
}
