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
                                    
                                    if (field.FieldType == typeof(bool))
                                        value = reader.GetBoolean(i);

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
        internal void TreatDefaultFields(BasicAction<FieldInfo, int, int, object> act, bool withBooleans = false)
        {
            var default_value = OrmHelper.InstantiateObject(GetType());

            TreatFields((field, index, length) => {
                var fieldValue = field.GetValue(this);
                var defaultFieldValue = field.GetValue(default_value);

                if (fieldValue != null && !fieldValue.Equals(field.GetValue(default_value)) ||
                    field.FieldType == typeof(bool) && withBooleans)
                    act(field, index, length, fieldValue);
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
