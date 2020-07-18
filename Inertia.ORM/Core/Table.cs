using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;
using MySql.Data.MySqlClient;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the table class
    /// </summary>
    public abstract class Table
    {
        #region Public variables

        /// <summary>
        /// Get the name of the table
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Get the <see cref="Database"/> attached
        /// </summary>
        public Database Database { get; internal set; }

        #endregion

        #region Internal variables

        internal bool IgnoreCreation;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="Table"/>
        /// </summary>
        public Table()
        {
            LoadInstance();
        }
        
        #endregion

        /// <summary>
        /// Insert the current instance values in the database
        /// </summary>
        public void Insert(BasicAction<long> onInserted = null)
        {
            SqlManager.AsyncOperation(Database, (db) => {
                return ExecuteQuery(QueryType.Insert, null, null, null, "Executing insertion query failed: {0}");
            }, (success, result) => {
                onInserted?.Invoke((long)result);
            });
        }
        /// <summary>
        /// Update all rows in the database by the current instance values
        /// </summary>
        public void UpdateAllRows(BasicAction onUpdated = null)
        {
            SqlManager.AsyncOperation(Database, (db) => {
                return ExecuteQuery(QueryType.Update, null, null, null, "Executing update query failed: {0}");
            }, (success, obj) => {
                onUpdated?.Invoke();
            });
        }
        /// <summary>
        /// Set the current instance values by the selected row based on the specified values
        /// </summary>
        /// <param name="onSelected">Callback called when selelct query is executed</param>
        /// <param name="columns">Columns to select</param>
        public void Select(BasicAction onSelected = null, params string[] columns)
        {
            SqlManager.AsyncOperation(Database, (db) => {
                using (var condition = new ConditionBuilder(Database).SetLimit(1))
                {
                    return ExecuteQuery(QueryType.Select, condition, columns, (reader) => {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var field = GetType().GetField(reader.GetName(i));
                                if (field != null)
                                {
                                    var value = reader.GetValue(i);
                                    if (value.GetType() == typeof(DBNull))
                                        value = null;

                                    field.SetValue(this, value);
                                }
                            }
                        }
                    }, "Executing select query failed: {0}");
                }
            }, (success, obj) => {
                onSelected?.Invoke();
            });
        }
        /// <summary>
        /// Delete rows based on the current instance values
        /// </summary>
        /// <param name="onDeleted">Callback called when delete query is executed</param>
        /// <param name="limit">Number of rows to delete</param>
        public void Delete(BasicAction onDeleted = null, int limit = 1)
        {
            SqlManager.AsyncOperation(Database, (db) => {
                using (var condition = new ConditionBuilder(Database).SetLimit(limit))
                    return ExecuteQuery(QueryType.DeleteWithAutoCondition, condition, null, null, "Executing delete query failed: {0}");
            }, (success, obj) => {
                onDeleted?.Invoke();
            });
        }
        /// <summary>
        /// Get the number of rows that have the same values as the current instance
        /// </summary>
        /// <param name="onCounted">Callback called when count query is executed</param>
        /// <param name="limit">Limit to set for counting</param>
        /// <param name="columnName">Column to focus (or all if not specified)</param>
        /// <returns>Number of rows</returns>
        public void Count(BasicAction<long> onCounted, int limit = -1, string columnName = "*")
        {
            SqlManager.AsyncOperation(Database, (db) => {
                var count = 0L;

                using (var condition = new ConditionBuilder(Database))
                {
                    condition.SetLimit(limit);

                    ExecuteQuery(QueryType.Count, condition, columnName, (reader) => {
                        if (reader.Read())
                            count = (long)reader.GetValue(0);
                    }, "Executing count query failed: {0}");
                }

                return count;
            }, (success, obj) => {
                onCounted((long)obj);
            });
        }
        /// <summary>
        /// Return true if the current instance values exist in the table
        /// </summary>
        /// <returns></returns>
        public void Exist(BasicAction<bool> onChecked)
        {
            Count((count) => onChecked(count > 0), 1);
        }

        internal void LoadInstance()
        {
            var attachment = GetType().GetCustomAttribute<DatabaseAttach>();
            if (attachment == null)
                return;

            IgnoreCreation = GetType().GetCustomAttribute<IgnoreTableCreation>() != null;
            Database = Database.GetDatabase(attachment.DatabaseType);

            if (Database == null)
                throw new InvalidDatabaseAttachException(attachment.DatabaseType);
        }
        internal void TreatFields(BasicAction<FieldInfo, int, int> act)
        {
            var fields = GetType().GetFields();
            if (fields.Length == 0)
                throw new NoFieldsException(this);

            for (var i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsStatic)
                    continue;
                var ignore = fields[i].GetCustomAttribute<IgnoreField>() != null;
                if (ignore)
                    continue;

                var type = FieldType.GetFieldType(fields[i].FieldType);
                if (type.Code == TypeCode.Object)
                    continue;

                act(fields[i], i, fields.Length);
            }
        }
        internal void TreatDefaultFields(BasicAction<FieldInfo, int, int, object> act)
        {
            var default_value = OrmHelper.InstantiateObject(GetType());

            TreatFields((field, index, length) => {
                var value = field.GetValue(this);
                if (value != null && !value.Equals(field.GetValue(default_value)))
                    act(field, index, length, value);
            });
        }
        internal long ExecuteQuery(QueryType queryType, ConditionBuilder condition, object data, BasicAction<MySqlDataReader> onReader, string errorMessage)
        {
            var insertedId = (long)0;

            try
            {
                var builder = new QueryBuilder(this, queryType, condition, data);

                builder.Command.CommandText = builder.GetQuery();

                if (onReader != null)
                {
                    using (var reader = builder.Command.ExecuteReader())
                        onReader(reader);
                }
                else
                    builder.Command.ExecuteNonQuery();

                insertedId = builder.Command.LastInsertedId;
                builder.Dispose();
            }
            catch (Exception ex)
            {
                BaseLogger.DefaultLogger.Log(errorMessage, ex);
            }

            return insertedId;
        }
    }
}
