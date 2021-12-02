using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Link a <see cref="Table"/> object to a <see cref="Database"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableLink : Attribute
    {
        public readonly string DatabaseName;
        public readonly string TableName;
        public readonly Type DatabaseType;
        
        public TableLink(string tableName, string databaseName)
        {
            DatabaseName = databaseName;
            TableName = tableName;
        }
        public TableLink(string tableName, Type databaseType)
        {
            DatabaseType = databaseType;
            TableName = tableName;
        }
    }
}