using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Link a <see cref="Table"/> object to a <see cref="Database"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableLink : Attribute
    {
        public string TableName => _tbName;
        public Type DatabaseType => _dbType;

        private string _tbName;
        private Type _dbType;

        public TableLink(string tableName, Type databaseType)
        {
            _tbName = tableName;
            _dbType = databaseType;
        }
    }
}