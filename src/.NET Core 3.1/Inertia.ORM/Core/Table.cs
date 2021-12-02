using System;
using System.Reflection;

namespace Inertia.ORM
{
<<<<<<< HEAD
    /// <summary>
    /// SQL Table
    /// </summary>
    public abstract class Table
    {
        /// <summary>
        /// Returns the name of the table
        /// </summary>
        public abstract string Identifier { get; }
=======
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

>>>>>>> premaster
        /// <summary>
        /// Returns the <see cref="Database"/> attached
        /// </summary>
        public Database Database { get; internal set; }

<<<<<<< HEAD
        /// <summary>
        /// Instantiate a new instance of class <see cref="Table"/>
        /// </summary>
        protected Table()
        {
            var attachTo = GetType().GetCustomAttribute<AttachTo>();
            if (attachTo != null)
            {
                if (!string.IsNullOrEmpty(attachTo.DatabaseName))
                {
                    if (SqlManager.TrySearchDatabase(attachTo.DatabaseName, out Database db))
                    {
                        this.Database = db;
                    }
                }
                else if (attachTo.DatabaseType != null)
                {
                    if (SqlManager.TrySearchDatabase(attachTo.DatabaseType, out Database db))
                    {
                        this.Database = db;
                    }
                }

                if (this.Database == null)
                {
<<<<<<< HEAD
                    throw new ArgumentNullException($"The database '{ attachTo.DatabaseName }' isn't registered");
=======
                    throw new ArgumentNullException($"The database isn't registered for table '{ Identifier }'");
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
        internal string Identifier { get; private set; }

        protected Table()
        {
            var link = GetType().GetCustomAttribute<TableLink>(false);
            if (link != null)
            {
                Identifier = link.TableName;

                if (!string.IsNullOrEmpty(link.DatabaseName))
                {
                    if (SqlManager.TrySearchDatabase(link.DatabaseName, out Database db))
                    {
                        Database = db;
                    }
                }
                else if (link.DatabaseType != null)
                {
                    if (SqlManager.TrySearchDatabase(link.DatabaseType, out Database db))
                    {
                        Database = db;
                    }
                }

                if (Database == null)
                {
                    throw new ArgumentNullException($"The database isn't registered for table '{ link.TableName }'");
>>>>>>> premaster
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
<<<<<<< HEAD

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
=======
>>>>>>> premaster
    }
}