using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.ORM;
using MySql.Data.MySqlClient;

namespace Inertia.Internal
{
    internal class QueryBuilder : IDisposable
    {
        #region Internal variables

        internal MySqlCommand Command;

        #endregion

        #region Private variables

        private StringBuilder m_builder;
        private ConditionBuilder m_condition;
        private Table m_table;
        private readonly QueryType m_queryType;
        private object m_data;

        #endregion

        #region Constructors

        public QueryBuilder(Table table, QueryType queryType, object data = null) : this(table, queryType, null, data)
        {
        }
        public QueryBuilder(Table table, QueryType queryType, ConditionBuilder condition, object data = null)
        {
            m_builder = new StringBuilder();
            m_condition = condition;
            m_table = table;
            m_queryType = queryType;
            m_data = data;

            if (m_condition == null)
                Command = m_table.Database.CreateCommand();
            else
                Command = m_condition.Command;
        }

        #endregion

        public string GetQuery()
        {
            m_builder.Clear();

            switch (m_queryType)
            {
                case QueryType.CreateTable:
                    CreateTable();
                    break;
                case QueryType.DropTable:
                    DropTable();
                    break;
                case QueryType.Insert:
                    Insert();
                    break;
                case QueryType.Update:
                    Update();
                    break;
                case QueryType.Select:
                    Select();
                    break;
                case QueryType.Delete:
                    Delete();
                    break;
                case QueryType.DeleteWithAutoCondition:
                    DeleteWithAutoCondition();
                    break;
                case QueryType.DeleteAll:
                    DeleteAll();
                    break;
                case QueryType.Count:
                    Count();
                    break;
                default:
                    throw new NotImplementedException();
            }

            var query = m_builder.ToString();
            if (m_condition != null && m_condition.Length > 0)
                query += " " + m_condition.GetQuery();

            return query;
        }

        public void Dispose()
        {
            if (m_condition != null && Command == m_condition.Command)
                Command.Dispose();

            m_table = null;
            m_data = null;
            m_condition = null;
            m_builder = null;
            Command = null;
        }

        private void CreateTable()
        {
            var primaryKeys = string.Empty;

            m_builder.Append("CREATE TABLE IF NOT EXISTS `" + m_table.Name + "` (");
            m_table.TreatFields((field, index, length) => {
                if (index > 0 && index < length)
                    m_builder.Append(",");

                m_builder.Append("`" + field.Name + "` ");

                var fieldType = FieldType.GetFieldType(field.FieldType);
                var varChar = field.GetCustomAttribute<VarChar>();

                if (fieldType.SqlType.Equals("text") && varChar != null)
                    m_builder.Append("VARCHAR(" + varChar.Length + ")");
                else
                    m_builder.Append(fieldType.SqlType.ToUpper() + (fieldType.Unsigned ? " UNSIGNED" : string.Empty));

                var primaryKey = field.GetCustomAttribute<PrimaryKey>();
                if (primaryKey != null)
                {
                    if (!string.IsNullOrEmpty(primaryKeys))
                        primaryKeys += ",";

                    primaryKeys += "`" + field.Name + "`";
                    if (primaryKey.AutoIncrement)
                        m_builder.Append(" AUTO_INCREMENT");
                }
            });

            if (!string.IsNullOrEmpty(primaryKeys))
                m_builder.Append(", primary key(" + primaryKeys + ")");

            m_builder.Append(")");
        }
        private void DropTable()
        {
            m_builder.Append("DROP TABLE " + m_table.Name);
        }
        private void Insert()
        {
            var values = string.Empty;

            m_builder.Append("INSERT INTO `" + m_table.Name + "`");
            m_table.TreatFields((field, index, length) => {
                var value = field.GetValue(m_table);
                values += field.Name;

                Command.Parameters.AddWithValue("@" + field.Name, value);

                if (index < length - 1)
                    values += ",";
            });

            m_builder.Append("(" + values + ") VALUES (@" + values.Replace(",", ",@") + ")");
        }
        private void Update()
        {
            var default_value = OrmHelper.InstantiateObject(m_table.GetType());
            var hasFields = false;

            m_builder.Append("UPDATE `" + m_table.Name + "` SET ");
            m_table.TreatDefaultFields((field, index, length, value) => {
                var paramName = OrmHelper.GetRandomName(7);

                m_builder.Append(field.Name + "=" + paramName);
                Command.Parameters.AddWithValue(paramName, value);

                if (index < length - 1)
                    m_builder.Append(",");

                hasFields = true;
            });

            if (m_builder[m_builder.Length - 1] == ',')
                m_builder.Remove(m_builder.Length - 1, 1);

            if (!hasFields)
                throw new NoFieldsException(m_table);
        }
        private void Select()
        {
            var columns = (string[])m_data;
            m_builder.Append("SELECT ");
            if (columns.Length > 0)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    m_builder.Append(columns[i]);
                    if (i < columns.Length - 1)
                        m_builder.Append(",");
                }
            }
            else
                m_builder.Append("*");

            m_builder.Append(" FROM `" + m_table.Name + "`");
            QueryWithAutoCondition();
        }
        private void Delete()
        {
            m_builder.Append("DELETE FROM `" + m_table.Name + "`");
        }
        private void DeleteWithAutoCondition()
        {
            m_builder.Append("DELETE FROM `" + m_table.Name + "`");
            QueryWithAutoCondition();
        }
        private void DeleteAll()
        {
            m_builder.Append("TRUNCATE TABLE `" + m_table.Name + "`");
        }
        private void Count()
        {
            var columnName = (string)m_data;
            m_builder.Append("SELECT COUNT(" + columnName + ") FROM `" + m_table.Name + "`");

            QueryWithAutoCondition();
        }

        private void QueryWithAutoCondition()
        {
            if (m_condition == null)
                return;

            var default_value = OrmHelper.InstantiateObject(m_table.GetType());
            m_table.TreatDefaultFields((field, index, length, value) => {
                m_condition.AddCondition(field.Name, value, ConditionOperator.Equal);
            });
        }
    }
}
