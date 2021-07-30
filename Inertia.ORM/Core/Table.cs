using System;
using System.Reflection;

namespace Inertia.ORM
{
    /// <summary>
    /// SQL Table
    /// </summary>
    public abstract class Table
    {
        /// <summary>
        /// Returns the name of the table
        /// </summary>
        public abstract string Identifier { get; }
        /// <summary>
        /// Returns the <see cref="Database"/> attached
        /// </summary>
        public Database Database { get; internal set; }

        /// <summary>
        /// Instantiate a new instance of class <see cref="Table"/>
        /// </summary>
        public Table()
        {
            var attachTo = GetType().GetCustomAttribute<AttachTo>();
            if (attachTo == null) return;

            Database = SqlManager.TrySearchDatabase(attachTo.DatabaseName);
            if (Database == null)
                throw new ArgumentNullException($"The database '{ attachTo.DatabaseName }' isn't registered");
        }

        /// <summary>
        /// Insert the current instance values in <see cref="Database"/>
        /// </summary>
        /// <returns></returns>
        public long Insert()
        {
            using (var cmd = Database.CreateCommand())
            {
                cmd.SetQuery(QueryBuilder.GetInsertQuery(this, cmd));
                cmd.ExecuteNonQuery();

                return cmd.LastInsertedId;
            }
        }
        /// <summary>
        /// Update all elements in the <see cref="Database"/> with current instance's values using the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns></returns>
        public bool Update(SqlCondition condition, params string[] columnsToUpdate)
        {
            using (var cmd = Database.CreateCommand())
            {
                cmd.SetQuery(QueryBuilder.GetUpdateQuery(this, cmd, condition, columnsToUpdate), condition);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        internal static FieldInfo[] GetFields<T>() where T : Table
        {
            return GetFields(typeof(T));
        }
        internal static FieldInfo[] GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length == 0) return new FieldInfo[0];

            return Array.FindAll(fields, (f) => {
                return !f.IsStatic && f.GetCustomAttribute<IgnoreField>() == null && FieldType.GetFieldType(f.FieldType).Code != TypeCode.Object;
            });
        }
    }
}