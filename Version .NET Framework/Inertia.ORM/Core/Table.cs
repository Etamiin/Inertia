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
        public abstract string TableName { get; }
        /// <summary>
        /// Get the <see cref="Database"/> attached
        /// </summary>
        public Database Database { get; internal set; }

        #endregion

        #region Internal variables

        [IgnoreField]
        internal bool IgnoreCreation;
        [IgnoreField]
        internal FieldInfo[] Fields;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance
        /// </summary>
        public Table()
        {
            var attachment = GetType().GetCustomAttribute<DatabaseAttach>();
            if (attachment == null)
                return;

            IgnoreCreation = GetType().GetCustomAttribute<IgnoreTableCreation>() != null;
            Database = attachment.GetDatabase();

            if (Database == null)
                throw new InvalidDatabaseAttachException(attachment.DatabaseType);

            var fields = GetType().GetFields().ToList();
            if (fields.Count == 0)
                throw new NoFieldsException(this);

            fields.RemoveAll((field) =>
                field.IsStatic ||
                field.GetCustomAttribute<IgnoreField>() != null ||
                FieldType.GetFieldType(field.FieldType).Code == TypeCode.Object);

            Fields = fields.ToArray();
        }

        #endregion

        /// <summary>
        /// Create the current <see cref="Table"/> in the attached <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public bool Create()
        {
            return SqlQuery<Table>
                .CreateTable(this)
                .Execute();
        }
        /// <summary>
        /// Create the current <see cref="Table"/> instance asynchronously in the attached <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public void CreateAsync(BasicAction<bool> callback = null)
        {
            SqlQuery<Table>
                .CreateTable(this)
                .ExecuteAsync((success, result) => callback?.Invoke(success));
        }

        /// <summary>
        /// Delete the current <see cref="Table"/> instance in the attached <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public bool Delete()
        {
            return SqlQuery<Table>
                .DeleteTable(this)
                .Execute();
        }
        /// <summary>
        /// Delete the current <see cref="Table"/> instance asynchronously in the attached <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public void DeleteAsync(BasicAction<bool> callback = null)
        {
            SqlQuery<Table>
                .DeleteTable(this)
                .ExecuteAsync((success, result) => callback?.Invoke(success));
        }

        /// <summary>
        /// Insert a new record of type <see cref="Table"/> in the <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public bool Insert()
        {
            return SqlQuery<Table>
                .Insert(this)
                .Execute();
        }
        /// <summary>
        /// Insert a new record of type <see cref="Table"/> in the <see cref="Inertia.ORM.Database"/>
        /// </summary>
        public bool Insert(out long lastInsertedId)
        {
            return SqlQuery<Table>
                .Insert(this)
                .Execute(out lastInsertedId);
        }
        /// <summary>
        /// Insert a new record of type <see cref="Table"/> asynchronously in the <see cref="Inertia.ORM.Database"/>
        /// </summary>
        /// <param name="callback"></param>
        public void InsertAsync(BasicAction<bool> callback = null)
        {
            SqlQuery<Table>
                .Insert(this)
                .ExecuteAsync((success, result) => callback?.Invoke(success));
        }
    }
}
