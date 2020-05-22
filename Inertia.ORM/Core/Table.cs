using System;
using System.Collections.Generic;
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
        public void Insert()
        {
            ExecuteQuery(QueryType.Insert, null, null, null, "Executing insertion query failed: {0}");
        }
        /// <summary>
        /// Update all rows in the database by the current instance values
        /// </summary>
        public void UpdateAll()
        {
            Update(null);
        }
        /// <summary>
        /// Update rows based on the specified <see cref="ConditionBuilder"/> by the current instance values
        /// </summary>
        /// <param name="condition"></param>
        public void Update(ConditionBuilder condition)
        {
            ExecuteQuery(QueryType.Update, condition, null, null, "Executing update query failed: {0}");
        }
        /// <summary>
        /// Set the current instance values by the selected row based on the specified values
        /// </summary>
        /// <param name="columns">Columns to select</param>
        public void Select(params string[] columns)
        {
            using (var condition = new ConditionBuilder(Database).SetLimit(1))
            {
                ExecuteQuery(QueryType.Select, condition, columns, (reader) => {
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
        }
        /// <summary>
        /// Delete rows based on the current instance values
        /// </summary>
        /// <param name="limit">Number of rows to delete</param>
        public void Delete(int limit = 1)
        {
            using (var condition = new ConditionBuilder(Database).SetLimit(limit))
                ExecuteQuery(QueryType.DeleteWithAutoCondition, condition, null, null, "Executing delete query failed: {0}");
        }
        /// <summary>
        /// Get the number of rows that have the same values as the current instance
        /// </summary>
        /// <param name="columnName">Column to focus (or all if not specified)</param>
        /// <returns>Number of rows</returns>
        public long Count(string columnName = "*")
        {
            var count = 0L;

            using (var condition = new ConditionBuilder(Database))
            {
                ExecuteQuery(QueryType.Count, condition, columnName, (reader) => {
                    if (reader.Read())
                        count = (long)reader.GetValue(0);
                }, "Executing count query failed: {0}");
            }

            return count;
        }
        /// <summary>
        /// Return true if the current instance values exist in the table
        /// </summary>
        /// <returns></returns>
        public bool Exist()
        {
            return Count() > 0;
        }

        internal void LoadInstance()
        {
            var attachment = GetType().GetCustomAttribute<DatabaseAttach>();
            if (attachment == null)
                return;

            IgnoreCreation = GetType().GetCustomAttribute<IgnoreTableCreation>() != null;
            Database = Database.GetDatabase(attachment.Database);

            if (Database == null)
                throw new InvalidDatabaseAttachException(attachment.Database);
        }
        internal void TreatFields(SimpleAction<FieldInfo, int, int> act)
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
        internal void TreatDefaultFields(SimpleAction<FieldInfo, int, int, object> act)
        {
            var default_value = OrmHelper.InstantiateObject(GetType());

            TreatFields((field, index, length) => {
                var value = field.GetValue(this);
                if (value != null && !value.Equals(field.GetValue(default_value)))
                    act(field, index, length, value);
            });
        }
        internal void ExecuteQuery(QueryType queryType, ConditionBuilder condition, object data, SimpleAction<MySqlDataReader> onReader, string errorMessage)
        {
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

                builder.Dispose();
            }
            catch (Exception ex)
            {
                BaseLogger.DefaultLogger.Log(errorMessage, ex);
            }
        }
    }
}
