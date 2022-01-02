using System;
using System.Reflection;

namespace Inertia.ORM
{
    public abstract class Table
    {
        internal static FieldInfo[] GetFields<T>() where T : Table
        {
            return GetFields(typeof(T));
        }
        internal static FieldInfo[] GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length == 0)
            {
                return new FieldInfo[0];
            }

            return Array.FindAll(fields, (f) => {
                return !f.IsStatic && f.GetCustomAttribute<IgnoreField>() == null && FieldType.GetFieldType(f.FieldType).Code != TypeCode.Object;
            });
        }

        /// <summary>
        /// Returns the <see cref="Database"/> attached
        /// </summary>
        public Database Database { get; internal set; }

        internal string Identifier { get; private set; }

        protected Table()
        {
            var link = GetType().GetCustomAttribute<TableLink>(false);
            if (link != null)
            {
                Identifier = link.TableName;

                if (link.DatabaseType != null && SqlManager.TrySearchDatabase(link.DatabaseType, out Database db))
                {
                    Database = db;
                }

                if (Database == null)
                {
                    throw new ArgumentNullException($"The database isn't registered for table '{ link.TableName }'");
                }
            }
        }

        /// <summary>
        /// Insert the current instance values in <see cref="Database"/>
        /// </summary>
        /// <returns></returns>
        public long Insert()
        {
            long id = 0;

            Database.ExecuteCommand((cmd) => {
                cmd.SetQuery(QueryBuilder.GetInsertQuery(this, cmd));
                cmd.ExecuteNonQuery();

                id = cmd.LastInsertedId;
            });

            return id;
        }
        /// <summary>
        /// Insert the current instance values in <see cref="Database"/>
        /// </summary>
        /// <returns></returns>
        public void InsertAsync(BasicAction<long> onResult)
        {
            SqlManager.PoolAsyncOperation(() => {
                onResult(Insert());
            });
        }
        /// <summary>
        /// Update all elements in the <see cref="Database"/> with current instance's values using the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns></returns>
        public bool Update(SqlCondition condition, params string[] columnsToUpdate)
        {
            var updated = false;

            Database.ExecuteCommand((cmd) => {
                cmd.SetQuery(QueryBuilder.GetUpdateQuery(this, cmd, condition, columnsToUpdate), condition);
                updated = cmd.ExecuteNonQuery() > 0;
            });

            return updated;
        }
        /// <summary>
        /// Update all elements in the <see cref="Database"/> with current instance's values using the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="onUpdated"></param>
        /// <param name="columnsToUpdate"></param>
        public void UpdateAsync(SqlCondition condition, BasicAction<bool> onUpdated, params string[] columnsToUpdate)
        {
            SqlManager.PoolAsyncOperation(() => {
                onUpdated(Update(condition, columnsToUpdate));
            });
        }
    }
}